using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common
{
	public static class TagHelper
	{

		/// <summary>
		/// Calculates the size of the DataType specified.
		/// </summary>
		/// <returns>Size in bytes, -1 if not primitive.</returns>
		public static int GetPrimitiveByteSize(DataType dataType, int stringLength = 0)
		{
			switch (dataType)
			{
				case DataType.BOOL:
					return 0;
				case DataType.BYTE:
				case DataType.CHAR:
					return 1;
				case DataType.WORD:
				case DataType.INT:
				case DataType.DATE:
				case DataType.S5TIME:
				case DataType.COUNTER:
				case DataType.TIMER:
					return 2;
				case DataType.DWORD:
				case DataType.DINT:
				case DataType.TIME:
				case DataType.REAL:
				case DataType.TIME_OF_DAY:
					return 4;
				case DataType.POINTER:
					return 6;
				case DataType.DATE_AND_TIME:
					return 8;
				case DataType.ANY:
					return 10;
				case DataType.STRING:
					return (2 + stringLength);
				default:
					return -1;
			}
		}

		public static bool IsPrimitive(DataType dataType)
		{
			switch (dataType)
			{
				case DataType.BOOL:
				case DataType.BYTE:
				case DataType.CHAR:
				case DataType.WORD:
				case DataType.INT:
				case DataType.DATE:
				case DataType.S5TIME:
				case DataType.COUNTER:
				case DataType.TIMER:
				case DataType.DWORD:
				case DataType.DINT:
				case DataType.TIME:
				case DataType.REAL:
				case DataType.TIME_OF_DAY:
				case DataType.POINTER:
				case DataType.DATE_AND_TIME:
				case DataType.ANY:
				case DataType.STRING:
					return true;
				default:
					return false;
			}
		}

		public static DataType ParseDataType(string dataTypeString)
		{
			if (dataTypeString.ToUpper().Replace(" ", string.Empty).Contains("ARRAY["))
			{
				return DataType.ARRAY;
			}
			else if (dataTypeString.Contains('"'))
			{
				return DataType.UDT;
			}
			else if (dataTypeString.ToUpper().Contains("STRING"))
			{
				return DataType.STRING;
			}
			else if (dataTypeString.ToUpper().Contains("STRUCT"))
			{
				return DataType.STRUCT;
			}
			return (Enum.TryParse(dataTypeString, out DataType type)) ? type : DataType.UNKNOWN;
		}

		public static IDictionary<DataEntry, Address> CalcluateAddresses(IDataEntry dataEntry, IPlc plc)
		{
			if (dataEntry.Children?.Count <= 0)
				return null;

			Dictionary<DataEntry, Address> addresses = new Dictionary<DataEntry, Address>();
			Address currentAddress = new Address();

			foreach (DataEntry entry in dataEntry)
			{
				Address entryAddress;
				switch (entry.DataType)
				{
					case DataType.ANY:
					case DataType.ARRAY:
					case DataType.DATE:
					case DataType.DATE_AND_TIME:
					case DataType.DINT:
					case DataType.DWORD:
					case DataType.INT:
					case DataType.POINTER:
					case DataType.REAL:
					case DataType.STRING:
					case DataType.STRUCT:
					case DataType.TIME:
					case DataType.TIME_OF_DAY:
					case DataType.UDT:
					case DataType.WORD:
						currentAddress = IncrementAddress(currentAddress);
						entryAddress = currentAddress;
						currentAddress += dataEntry.CalculateSize(plc);
						break;
					case DataType.BOOL:
						entryAddress = currentAddress;
						currentAddress = IncrementAddress(currentAddress, isBit: true);
						break;
					case DataType.BYTE:
					case DataType.CHAR:
						currentAddress = IncrementAddress(currentAddress, isByte: true);
						entryAddress = currentAddress;
						currentAddress += dataEntry.CalculateSize(null);
						break;
					default:
						entryAddress = new Address();
						break;
				}

				// Add or overwrite the current address
				addresses[entry] = entryAddress;

			}

			return addresses;
		}

		public static Address CalculateSize(IDataEntry dataEntry, IPlc plc)
		{
			// Check if it is a DataEntry (not a block) then check if it is a primitive
			int size = 0;
			Address address;
			DataEntry entry = (dataEntry as DataEntry);
			if (entry != null && (size = TagHelper.GetPrimitiveByteSize(entry.DataType, (entry.StringLength.HasValue) ? entry.StringLength.Value : 0)) >= 0)
			{
				if (size > 0)
					return new Address(size, 0);
				else
					return new Address(0, 1);
			}

			// At this point, plc cannot be null because there are non-primitives
			if (plc == null)
				throw new ArgumentNullException(nameof(plc));
			// If the DataType is not a struct, then dataEntry must be a DataEntry and therefore cannot be null after the cast attempt
			if (dataEntry.DataType != DataType.STRUCT && entry == null)
				throw new SiemensException("Cannot have a non-STRUCT IDataEntry that is not of type DataEntry");

			// Handle the cases where it is not
			switch (dataEntry.DataType)
			{
				case DataType.ARRAY:
					int arraySize = plc.GetConstantValue(entry.ArrayEndIndex) - plc.GetConstantValue(entry.ArrayStartIndex) + 1;
					IDataEntry newEntry;
					if (entry.ArrayDataType.HasValue && entry.ArrayDataType != DataType.UDT)
					{
						if (entry.ArrayDataType.Value == DataType.STRING)
						{
							newEntry = new DataEntry(dataType: DataType.STRING, stringLength: entry.StringLength);
						}
						else
						{
							newEntry = new DataEntry() { DataType = entry.ArrayDataType.Value };
						}
					}
					else if (entry.ArrayDataType == DataType.UDT)
					{
						newEntry = plc.GetUdtStructure(entry.DataTypeName);
					}
					else
					{
						throw new SiemensException("Array '" + entry.Name + "' does not have a correct sub-DataType");
					}
					address = newEntry.CalculateSize(plc);
					if (address.Byte == 0)
					{
						int overflow = arraySize % 16;
						if (overflow > 0)
						{
							return new Address((arraySize / 16) * 2 + 2);
						}
						else
						{
							return new Address(arraySize / 16);
						}
					}
					else if (address.Byte == 1)
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
					else
					{
						return new Address(arraySize * address.Byte);
					}

				case DataType.STRUCT:
					Address sumAddress = new Address();
					foreach (var subEntry in dataEntry)
					{
						sumAddress += subEntry.CalculateSize(plc);
					}
					return sumAddress;
				case DataType.UDT:
					return plc.GetUdtStructure(entry.DataTypeName).CalculateSize(plc);
				default:
					throw new SiemensException($"Invalid DataType: {dataEntry.DataType.ToString()}");
			}
		}

		public static Address IncrementAddress(Address address, bool isBit = false, bool isByte = false)
		{
			if (isBit)
			{
				// If this was a bit, check for rollover to byte and if not just increment bit
				if (address.Bit >= 7)
				{
					address = new Address(address.Byte + 1);
				}
				else
				{
					address = new Address(address.Byte, address.Bit + 1);
				}
			}
			else if (isByte)
			{
				// if its a byte, we only need to check for the case where the bit is not zero
				// in which case just increment the byte by one and reset the bit
				if (address.Bit != 0)
				{
					address = new Address(address.Byte + 1);
				}
			}
			else
			{
				// if the byte is an even number, only increment if the bit is not 0
				if (address.Byte % 2 == 0 && address.Bit != 0)
				{
					address = new Address(address.Byte + 2);
				}
				// if the byte is an odd number, add 1 byte and clear the bit
				else if (address.Byte % 2 == 1)
				{
					address = new Address(address.Byte + 1);
				}
			}

			return address;
		}

	}
}
