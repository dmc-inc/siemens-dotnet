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
    public class DataEntry : DataObject
    {

		#region Constructors

		public DataEntry(string name = "", DataType dataType = DataType.UNKNOWN, string comment = null,
			IEnumerable<DataEntry> children = null, string dataTypeName = null, Constant<int> stringLength = default,
			DataEntry arrayDataEntry = null, Constant<int> arrayStartIndex = default, Constant<int> arrayEndIndex = default)
			: base(name, dataType, comment, children)
		{
			this.DataTypeName = dataTypeName;
			this.StringLength = stringLength;
			this.ArrayDataEntry = arrayDataEntry;
			this.ArrayStartIndex = arrayStartIndex;
			this.ArrayEndIndex = arrayEndIndex;
		}

		#endregion

		#region Public Properties

		private DataEntry _arrayDataEntry;
		public DataEntry ArrayDataEntry
        {
            get => this._arrayDataEntry;
            set => this.SetProperty(ref this._arrayDataEntry, value);
        }

        public Constant<int> ArrayStartIndex { get; set; }
        public Constant<int> ArrayEndIndex { get; set; }
        public Constant<int> StringLength { get; set; }
		
		public string DataTypeString
		{
			get
			{
				switch (this.DataType)
				{
					case DataType.ANY:
						return "Any";
					case DataType.ARRAY:
						var arrayStart = this.ArrayStartIndex.HasValue ? this.ArrayStartIndex.Value.ToString() : $"\"{this.ArrayStartIndex.Name}\"";
						var arrayEnd = this.ArrayEndIndex.HasValue ? this.ArrayEndIndex.Value.ToString() : $"\"{this.ArrayEndIndex.Name}\"";
						return $"Array[{arrayStart}..{arrayEnd}] of {this.ArrayDataEntry.DataTypeString}";
					case DataType.BOOL:
						return "Bool";
					case DataType.BYTE:
						return "Byte";
					case DataType.CHAR:
						return "Char";
					case DataType.DATE:
						return "Date";
					case DataType.DATE_AND_TIME:
						return "Date_And_Time";
					case DataType.DINT:
						return "Dint";
					case DataType.DWORD:
						return "DWord";
					case DataType.INT:
						return "Int";
					case DataType.REAL:
						return "Real";
					case DataType.STRING:
						if (this.StringLength.HasValue)
							if (this.StringLength.Value == 254)
								return "String";
							else
								return $"String[{this.StringLength.Value}]";
						else
							return $"String[\"{this.StringLength.Name}\"";
					case DataType.STRUCT:
						return "Struct";
					case DataType.TIME:
						return "Time";
					case DataType.TIME_OF_DAY:
						return "Time_Of_Day";
					case DataType.UDT:
						return $"\"{this.DataTypeName}\"";
					case DataType.WORD:
						return "Word";
					default:
						throw new NotSupportedException("Data type: " + this.DataType.ToString() + " is not supported");
				}
			}
		}

		#endregion

		#region Public Methods

		public static DataEntry FromString(string dataEntry, TextReader dataReader)
        {

			DataEntry newEntry;

            var trimmedData = dataEntry.Trim();
            var type = string.Empty;
			var name = string.Empty;
			var comment = string.Empty;
			string[] splitString;

            if (trimmedData.Contains("//"))
            {
                splitString = trimmedData.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                comment = splitString[1].Trim();
                trimmedData = splitString[0];
            }
			if (trimmedData.Contains('{'))
			{
				// This removes anything in between {} characters
				trimmedData = Regex.Replace(trimmedData, "\\{.*\\}", "");
			}
            if (trimmedData.Contains(':'))
            {
                splitString = trimmedData.Split(':');
                type = splitString[1].Trim().Trim(';');
                name = splitString[0].Trim().Trim('"');
            }

			newEntry = TagHelper.ParseDataEntry(type, dataReader);
			newEntry.Name = name;
			newEntry.Comment = comment;

			return newEntry;

        }

		public override Address CalculateSize(PortalPlc plc)
		{
			// we only need to override size calculations on string and array
			// DataObject knows how to calculate the size of everything else
			switch (this.DataType)
			{
				case DataType.ARRAY:
					// check to make sure we have a valid ArrayDataEntry
					if (this.ArrayDataEntry == null || this.ArrayDataEntry.DataType == DataType.UNKNOWN)
						throw new SiemensException("Array '" + this.Name + "' does not have a valid ArrayDataType");

					// 2D arrays are not yet supported by Siemens
					if (this.ArrayDataEntry.DataType == DataType.ARRAY)
						throw new SiemensException("Siemens does not support 2D arrays. Array '" + this.Name + "' is invalid.");

					// check to make sure we are able to get the indices of the array
					if ((!this.ArrayStartIndex.HasValue || !this.ArrayEndIndex.HasValue) && plc == null)
						throw new ArgumentNullException(nameof(plc), "Plc cannot be null when the indices of an array are user defined constant values");

					// resolve the actual start and end indices
					var arrayStart = plc?.GetConstantValue(this.ArrayStartIndex) ?? this.ArrayStartIndex.Value;
					var arrayEnd = plc?.GetConstantValue(this.ArrayEndIndex) ?? this.ArrayEndIndex.Value;

					// calculate the array dimension and the size of each index
					var arraySize = arrayEnd - arrayStart + 1;
					var arraySubTypeSize = this.ArrayDataEntry.CalculateSize(plc);
					// check for the case that this is an array of bools
					if (arraySubTypeSize.Byte == 0)
					{
						var overflow = arraySize % 16;
						if (overflow > 0)
						{
							return TagHelper.IncrementAddress(new Address(arraySize / 16, overflow));
						}
						else
						{
							return new Address(arraySize / 16);
						}
					}
					// check for the case that this is an array of bytes
					else if (arraySubTypeSize.Byte == 1)
					{
						if (arraySize % 2 > 0)
						{
							return new Address(arraySize / 2 + 1);
						}
						else
						{
							return new Address(arraySize / 2);
						}
					}
					// all other cases are types of size larger than a word
					else
					{
						return new Address(arraySize * arraySubTypeSize.Byte);
					}

				case DataType.STRING:
					// first to check to make sure we are able to get the size of the string
					if (!this.StringLength.HasValue && plc == null)
						throw new ArgumentNullException(nameof(plc), "Plc cannot be null if the string length is a user defined constant value");

					return new Address(TagHelper.GetPrimitiveByteSize(this.DataType, plc?.GetConstantValue(this.StringLength) ?? this.StringLength.Value));

				default:
					return base.CalculateSize(plc);
			}
		}
		
		#endregion

	}
}
