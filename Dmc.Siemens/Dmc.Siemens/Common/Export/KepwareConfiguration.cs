using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.IO;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Common.PLC.Interfaces;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.Export
{
	public static class KepwareConfiguration
	{

		#region Public Methods

		public static void CreateFromBlocks(IEnumerable<IBlock> blocks, string path, IPlc owningPlc)
		{
			if (blocks == null)
				throw new ArgumentNullException(nameof(blocks));
			IEnumerable<DataBlock> dataBlocks;
			if ((dataBlocks = blocks.OfType<DataBlock>())?.Count() <= 0)
				throw new ArgumentException("Blocks does not contain any valid DataBlocks.", nameof(blocks));

			CreateFromBlocksInternal(dataBlocks, path, owningPlc);
		}

		public static void CreateFromBlocks(DataBlock block, string path, IPlc owningPlc)
		{
			CreateFromBlocksInternal(new[] { block }, path, owningPlc);
		}

		#endregion

		#region Private Methods

		private static void CreateFromBlocksInternal(IEnumerable<DataBlock> blocks, string path, IPlc parentPlc)
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
						if (block.Children?.Count <= 0)
							throw new ArgumentException("Block '" + block.Name + "' contains no data", nameof(block));

						ExportDataBlockToFile(block, writer, parentPlc);
					}
				}
			}
			catch (Exception e)
			{
				throw new SiemensException("Could not write Kepware configuration", e);
			}
		}

		private static void ExportDataBlockToFile(DataBlock block, TextWriter writer, IPlc parentPlc)
		{
			block.CalcluateAddresses(parentPlc);
			foreach (var entry in block)
			{
				AddDataEntry(entry, block.Name + ".", new Address());
			}

			void AddDataEntry(DataEntry entry, string entryPrefix, Address parentOffset)
			{
				string addressPrefix = "";
				string type = "";

				switch (entry.DataType)
				{
					case DataType.ARRAY:
						int arrayStart = parentPlc?.GetConstantValue(entry.ArrayStartIndex) ?? entry.ArrayStartIndex.Value;
						int arrayEnd = parentPlc?.GetConstantValue(entry.ArrayEndIndex) ?? entry.ArrayEndIndex.Value;
						Address arraySubTypeSize = entry.ArrayDataEntry.CalculateSize(parentPlc);
						entry.Children.Clear();

						// First populate the array Children with the correct number and type of children
						for (int i = arrayStart; i <= arrayEnd; i++)
						{
							entry.Children.AddLast(new DataEntry(entry.Name + $"[{i}]", entry.ArrayDataEntry.DataType, entry.Comment + $" ({i})", entry.ArrayDataEntry.Children,
								entry.ArrayDataEntry.DataTypeName, entry.ArrayDataEntry.StringLength));
						}

						// re-calculate the addresses of the children
						entry.CalcluateAddresses(parentPlc);

						// write a new entry for each of the children
						foreach (var child in entry.Children)
						{
							AddDataEntry(child, entryPrefix, (entry.AddressOffset.Value + parentOffset));
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
						entry.CalcluateAddresses(parentPlc);
						foreach (var child in entry)
						{
							AddDataEntry(child, entryPrefix + entry.Name + ".", (entry.AddressOffset.Value + parentOffset));
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
					Address absoluteAddress = parentOffset + entry.AddressOffset.Value;

					string addressString = "DB" + block.Number + "." + addressPrefix + absoluteAddress.Byte;
					if (entry.DataType == DataType.BOOL)
						addressString += "." + absoluteAddress.Bit;
					else if (entry.DataType == DataType.STRING)
						addressString += "." + (parentPlc?.GetConstantValue(entry.StringLength) ?? entry.StringLength.Value).ToString();

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
