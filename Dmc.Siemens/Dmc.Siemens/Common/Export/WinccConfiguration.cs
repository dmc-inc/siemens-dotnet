using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.IO;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Portal.Base;
using Dmc.Siemens.Portal.Plc;
using SpreadsheetLight;
using Dmc.Siemens.Common.Export.Base;

namespace Dmc.Siemens.Common.Export
{
	public static class WinccConfiguration
	{

		#region Private Fields

		private const string ALARM_WORKSHEET_NAME = "DiscreteAlarms";
		private const string TAG_WORKSHEET_NAME = "Hmi Tags";

		#endregion

		#region Public Methods

		public static void Create(IEnumerable<IBlock> blocks, string path, PortalPlc parentPlc, WinccExportType exportType = WinccExportType.ComfortAdvanced)
		{
			if (blocks == null)
				throw new ArgumentNullException(nameof(blocks));
			IEnumerable<DataBlock> dataBlocks;
			if ((dataBlocks = blocks.OfType<DataBlock>())?.Count() <= 0)
				throw new ArgumentException("Blocks does not contain any valid DataBlocks.", nameof(blocks));

			WinccConfiguration.CreateInternal(dataBlocks, path, parentPlc, exportType);
		}

		public static void Create(IBlock block, string path, PortalPlc parentPlc, WinccExportType exportType = WinccExportType.ComfortAdvanced)
		{
			WinccConfiguration.Create(new[] { block }, path, parentPlc);
		}

		#endregion

		#region Private Methods

		private static void CreateInternal(IEnumerable<DataBlock> dataBlocks, string path, PortalPlc parentPlc, WinccExportType exportType)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!FileHelpers.CheckValidFilePath(path, ".xlsx"))
				throw new ArgumentException(path + " is not a valid path.", nameof(path));

			int currentAlarmRow = 2;
			int currentTagRow = 2;
			string dataBlockName = string.Empty;

			using (SLDocument document = new SLDocument())
			{
				WinccConfiguration.WriteXlsxHeaders(document, exportType);
				
				foreach (DataBlock db in dataBlocks)
				{
					dataBlockName = db.Name;
					db.CalcluateAddresses(parentPlc);

					foreach (DataEntry entry in db.Children)
					{
						ProcessDataEntry(document, entry, string.Empty, new Address(), db.Name);
					}

					if (exportType != WinccExportType.Professional)
						WriteComfortAdvancedTagRow(document, db, "HMI_Connection_1");

				}

				using (Stream file = new FileStream(path, FileMode.Create))
				{
					document.SaveAs(file);
				}
			}

