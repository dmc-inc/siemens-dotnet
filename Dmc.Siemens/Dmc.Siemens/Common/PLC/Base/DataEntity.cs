using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.PLC
{
    public abstract class DataEntity : Block, IEnumerable<DataEntry>
    {

        #region Public Properties

        public LinkedList<DataEntry> Data { get; set; } = new LinkedList<DataEntry>();



		#endregion

		#region Protected Properties
		
		private IDictionary<DataEntry, Address> Addresses { get; } = new Dictionary<DataEntry, Address>();

		#endregion

		#region Public Methods

		public IDictionary<DataEntry, Address> CalcluateAddresses(IProject project)
		{
			int currentByte = 0;
			int currentBit = 0;
			
			foreach (DataEntry entry in this)
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
						currentByte += this.GetByteSize(entry, project);
						break;
					case DataType.BOOL:
						entryAddress = new Address(currentByte, currentBit);
						Increment(isBit: true);
						break;
					case DataType.BYTE:
					case DataType.CHAR:
						Increment(isByte: true);
						entryAddress = new Address(currentByte, currentBit);
						currentByte += this.GetByteSize(entry);
						break;
					default:
						entryAddress = new Address();
						break;
				}

				// Add or overwrite the current address
				this.Addresses[entry] = entryAddress;

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

			return this.Addresses;
		}

		public override IParsableSource ParseSource(TextReader reader)
        {
            string line;
            string[] split;
            bool isInData = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!isInData)
                {
                    if (line.Contains("VERSION"))
                    {
                        split = line.Split(':');
                        if (split.Length > 1)
                        {
                            this.Version = split[1].Trim();
                        }
                    }
                    else if (line.Contains(this.DataHeader))
                    {
                        isInData = true;
                    }
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        if (line.Contains("END_" + this.DataHeader))
                        {
                            isInData = false;
                            break;
                        }
                        else
                        {
                            this.Data.AddLast(DataEntry.FromString(line, reader));
                        }
                    }
                }

            }

            return this;

        }

		public IEnumerator<DataEntry> GetEnumerator()
		{
			return ((IEnumerable<DataEntry>)this.Data).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<DataEntry>)this.Data).GetEnumerator();
		}

		#endregion

		#region Private Methods

		// Project can be null if it is a primitive data type
		private int GetByteSize(DataEntry entry, IProject project = null)
		{
			// Check if it is a primitive first
			int size;
			if ((size = TagHelper.GetPrimitiveByteSize(entry.DataType, (entry.StringLength.HasValue) ? entry.StringLength.Value : 0)) >= 0)
				return size;

			// Handle the cases where it is not
			switch (entry.DataType)
			{
				case DataType.ARRAY:
					int arraySize = project.GetConstantValue(entry.ArrayEndIndex) - project.GetConstantValue(entry.ArrayStartIndex) + 1;
					DataType type;
					DataEntry newEntry;
					if (Enum.TryParse(entry.DataTypeName, out type))
					{
						newEntry = new DataEntry() { DataType = type };
					}
					else
					{
						newEntry = project.GetUdtStructure(entry.DataTypeName);
					}
					size = this.GetByteSize(entry, project);
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
				case DataType.UDT:
					return GetByteSize(project.GetUdtStructure(entry.DataTypeName), project);
				default:
					throw new SiemensException($"Invalid DataType: {entry.DataType.ToString()}");
			}
		}

		#endregion

	}
}
