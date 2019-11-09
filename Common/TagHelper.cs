using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Common.Plc.Types;
using Dmc.Siemens.Portal.Base;
using Dmc.Siemens.Portal.Plc;

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
				case DataType.LREAL:
					return 8;
				case DataType.ANY:
					return 10;
				case DataType.STRING:
					return 2 + stringLength;
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
				case DataType.LREAL:
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
			return Enum.TryParse(dataTypeString, true, out DataType type) ? type : DataType.UNKNOWN;
		}

		public static DataEntry ParseDataEntry(string dataEntryString, TextReader reader = null)
		{
			var newEntry = new DataEntry();

			var type = dataEntryString.Trim();
			string[] splitString;
            var isUdt = false;

            int length;
            if (type.ToUpper().Replace(" ", string.Empty).Contains("ARRAY["))
            {
                splitString = type.Split(new string[] { "[", "..", "]" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitString.Length >= 4)
                {
                    if (int.TryParse(splitString[2], out length)) // See if the array end index is an integer
                    {
                        newEntry.ArrayEndIndex = length;
                    }
                    else // If not, the array index is a constant defined elsewhere
                    {
                        newEntry.ArrayEndIndex = new Constant<int>(splitString[2].Trim('\"'));
                    }
                    if (int.TryParse(splitString[1], out var arrayStart)) // Do the same as above for the start index
                    {
                        newEntry.ArrayStartIndex = arrayStart;
                    }
                    else
                    {
                        newEntry.ArrayStartIndex = new Constant<int>(splitString[1].Trim('\"'));
                    }

                }
                splitString = type.Replace(" of ", " OF ").Split(new string[] { " OF " }, StringSplitOptions.RemoveEmptyEntries);

                var arrayType = string.Empty;
                if (splitString.Length > 1)
                {
                    arrayType = splitString[1].Trim().Trim('"');
                }
                else if (splitString.Length == 1 && reader != null)  // Search the next line for the array type if it hasn't already been defined
                {
                    string line;
                    line = reader.ReadLine();

                    arrayType = line.Trim().Trim(';').Trim('\"');
                }

                var parsedType = TagHelper.ParseDataType(arrayType);
                if (parsedType == DataType.UNKNOWN)
                {
                    newEntry.ArrayDataEntry = new DataEntry(dataType: DataType.UDT, dataTypeName: arrayType);
                }
                else if (parsedType == DataType.STRING)
                {
                    newEntry.ArrayDataEntry = new DataEntry(dataType: DataType.STRING, stringLength: ParseStringLength(arrayType));
                }
                else
                {
                    newEntry.ArrayDataEntry = new DataEntry(dataType: parsedType);
                }

                type = "ARRAY";

            }
            else if (type.Contains('"'))
            {
                var startUdt = type.IndexOf('"');
                var endUdt = type.LastIndexOf('"');
                if (startUdt >= 0 && endUdt >= 0)
                {
                    newEntry.DataTypeName = type.Substring(startUdt + 1, endUdt - startUdt - 1);
                    isUdt = true;
                }
            }
            else if (type.ToUpper().Contains("STRING"))
            {
                newEntry.StringLength = ParseStringLength(type);

                type = "STRING";

            }
            else if (type.ToUpper().Contains("STRUCT") && reader != null)
            {
                newEntry.Children = new LinkedList<DataEntry>();
                string line;
                while ((line = reader.ReadLine()) != null && !line.Contains("END_STRUCT"))
                {
                    newEntry.Children.AddLast(DataEntry.FromString(line, reader));
                }
            }

            DataType t;
			if (isUdt && type != "ARRAY")
			{
				t = DataType.UDT;
			}
			else
			{
				if (!Enum.TryParse<DataType>(type, true, out t))
				{
					throw new SiemensException("Invalid type detected: " + type);
				}
			}

			newEntry.DataType = t;

			return newEntry;

			Constant<int> ParseStringLength(string typeString)
			{
				splitString = typeString.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
				if (splitString.Length > 1)
				{
					if (int.TryParse(splitString[1], out length)) // Check to make sure the string length is an integer
					{
						return length;
					}
					else // If not, it's a constant referenced elsewhere
					{
						return new Constant<int>(splitString[1].Trim('\"'));
					}
				}
				else
				{
					return 254;
				}
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

		public static void ResolveArrayChildren(DataEntry entry, PortalPlc parentPlc)
		{
			if (entry.DataType != DataType.ARRAY)
				return;

			var arrayStart = parentPlc?.GetConstantValue(entry.ArrayStartIndex) ?? entry.ArrayStartIndex.Value;
			var arrayEnd = parentPlc?.GetConstantValue(entry.ArrayEndIndex) ?? entry.ArrayEndIndex.Value;

            _ = entry.ArrayDataEntry.CalculateSize(parentPlc);
            entry.Children = new LinkedList<DataEntry>();

			// First populate the array Children with the correct number and type of children
			for (var i = arrayStart; i <= arrayEnd; i++)
			{
				entry.Children.AddLast(new DataEntry(entry.Name + $"[{i}]", entry.ArrayDataEntry.DataType, entry.Comment + $" ({i})", entry.ArrayDataEntry.Children,
					entry.ArrayDataEntry.DataTypeName, entry.ArrayDataEntry.StringLength));
			}

			// re-calculate the addresses of the children
			entry.CalcluateAddresses(parentPlc);
		}

	}
}
