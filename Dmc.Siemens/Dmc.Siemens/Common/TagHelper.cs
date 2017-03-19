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
			return (Enum.TryParse(dataTypeString, out DataType type)) ? type : DataType.UNKNOWN;
		}

		public static IDictionary<DataEntry, Address> CalcluateAddresses(IDataEntry dataEntry, IPlc plc)
		{
			if (dataEntry.Children?.Count <= 0)
				return null;

			Dictionary<DataEntry, Address> addresses = new Dictionary<DataEntry, Address>();
			int currentByte = 0;
			int currentBit = 0;

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
						Increment();
						entryAddress = new Address(currentByte, currentBit);
						currentByte += dataEntry.CalculateByteSize(plc);
						break;
					case DataType.BOOL:
						entryAddress = new Address(currentByte, currentBit);
						Increment(isBit: true);
						break;
					case DataType.BYTE:
					case DataType.CHAR:
						Increment(isByte: true);
						entryAddress = new Address(currentByte, currentBit);
						currentByte += dataEntry.CalculateByteSize(null);
						break;
					default:
						entryAddress = new Address();
						break;
				}

				// Add or overwrite the current address
				addresses[entry] = entryAddress;

			}

			void Increment(bool isBit = false, bool isByte = false)
			{
				if (isBit)
				{
					if (currentBit >= 7)
					{
						currentBit = 0;
						currentByte++;
					}
					else
					{
						currentBit++;
					}
				}
				else if (isByte)
				{
					if (currentBit != 0)
					{
						currentByte += 1;
					}
					currentBit = 0;
				}
				else
				{
					if (currentByte % 2 == 0 && currentBit != 0)
					{
						currentByte += 2;
					}
					else if (currentByte % 2 == 1)
					{
						currentByte += 1;
					}

					currentBit = 0;

				}
			}

			return addresses;
		}

		public static int CalculateByteSize(IDataEntry dataEntry, IPlc plc)
		{
			// Check if it is a DataEntry (not a block) then check if it is a primitive
			int size;
			DataEntry entry = (dataEntry as DataEntry);
			if (entry != null && (size = TagHelper.GetPrimitiveByteSize(entry.DataType, (entry.StringLength.HasValue) ? entry.StringLength.Value : 0)) >= 0)
				return size;

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
					DataType type;
					IDataEntry newEntry;
					if (Enum.TryParse(entry.DataTypeName, out type))
					{
						newEntry = new DataEntry() { DataType = type };
					}
					else
					{
						newEntry = plc.GetUdtStructure(entry.DataTypeName);
					}
					size = newEntry.CalculateByteSize(plc);
					if (size == 0)
					{
						int overflow = arraySize % 16;
						if (overflow > 0)
						{
							return ((arraySize / 16) * 2 + 2);
						}
						else
						{
							return (arraySize / 16);
						}
					}
					else if (size == 1)
					{
						if (arraySize % 2 > 0)
						{
							return (arraySize / 2 + 1);
						}
						else
						{
							return (arraySize / 2);
						}
					}
					else
					{
						return arraySize * size;
					}

				case DataType.STRUCT:
					int sum = 0;
					Parallel.ForEach(dataEntry, e => Interlocked.Add(ref sum, e.CalculateByteSize(plc)));
					return sum;
				case DataType.UDT:
					return plc.GetUdtStructure(entry.DataTypeName).CalculateByteSize(plc);
				default:
					throw new SiemensException($"Invalid DataType: {dataEntry.DataType.ToString()}");
			}
		}

	}
}
