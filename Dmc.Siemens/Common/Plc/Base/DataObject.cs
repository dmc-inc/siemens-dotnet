using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Common.Plc.Types;
using Dmc.Siemens.Portal.Base;
using Dmc.Siemens.Portal.Plc;
using Dmc.Wpf;

namespace Dmc.Siemens.Common.Plc
{
    public abstract class DataObject : NotifyPropertyChanged, IAutomationObject, IEnumerable<DataEntry>
    {

		private string _name;
        public string Name
        {
            get => this._name;
            set => this.SetProperty(ref this._name, value);
        }

        private DataType _dataType;
        public virtual DataType DataType
        {
            get => this._dataType;
            set => this.SetProperty(ref this._dataType, value);
        }

        private string _dataTypeName;
		public string DataTypeName
        {
            get => this._dataTypeName;
            set => this.SetProperty(ref this._dataTypeName, value);
        }

        private Address? _address;
		public Address? Address
        {
            get => this._address;
            set => this.SetProperty(ref this._address, value);
        }

        private string _comment;
        public string Comment
        {
            get => this._comment;
            set => this.SetProperty(ref this._comment, value);
        }

        private object _startValue;
		public object StartValue
        {
            get => this._startValue;
            set => this.SetProperty(ref this._startValue, value);
        }

        public virtual bool IsPrimitiveDataType => TagHelper.IsPrimitive(this.DataType);

        public DataObject(string name = "", DataType dataType = DataType.UNKNOWN, string comment = null, IEnumerable<DataEntry> children = null)
        {
            this.Name = name;
            this.DataType = dataType;
            this.Comment = comment;
            this.Children = (children != null) ? new LinkedList<DataEntry>(children) : null;
        }

        public LinkedList<DataEntry> Children { get; set; }

		public void CalcluateAddresses(PortalPlc plc)
		{
			// make sure we have a valid structure if we are a UDT
			this.ResolveUdt(plc);

			// if we don't have any children, there is nothing to calculate
			if (this.Children?.Count <= 0)
				return;

			// start at with the first child and offset 0
			var currentObject = this.Children.First;
			var addressOffset = new Address();
			do
			{
				// set the next child's address
				currentObject.Value.Address = addressOffset;
				// add the currentObject size to the current offset
				addressOffset += currentObject.Value.CalculateSize(plc);
				// increment the next address offset if necessary
				addressOffset = DataObject.IncrementAddressOfChild(addressOffset, currentObject);
			} while ((currentObject = currentObject.Next) != null);
		}

		public virtual Address CalculateSize(PortalPlc plc)
		{
			var isPrimitive = this.IsPrimitiveDataType;
			// If we are a pimitive, return the size directly
			if (isPrimitive && this.DataType != DataType.STRING)
			{
				var size = TagHelper.GetPrimitiveByteSize(this.DataType);
				if (size == 0) // Check for the case that is a boolean
					return new Address(0, 1);
				else
					return new Address(size);
			}
			// if we are not a primitive, we need to add up the size of all the children
			else if (!isPrimitive)
			{
				// check for an array type - we do not know how to calculate this
				if (this.DataType == DataType.ARRAY)
					throw new SiemensException("DataObject does not know how to calulate the size of an array.  Override CalculateSize().");

				// make sure if we are a UDT that our structure is valid
				this.ResolveUdt(plc);

				// make sure the struct/UDT has valid children
				if (this.Children?.Count <= 0)
					throw new SiemensException("Struct does not have any valid children: " + this.Name);

				// now we loop through all of the Children and add them together
				var currentObject = this.Children.First;
				var sumAddress = new Address();
				do
				{
					sumAddress += currentObject.Value.CalculateSize(plc);
					// potentially increment the address based on the next object
					sumAddress = DataObject.IncrementAddressOfChild(sumAddress, currentObject);

				} while ((currentObject = currentObject.Next) != null);

				// return our calculated sum
				return sumAddress;
			}
			// At this point, the only option left is a string data type
			// This object does not know how to handle strings, so throw an exception
			else
			{
				throw new SiemensException("DataObject does not know how to calculate the size of a string.  Override CalculateSize().");
			}
		}

		public void SetUdtStructure(IEnumerable<DataEntry> children)
		{
			if (this.DataType == DataType.UDT)
			{
				this.Children = new LinkedList<DataEntry>(children);
			}
		}

		IEnumerator<DataEntry> IEnumerable<DataEntry>.GetEnumerator()
		{
			return this.Children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Children.GetEnumerator();
		}

		private static Address IncrementAddressOfChild(Address currentAddress, LinkedListNode<DataEntry> child)
		{
			var nextAddress = currentAddress;

			// if this is the last child object, make sure to round up to the nearest word
			if (child.Next == null)
			{
				return TagHelper.IncrementAddress(currentAddress);
			}

			// if this is not the last child, increment depending on the next data type
			var nextDataType = child.Next.Value.DataType;
			switch (child.Value.DataType)
			{
				case DataType.BOOL:
					// if the next object is a byte, only increment to the next byte
					if (nextDataType == DataType.BYTE || nextDataType == DataType.CHAR)
					{
						nextAddress = TagHelper.IncrementAddress(currentAddress, isByte: true);
					}
					// if the next object is a word or more, increment to the next word
					else if (nextDataType != DataType.BOOL)
					{
						nextAddress = TagHelper.IncrementAddress(currentAddress);
					}
					// if the next object is a byte, don't increment at all
					break;
				case DataType.BYTE:
				case DataType.CHAR:
					// only if the next data type is a word or larger, increment to the next word
					if (nextDataType != DataType.BOOL && nextDataType != DataType.BYTE && nextDataType != DataType.CHAR)
					{
						nextAddress = TagHelper.IncrementAddress(currentAddress);
					}
					break;
				default:
					// if this is a word or larger, always make sure you're at a word address increment
					nextAddress = TagHelper.IncrementAddress(currentAddress);
					break;
			}

			return nextAddress;
		}

		private void ResolveUdt(IPlc plc)
		{
			// only do this if we are a UDT
			if (this.DataType == DataType.UDT)
			{
				// Plc cannot be null for a UDT whose structure has not been defined
				if (plc == null)
					throw new ArgumentNullException(nameof(plc), "Plc cannot be null when calculating the size of a UDT");

				var udt = plc.UserDataTypes.FirstOrDefault(u => u.Name == this.DataTypeName);
				if (udt == null)
					throw new SiemensException("Specified PLC does not contain UDT \"" + this.DataTypeName + "\"");

				this.SetUdtStructure(udt);
			}
		}

	}
}