			void ProcessDataEntry(SLDocument document, DataEntry entry, string prependText, Address addressOffset, string prependTag = "")
			{
				string stackedComment;
				if (string.IsNullOrWhiteSpace(prependText))
					stackedComment = entry.Comment;
				else if (prependText.EndsWith(" - "))
					stackedComment = prependText + entry.Comment;
				else
					stackedComment = prependText + " - " + entry.Comment;

				string stackedTag;
				if (string.IsNullOrWhiteSpace(prependTag))
					stackedTag = entry.Name;
				else if (prependText.EndsWith("."))
					stackedTag = prependTag + entry.Name;
				else
					stackedTag = prependTag + "." + entry.Name;

				switch (entry.DataType)
				{
					case DataType.BOOL:
						int alarmNumber = currentAlarmRow - 1;

						if (document.GetCurrentWorksheetName() != ALARM_WORKSHEET_NAME)
						{
							document.SelectWorksheet(ALARM_WORKSHEET_NAME);
						}

						// Common data between versions
						document.SetCellValue(currentAlarmRow, 1, alarmNumber.ToString());
						document.SetCellValue(currentAlarmRow, 2, $"Discrete_alarm_{alarmNumber}");
						document.SetCellValue(currentAlarmRow, 3, stackedComment);
						document.SetCellValue(currentAlarmRow, 4, "<No value>");
						document.SetCellValue(currentAlarmRow, 5, "Errors");
						

						if (exportType == WinccExportType.Professional)
						{
							document.SetCellValue(currentAlarmRow, 6, stackedTag.Replace('.', '_'));
							document.SetCellValue(currentAlarmRow, 7, "0");
							document.SetCellValue(currentAlarmRow, 8, "On rising edge");
							document.SetCellValue(currentAlarmRow, 9, "<No value>");
							document.SetCellValue(currentAlarmRow, 10, "0");
							document.SetCellValue(currentAlarmRow, 11, "<No value>");
							document.SetCellValue(currentAlarmRow, 12, "0");
							document.SetCellValue(currentAlarmRow, 13, "<No value>");
							document.SetCellValue(currentAlarmRow, 14, "0");
							document.SetCellStyle(currentAlarmRow, 15, new SLStyle() { FormatCode = "@" });
							document.SetCellValue(currentAlarmRow, 15, "False");
							document.SetCellValue(currentAlarmRow, 16, stackedComment);
							document.SetCellValue(currentAlarmRow, 17, "<No value>");
							document.SetCellValue(currentAlarmRow, 19, "<No value>");
							document.SetCellValue(currentAlarmRow, 21, "<No value>");
							document.SetCellValue(currentAlarmRow, 23, "<No value>");
							document.SetCellValue(currentAlarmRow, 25, "<No value>");
							document.SetCellValue(currentAlarmRow, 27, "<No value>");
							document.SetCellValue(currentAlarmRow, 29, "<No value>");
							document.SetCellValue(currentAlarmRow, 31, "<No value>");
							document.SetCellValue(currentAlarmRow, 33, "<No value>");
							document.SetCellValue(currentAlarmRow, 35, "<No value>");
							document.SetCellValue(currentAlarmRow, 36, "<No value>");
							document.SetCellValue(currentAlarmRow, 37, "<No value>");
							document.SetCellValue(currentAlarmRow, 38, "<No value>");
							document.SetCellValue(currentAlarmRow, 39, "<No value>");
							document.SetCellValue(currentAlarmRow, 40, "<No value>");
							document.SetCellValue(currentAlarmRow, 41, "<No value>");
							document.SetCellValue(currentAlarmRow, 42, "<No value>");
							document.SetCellValue(currentAlarmRow, 43, "<No value>");
							document.SetCellValue(currentAlarmRow, 44, "<No value>");
							document.SetCellStyle(currentAlarmRow, 45, new SLStyle() { FormatCode = "@" });
							document.SetCellValue(currentAlarmRow, 45, "False");
							document.SetCellValue(currentAlarmRow, 46, "0");
							document.SetCellValue(currentAlarmRow, 47, "0");
							document.SetCellValue(currentAlarmRow, 48, "0");

							WriteProfessionalTagRow(document, stackedTag, "HMI_Connection_1");
						}
						else // WinCC Comfort/Advanced
						{
							document.SetCellValue(currentAlarmRow, 6, dataBlockName);
							document.SetCellValue(currentAlarmRow, 7, AddressToTriggerBit(addressOffset + entry.Address.Value).ToString());
							document.SetCellValue(currentAlarmRow, 8, "<No value>");
							document.SetCellValue(currentAlarmRow, 9, "0");
							document.SetCellValue(currentAlarmRow, 10, "<No value>");
							document.SetCellValue(currentAlarmRow, 11, "0");
							document.SetCellValue(currentAlarmRow, 12, "<No value>");
							document.SetCellStyle(currentAlarmRow, 13, new SLStyle() { FormatCode = "@" });
							document.SetCellValue(currentAlarmRow, 13, "False");
							document.SetCellValue(currentAlarmRow, 14, stackedComment);
						}

						currentAlarmRow++;
						break;
					case DataType.UDT:
					case DataType.STRUCT:
						entry.CalcluateAddresses(parentPlc);
						foreach (DataEntry newEntry in entry.Children)
						{
							ProcessDataEntry(document, newEntry, stackedComment, addressOffset + entry.Address.Value, stackedTag);
						}
						break;
					case DataType.ARRAY:
						TagHelper.ResolveArrayChildren(entry, parentPlc);

						// write a new entry for each of the children
						foreach (var child in entry.Children)
						{
							ProcessDataEntry(document, child, prependText, (entry.Address.Value + addressOffset), stackedTag);
						}
						break;
					default:
						throw new SiemensException("Unsupported data type for WinCC alarms: " + entry.Name + ", " + entry.DataType.ToString());
				}

			}

			

