using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.IO;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.Export
{
	public static class KepwareConfiguration
	{

		#region Public Methods

		public static void CreateFromBlocks(IEnumerable<Block> blocks, string path, IProject owningProject)
		{
			if (blocks == null)
				throw new ArgumentNullException(nameof(blocks));
			IEnumerable<DataBlock> dataBlocks;
			if ((dataBlocks = blocks.OfType<DataBlock>()).Count() <= 0)
				throw new ArgumentException("Blocks does not contain any valid DataBlocks.", nameof(blocks));

			CreateFromBlocksInternal(dataBlocks, path, owningProject);
		}

		public static void CreateFromBlocks(DataBlock block, string path, IProject owningProject)
		{
			CreateFromBlocksInternal(new[] { block }, path, owningProject);
		}

		#endregion

		#region Private Methods

		private static void CreateFromBlocksInternal(IEnumerable<DataBlock> blocks, string path, IProject parentProject)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!FileHelpers.CheckValidFilePath(path, ".csv"))
				throw new ArgumentException(path + " is not a valid path.", nameof(path));

			try
			{
				using (var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					StreamWriter writer = new StreamWriter(file);

					WriteHeaders(writer);

					foreach (var block in blocks)
					{
						if (block == null)
							throw new ArgumentNullException(nameof(block));
						if (block.Data?.Count <= 0)
							throw new ArgumentException("Block '" + block.Name + "' contains no data", nameof(block));

						ExportDataBlockToFile(block, writer, parentProject);
					}
				}
			}
			catch (Exception e)
			{
				throw new SiemensException("Could not write Kepware configuration", e);
			}
		}

		private static void ExportDataBlockToFile(DataBlock block, TextWriter writer, IProject parentProject)
		{
			Address entryAddress = new Address();
			int previousSize = 0;
			foreach (var entry in block)
			{
				AddDataEntry(entry, block.Name, entryAddress);
				previousSize = entry.CalculateByteSize(parentProject);
				if (previousSize == 0)
					entryAddress += new Address(0, 1);
				else
					entryAddress += previousSize;
			}

			void AddDataEntry(DataEntry entry, string entryPrefix, Address address)
			{
				string addressPrefix = "";
				string type = "";

				switch (entry.DataType)
				{
					case DataType.ARRAY:
						DataType subType = TagHelper.ParseDataType(entry.DataTypeName);
						bool isNonPrimitive = !TagHelper.IsPrimitive(subType);
						int primitiveByteSize = TagHelper.GetPrimitiveByteSize(subType);
						DataEntry structContents = null;
						if (isNonPrimitive)
						{
							switch (subType)
							{
								case DataType.ARRAY:
									throw new SiemensException("2D arrays not supported: " + entry.Name);
								case DataType.STRUCT:
									structContents = entry;
									break;
								default:
									try
									{
										structContents = parentProject.GetUdtStructure(entry.DataTypeName);
										break;
									}
									catch (Exception e)
									{
										throw new SiemensException("Unsupported array type of '" + entry.Name + "', type: " + entry.DataTypeName, e);
									}
							}

							for (int i = parentProject.GetConstantValue(entry.ArrayStartIndex); i <= parentProject.GetConstantValue(entry.ArrayEndIndex); i++)
							{
								AddDataEntry(structContents, $"{entryPrefix}{entry.Name}[{i}].", address);
							}
						}
						else // if it is a primitive, create a temporary data entry to make recurison/address generation simpler
						{
							LinkedList<DataEntry> arrayEntries = new LinkedList<DataEntry>();
							for (int i = parentProject.GetConstantValue(entry.ArrayStartIndex); i <= parentProject.GetConstantValue(entry.ArrayEndIndex); i++)
							{
								arrayEntries.AddLast(new DataEntry($"[{i}]", subType, entry.Comment + i.ToString()));
							}

							structContents = new DataEntry(entry.Name, DataType.STRUCT, entry.Comment, arrayEntries);

							AddDataEntry(structContents, $"{entryPrefix}{entry.Name}", address);
						}
						break;
					case DataType.BOOL:
						addressPrefix = "DBX";
						type = "Boolean";
						break;
					case DataType.BYTE:
						addressPrefix = "DBB";
						type = "Byte";
						break;
					case DataType.CHAR:
						addressPrefix = "DBB";
						type = "Char";
						break;
					case DataType.DATE:
					case DataType.DATE_AND_TIME:
						addressPrefix = "DATE";
						type = "Date";
						break;
					case DataType.TIME:
					case DataType.DINT:
						addressPrefix = "DBD";
						type = "Long";
						break;
					case DataType.DWORD:
						addressPrefix = "DBD";
						type = "Dword";
						break;
					case DataType.INT:
						addressPrefix = "DBW";
						type = "Short";
						break;
					case DataType.REAL:
						addressPrefix = "DBD";
						type = "Float";
						break;
					case DataType.STRING:
						addressPrefix = "STRING";
						type = "String";
						break;
					case DataType.UDT:
					case DataType.STRUCT:
						var addresses = entry.CalcluateAddresses(parentProject);

						string prefix = entryPrefix + ((entry.Children.First.Value.Name.Contains("[")) ? "" : ".");
						foreach (var subEntry in entry.Children)
						{

							AddDataEntry(subEntry, prefix, addresses[subEntry] + address);
						}
						break;
					case DataType.WORD:
						addressPrefix = "DBW";
						type = "Word";
						break;
					default:
						throw new SiemensException("Data type: '" + entry.DataType.ToString() + "' not supported.");

				}

				if (TagHelper.IsPrimitive(entry.DataType))
				{
					string addressString = "DB" + block.Number + "." + addressPrefix;
					if (entry.DataType == DataType.BOOL)
						addressString += address.ByteOffset + "." + address.BitOffset;
					else if (entry.DataType == DataType.STRING)
						addressString += "0." + parentProject.GetConstantValue(entry.StringLength).ToString();
					else
						addressString += address.ByteOffset;

					string[] entryItems = new string[16]
					{
					entryPrefix + entry.Name,
					addressString,
					type,
					"1",
					"R/W",
					"100",
					string.Empty,
					string.Empty,
					string.Empty,
					string.Empty,
					string.Empty,
					string.Empty,
					string.Empty,
					string.Empty,
					string.Empty,
					entry.Comment
					};

					writer.WriteLine(string.Join(",", entryItems));
				}

			}

		}

		private static void WriteHeaders(TextWriter writer)
		{
			writer.WriteLine("Tag Name,Address,Data Type,Respect Data Type,Client Access,Scan Rate,Scaling,Raw Low,Raw High,Scaled Low,Scaled High,Scaled Data Type,Clamp Low,Clamp High,Eng Units,Description");
		}

		#endregion

	}
}