			void WriteComfortAdvancedTagRow(SLDocument document, DataBlock dataBlock, string connectionName)
			{

				if (document.GetCurrentWorksheetName() != TAG_WORKSHEET_NAME)
				{
					document.SelectWorksheet(TAG_WORKSHEET_NAME);
				}

				string dataType, address;

				int dbLength = dataBlock.CalculateSize(parentPlc).Byte;

				int wordLength = (int)Math.Ceiling(dbLength / 2.0);

				if (dbLength > 2)
				{
					dataType = $"Array [0..{wordLength - 1}] of Word";
					address = $"%DB{dataBlock.Number}.DBX0.0";
				}
				else
				{
					dataType = "Word";
					address = $"%DB{dataBlock.Number}.DBW0";
					wordLength = 0;
				}

				document.SetCellValue(currentTagRow, 1, dataBlock.Name);
				document.SetCellValue(currentTagRow, 2, "Default tag table");
				document.SetCellValue(currentTagRow, 3, connectionName);
				document.SetCellValue(currentTagRow, 4, "<No Value>");
				document.SetCellValue(currentTagRow, 5, dataType);
				document.SetCellValue(currentTagRow, 6, (wordLength + 1) * 2);
				document.SetCellValue(currentTagRow, 7, "Binary");
				document.SetCellValue(currentTagRow, 8, "Absolute access");
				document.SetCellValue(currentTagRow, 9, address);
				document.SetCellStyle(currentTagRow, 10, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 10, "False");
				document.SetCellValue(currentTagRow, 11, "<No Value>");
				document.SetCellValue(currentTagRow, 12, "<No Value>");
				document.SetCellValue(currentTagRow, 13, 0);
				document.SetCellValue(currentTagRow, 14, "<No Value>");
				document.SetCellValue(currentTagRow, 15, "<No Value>");
				document.SetCellValue(currentTagRow, 16, "Continuous");
				document.SetCellValue(currentTagRow, 17, "1 s");
				document.SetCellValue(currentTagRow, 18, "None");
				document.SetCellValue(currentTagRow, 19, "<No Value>");
				document.SetCellValue(currentTagRow, 20, "None");
				document.SetCellValue(currentTagRow, 21, "<No Value>");
				document.SetCellStyle(currentTagRow, 22, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 22, "False");
				document.SetCellValue(currentTagRow, 23, 10);
				document.SetCellValue(currentTagRow, 24, 0);
				document.SetCellValue(currentTagRow, 25, 100);
				document.SetCellValue(currentTagRow, 26, 0);
				document.SetCellStyle(currentTagRow, 27, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 27, "False");
				document.SetCellValue(currentTagRow, 28, "None");
				document.SetCellStyle(currentTagRow, 29, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 29, "False");
				
				currentTagRow++;

			}

			void WriteProfessionalTagRow(SLDocument document, string tag, string connectionName)
			{

				if (document.GetCurrentWorksheetName() != TAG_WORKSHEET_NAME)
				{
					document.SelectWorksheet(TAG_WORKSHEET_NAME);
				}

				document.SetCellValue(currentTagRow, 1, tag.Replace('.', '_'));
				document.SetCellValue(currentTagRow, 2, "Default tag table");
				document.SetCellValue(currentTagRow, 3, connectionName);
				document.SetCellValue(currentTagRow, 4, tag);
				document.SetCellValue(currentTagRow, 5, "Bool");
				document.SetCellValue(currentTagRow, 6, "Bool");
				document.SetCellValue(currentTagRow, 7, "1");
				document.SetCellValue(currentTagRow, 8, "Binary");
				document.SetCellValue(currentTagRow, 9, "Symbolic access");
				document.SetCellValue(currentTagRow, 10, "<No Value>");
				document.SetCellValue(currentTagRow, 11, "<No Value>");
				document.SetCellStyle(currentTagRow, 12, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 12, "False");
				document.SetCellStyle(currentTagRow, 13, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 13, "False");
				document.SetCellValue(currentTagRow, 14, "<No Value>");
				document.SetCellValue(currentTagRow, 15, "<No Value>");
				document.SetCellValue(currentTagRow, 16, "Client'=/Server wide");
				document.SetCellValue(currentTagRow, 17, "<No Value>");
				document.SetCellValue(currentTagRow, 18, "None");
				document.SetCellValue(currentTagRow, 19, "<No Value>");
				document.SetCellValue(currentTagRow, 20, "None");
				document.SetCellValue(currentTagRow, 21, "<No Value>");
				document.SetCellStyle(currentTagRow, 22, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 22, "False");
				document.SetCellValue(currentTagRow, 23, "10");
				document.SetCellValue(currentTagRow, 24, "0");
				document.SetCellValue(currentTagRow, 25, "100");
				document.SetCellValue(currentTagRow, 26, "0");
				document.SetCellStyle(currentTagRow, 27, new SLStyle() { FormatCode = "@" });
				document.SetCellValue(currentTagRow, 27, "False");

				currentTagRow++;

			}

			int AddressToTriggerBit(Address address)
			{
				if (address.Byte % 2 == 0)
				{
					return ((address.Byte + 1) * 8 + address.Bit);
				}
				else
				{
					return ((address.Byte - 1) * 8 + address.Bit);
				}
			}

		}

		private static void WriteXlsxHeaders(SLDocument document, WinccExportType exportType)
		{
			string name = document.GetCurrentWorksheetName();
			if (name != TAG_WORKSHEET_NAME)
			{
				document.RenameWorksheet(name, TAG_WORKSHEET_NAME);
				document.SelectWorksheet(TAG_WORKSHEET_NAME);
			}

			document.SetCellValue(1, 1, "Name");
			document.SetCellValue(1, 2, "Path");
			document.SetCellValue(1, 3, "Connection");
			document.SetCellValue(1, 5, "DataType");

			if (exportType == WinccExportType.Professional)
			{
				document.SetCellValue(1, 4, "PLC tag");
				document.SetCellValue(1, 6, "HMI DataType");
				document.SetCellValue(1, 7, "Length");
				document.SetCellValue(1, 8, "Coding");
				document.SetCellValue(1, 9, "Access Method");
				document.SetCellValue(1, 10, "Address");
				document.SetCellValue(1, 11, "Start value");
				document.SetCellValue(1, 12, "Quality Code");
				document.SetCellValue(1, 13, "Persistency");
				document.SetCellValue(1, 14, "Substitute value");
				document.SetCellValue(1, 15, "Tag value [en-US]");
				document.SetCellValue(1, 16, "Update Mode");
				document.SetCellValue(1, 17, "Comment [en-US]");
				document.SetCellValue(1, 18, "Limit Upper 2 Type");
				document.SetCellValue(1, 19, "Limit Upper 2");
				document.SetCellValue(1, 20, "Limit Lower 2 Type");
				document.SetCellValue(1, 21, "Limit Lower 2");
				document.SetCellValue(1, 22, "Linear scaling");
				document.SetCellValue(1, 23, "End value PLC");
				document.SetCellValue(1, 24, "Start value PLC");
				document.SetCellValue(1, 25, "End value HMI");
				document.SetCellValue(1, 26, "Start value HMI");
				document.SetCellValue(1, 27, "Synchronization");
			}
			else // WinCC Comfort/Advanced
			{
				document.SetCellValue(1, 4, "Plc tag");
				document.SetCellValue(1, 6, "Length");
				document.SetCellValue(1, 7, "Coding");
				document.SetCellValue(1, 8, "Access Method");
				document.SetCellValue(1, 9, "Address");
				document.SetCellValue(1, 10, "Indirect addressing");
				document.SetCellValue(1, 11, "Index tag");
				document.SetCellValue(1, 12, "Start value");
				document.SetCellValue(1, 13, "ID tag");
				document.SetCellValue(1, 14, "Display name [en-US]");
				document.SetCellValue(1, 15, "Comment [en-US]");
				document.SetCellValue(1, 16, "Acquisition mode");
				document.SetCellValue(1, 17, "Acquisition cycle");
				document.SetCellValue(1, 18, "Range Maximum Type");
				document.SetCellValue(1, 19, "Range Maximum");
				document.SetCellValue(1, 20, "Range Minimum Type");
				document.SetCellValue(1, 21, "Range Minimum");
				document.SetCellValue(1, 22, "Linear scaling");
				document.SetCellValue(1, 23, "End value Plc");
				document.SetCellValue(1, 24, "Start value Plc");
				document.SetCellValue(1, 25, "End value HMI");
				document.SetCellValue(1, 26, "Start value HMI");
				document.SetCellValue(1, 27, "Gmp relevant");
				document.SetCellValue(1, 28, "Confirmation Type");
				document.SetCellValue(1, 29, "Mandatory Commenting");
			}
			
			document.AddWorksheet(ALARM_WORKSHEET_NAME);

			document.SetCellValue(1, 1, "ID");
			document.SetCellValue(1, 2, "Name");
			document.SetCellValue(1, 5, "Class");
			document.SetCellValue(1, 6, "Trigger tag");
			document.SetCellValue(1, 7, "Trigger bit");
			switch (exportType)
			{
				case WinccExportType.ComfortAdvanced:
					document.SetCellValue(1, 3, "Alarm text [en-US], Alarm text");
					document.SetCellValue(1, 4, "FieldInfo [Alarm text]");
					document.SetCellValue(1, 8, "Acknowledgement tag");
					document.SetCellValue(1, 9, "Acknowledgement bit");
					document.SetCellValue(1, 10, "Plc acknowledgement tag");
					document.SetCellValue(1, 11, "Plc acknowledgement bit");
					document.SetCellValue(1, 12, "Group");
					document.SetCellValue(1, 13, "Report");
					document.SetCellValue(1, 14, "Info text [en-US], Info text");
					break;
				case WinccExportType.Professional:
					document.SetCellValue(1, 3, "Alarm text [en-US], Alarm text 1");
					document.SetCellValue(1, 4, "FieldInfo [Alarm text 1]");
					document.SetCellValue(1, 8, "Trigger mode");
					document.SetCellValue(1, 9, "Acknowledgement tag");
					document.SetCellValue(1, 10, "Acknowledgement bit");
					document.SetCellValue(1, 11, "Status tag");
					document.SetCellValue(1, 12, "Status bit");
					document.SetCellValue(1, 13, "Group");
					document.SetCellValue(1, 14, "Priority");
					document.SetCellValue(1, 15, "Single acknowledgement");
					document.SetCellValue(1, 16, "Info text [en-US], Info text");
					document.SetCellValue(1, 17, "Additional text 1 [en-US], Alarm text 2");
					document.SetCellValue(1, 18, "FieldInfo [Alarm text 2]");
					document.SetCellValue(1, 19, "Additional text 2 [en-US], Alarm text 3");
					document.SetCellValue(1, 20, "FieldInfo [Alarm text 3]");
					document.SetCellValue(1, 21, "Additional text 3 [en-US], Alarm text 4");
					document.SetCellValue(1, 22, "FieldInfo [Alarm text 4]");
					document.SetCellValue(1, 23, "Additional text 4 [en-US], Alarm text 5");
					document.SetCellValue(1, 24, "FieldInfo [Alarm text 5]");
					document.SetCellValue(1, 25, "Additional text 5 [en-US], Alarm text 6");
					document.SetCellValue(1, 26, "FieldInfo [Alarm text 6]");
					document.SetCellValue(1, 27, "Additional text 6 [en-US], Alarm text 7");
					document.SetCellValue(1, 28, "FieldInfo [Alarm text 7]");
					document.SetCellValue(1, 29, "Additional text 7 [en-US], Alarm text 8");
					document.SetCellValue(1, 30, "FieldInfo [Alarm text 8]");
					document.SetCellValue(1, 31, "Additional text 8 [en-US], Alarm text 9");
					document.SetCellValue(1, 32, "FieldInfo [Alarm text 9]");
					document.SetCellValue(1, 33, "Additional text 9 [en-US], Alarm text 10");
					document.SetCellValue(1, 34, "FieldInfo [Alarm text 10]");
					document.SetCellValue(1, 35, "Alarm parameter 1");
					document.SetCellValue(1, 36, "Alarm parameter 2");
					document.SetCellValue(1, 37, "Alarm parameter 3");
					document.SetCellValue(1, 38, "Alarm parameter 4");
					document.SetCellValue(1, 39, "Alarm parameter 5");
					document.SetCellValue(1, 40, "Alarm parameter 6");
					document.SetCellValue(1, 41, "Alarm parameter 7");
					document.SetCellValue(1, 42, "Alarm parameter 8");
					document.SetCellValue(1, 43, "Alarm parameter 9");
					document.SetCellValue(1, 44, "Alarm parameter 10");
					document.SetCellValue(1, 45, "Alarm annunciation");
					document.SetCellValue(1, 46, "Display suppression mask");
					document.SetCellValue(1, 47, "PLC number");
					document.SetCellValue(1, 48, "CPU number");
					
					break;
				default:
					throw new NotSupportedException($"Export for version '{exportType}' not yet available.");
			}
			

		}
		
		#endregion

	}
}
