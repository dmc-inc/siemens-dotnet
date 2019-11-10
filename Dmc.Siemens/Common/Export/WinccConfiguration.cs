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
using Dmc.Siemens.Portal;
using System.Collections;

namespace Dmc.Siemens.Common.Export
{
	public static class WinccConfiguration
	{

		#region Private Fields

		private const string AlarmWorksheetName = "DiscreteAlarms";
		private const string TagWorksheetName = "Hmi Tags";
		private const string WinccNoValue = "<No value>";
		private const string WinccZero = "0";
		private const string WinccFalse = "False";
		private const string WinccNone = "None";

		#endregion

		#region Public Methods

		public static void Create(IEnumerable<IBlock> blocks, string path, PortalPlc parentPlc, WinccExportType exportType, TiaPortalVersion portalVersion)
		{
			if (blocks == null)
				throw new ArgumentNullException(nameof(blocks));
			IEnumerable<DataBlock> dataBlocks;
			if ((dataBlocks = blocks.OfType<DataBlock>())?.Count() <= 0)
				throw new ArgumentException("Blocks does not contain any valid DataBlocks.", nameof(blocks));

			WinccConfiguration.CreateInternal(dataBlocks, path, parentPlc, exportType, portalVersion);
		}

		public static void Create(IBlock block, string path, PortalPlc parentPlc, WinccExportType exportType, TiaPortalVersion portalVersion)
		{
			WinccConfiguration.Create(new[] { block }, path, parentPlc, exportType, portalVersion);
		}

		#endregion

		#region Private Methods

		private static void CreateInternal(IEnumerable<DataBlock> dataBlocks, string path, PortalPlc parentPlc, WinccExportType exportType, TiaPortalVersion portalVersion)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!FileHelpers.IsValidFilePath(path, ".xlsx"))
				throw new ArgumentException(path + " is not a valid path.", nameof(path));

			var currentAlarmRow = 2;
			var currentTagRow = 2;
			string dataBlockName;
			var genericStyle = new SLStyle() { FormatCode = "@" };
			var proTags = new List<string>();

			using (var document = new SLDocument())
			{
				WinccConfiguration.WriteXlsxHeaders(document, exportType, portalVersion);
				
				foreach (var db in dataBlocks)
				{
					dataBlockName = db.Name;
					db.CalcluateAddresses(parentPlc);

					foreach (var entry in db.Children)
					{
						ProcessDataEntry(document, entry, string.Empty, new Address(), db.Name);
					}

					if (exportType != WinccExportType.Professional)
						WriteComfortAdvancedTagRow(document, db, "HMI_Connection_1");

				}

				if (exportType == WinccExportType.Professional)
				{
					foreach (var tag in proTags)
					{
						WriteProfessionalTagRow(document, tag, "HMI_Connection_1");
					}
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
						var alarmNumber = currentAlarmRow - 1;

						if (document.GetCurrentWorksheetName() != AlarmWorksheetName)
						{
							document.SelectWorksheet(AlarmWorksheetName);
						}

                        var row = WinccConstants.GetAlarmRow(exportType, portalVersion);
                        row[WinccExportField.Id] = alarmNumber.ToString();
                        row[WinccExportField.Name] = $"Discrete_alarm_{alarmNumber}";
                        row[WinccExportField.AlarmText] = stackedComment;
                        row[WinccExportField.InfoText] = stackedComment;

                        if (exportType == WinccExportType.Professional)
						{
                            row[WinccExportField.TriggerTag] = stackedTag.Replace('.', '_');

							proTags.Add(stackedTag);
						}
						else // WinCC Comfort/Advanced
						{
                            row[WinccExportField.TriggerTag] = dataBlockName;
                            row[WinccExportField.TriggerBit] = AddressToTriggerBit(addressOffset + entry.Address.Value).ToString();
						}

                        var column = 1;
                        foreach (var item in row)
                        {
                            document.SetCellStyle(currentAlarmRow, column, genericStyle);
                            document.SetCellValue(currentAlarmRow, column++, item);
                        }

                        currentAlarmRow++;
						break;
					case DataType.UDT:
					case DataType.STRUCT:
						entry.CalcluateAddresses(parentPlc);
						foreach (var newEntry in entry.Children)
						{
							ProcessDataEntry(document, newEntry, stackedComment, addressOffset + entry.Address.Value, stackedTag);
						}
						break;
					case DataType.ARRAY:
						TagHelper.ResolveArrayChildren(entry, parentPlc);

						// write a new entry for each of the children
						foreach (var child in entry.Children)
						{
							ProcessDataEntry(document, child, prependText, entry.Address.Value + addressOffset, stackedTag);
						}
						break;
					default:
						throw new SiemensException("Unsupported data type for WinCC alarms: " + entry.Name + ", " + entry.DataType.ToString());
				}

			}

			

			void WriteComfortAdvancedTagRow(SLDocument document, DataBlock dataBlock, string connectionName)
			{

				if (document.GetCurrentWorksheetName() != TagWorksheetName)
				{
					document.SelectWorksheet(TagWorksheetName);
				}

				string dataType, address;
				
				var dbLength = dataBlock.CalculateSize(parentPlc).Byte;

				var wordLength = (int)Math.Ceiling(dbLength / 2.0);

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

                var row = WinccConstants.GetTagRow(exportType, portalVersion);
                row[WinccExportField.Name] = dataBlock.Name;
                row[WinccExportField.Connection] = connectionName;
                row[WinccExportField.DataType] = dataType;
                row[WinccExportField.Length] = ((wordLength + 1) * 2).ToString();
                row[WinccExportField.Address] = address;

                var column = 1;
                foreach (var item in row)
                {
                    document.SetCellStyle(currentTagRow, column, genericStyle);
                    document.SetCellValue(currentTagRow, column++, item);
                }
				
				currentTagRow++;

			}

			void WriteProfessionalTagRow(SLDocument document, string tag, string connectionName)
			{

				if (document.GetCurrentWorksheetName() != TagWorksheetName)
				{
					document.SelectWorksheet(TagWorksheetName);
				}

                var row = WinccConstants.GetTagRow(exportType, portalVersion);
                row[WinccExportField.Name] = tag.Replace('.', '_');
                row[WinccExportField.Connection] = connectionName;
                row[WinccExportField.PlcTag] = tag;

                var column = 1;
                foreach (var item in row)
                {
                    document.SetCellStyle(currentTagRow, column, genericStyle);
                    document.SetCellValue(currentTagRow, column++, item);
                }

				currentTagRow++;

			}

			int AddressToTriggerBit(Address address)
			{
				if (address.Byte % 2 == 0)
				{
					return (address.Byte + 1) * 8 + address.Bit;
				}
				else
				{
					return (address.Byte - 1) * 8 + address.Bit;
				}
			}

		}

		private static void WriteXlsxHeaders(SLDocument document, WinccExportType exportType, TiaPortalVersion version)
		{
			var name = document.GetCurrentWorksheetName();
			if (name != TagWorksheetName)
			{
				document.RenameWorksheet(name, TagWorksheetName);
				document.SelectWorksheet(TagWorksheetName);
			}

            var column = 1;
            foreach (var header in WinccConstants.GetTagHeaders(exportType, version))
            {
                document.SetCellValue(1, column++, header);
            }
			
			document.AddWorksheet(AlarmWorksheetName);

            column = 1;
            foreach (var header in WinccConstants.GetAlarmHeaders(exportType, version))
            {
                document.SetCellValue(1, column++, header);
            }

		}
		
		#endregion

        private static class WinccConstants
        {

            #region Constants

            internal const string AlarmWorksheetName = "DiscreteAlarms";
            internal const string TagWorksheetName = "Hmi Tags";
            internal const string WinccNoValue = "<No value>";
            internal const string WinccNoValueCapital = "<No Value>";
            internal const string WinccZero = "0";
            internal const string WinccFalse = "False";
            internal const string WinccNone = "None";
            internal const string WinccBool = "Bool";

            #endregion

            #region Column Mappings

            private static readonly Dictionary<WinccExportField, int> s_professionalV14TagsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Name, 0 },
                { WinccExportField.Connection, 2 },
                { WinccExportField.PlcTag, 3 },
            };

            private static readonly IEnumerable<WinccExportColumn> s_professionalV14Tags = new WinccExportColumn[]
            {
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Path", "Default tag table"),
                new WinccExportColumn("Connection", null),
                new WinccExportColumn("PLC tag", null),
                new WinccExportColumn("DataType", WinccBool),
                new WinccExportColumn("HMI DataType", WinccBool),
                new WinccExportColumn("Length", "1"),
                new WinccExportColumn("Coding", "Binary"),
                new WinccExportColumn("Access Method", "Symbolic access"),
                new WinccExportColumn("Address", WinccNoValue),
                new WinccExportColumn("Start value", WinccNoValue),
                new WinccExportColumn("Quality Code", WinccNoValue),
                new WinccExportColumn("Persistency", WinccFalse),
                new WinccExportColumn("Substitute value", WinccFalse),
                new WinccExportColumn("Tag value [en-US]", WinccNoValue),
                new WinccExportColumn("Update Mode", "Client'=/Server wide"),
                new WinccExportColumn("Comment [en-US]", WinccNoValue),
                new WinccExportColumn("Limit Upper 2 Type", WinccNone),
                new WinccExportColumn("Limit Upper 2", WinccNoValue),
                new WinccExportColumn("Limit Lower 2 Type", WinccNone),
                new WinccExportColumn("Limit Lower 2", WinccNoValue),
                new WinccExportColumn("Linear scaling", WinccFalse),
                new WinccExportColumn("End value PLC", "10"),
                new WinccExportColumn("Start value PLC", WinccZero),
                new WinccExportColumn("End value HMI", "100"),
                new WinccExportColumn("Start value HMI", WinccZero),
                new WinccExportColumn("Synchronization", WinccFalse)
            };

            private static readonly Dictionary<WinccExportField, int> s_comfortAdvancedV14TagsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Name, 0 },
                { WinccExportField.Connection, 2 },
                { WinccExportField.DataType, 4 },
                { WinccExportField.Length, 5 },
                { WinccExportField.Address, 8 },
            };

            private static readonly IEnumerable<WinccExportColumn> s_comfortAdvancedV14Tags = new WinccExportColumn[]
            {
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Path", "Default tag table"),
                new WinccExportColumn("Connection", null),
                new WinccExportColumn("Plc tag", WinccNoValue),
                new WinccExportColumn("DataType", null),
                new WinccExportColumn("Length", null),
                new WinccExportColumn("Coding", "Binary"),
                new WinccExportColumn("Access Method", "Absolute access"),
                new WinccExportColumn("Address", null),
                new WinccExportColumn("Indirect addressing", WinccFalse),
                new WinccExportColumn("Index tag", WinccNoValue),
                new WinccExportColumn("Start value", WinccNoValue),
                new WinccExportColumn("ID tag", "0"),
                new WinccExportColumn("Display name [en-US]", WinccNoValue),
                new WinccExportColumn("Comment [en-US]", WinccNoValue),
                new WinccExportColumn("Acquisition mode", "Continuous"),
                new WinccExportColumn("Acquisition cycle", "1 s"),
                new WinccExportColumn("Range Maximum Type", WinccNone),
                new WinccExportColumn("Range Maximum", WinccNoValue),
                new WinccExportColumn("Range Minimum Type", WinccNone),
                new WinccExportColumn("Range Minimum", WinccNoValue),
                new WinccExportColumn("Linear scaling", WinccFalse),
                new WinccExportColumn("End value Plc", "10"),
                new WinccExportColumn("Start value Plc", "0"),
                new WinccExportColumn("End value HMI", "100"),
                new WinccExportColumn("Start value HMI", "0"),
                new WinccExportColumn("Gmp relevant", WinccFalse),
                new WinccExportColumn("Confirmation Type", WinccNone),
                new WinccExportColumn("Mandatory Commenting", WinccFalse)
            };

            private static readonly Dictionary<WinccExportField, int> s_professionalV14AlarmsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Id, 0 },
                { WinccExportField.Name, 1 },
                { WinccExportField.AlarmText, 2 },
                { WinccExportField.TriggerTag, 5 },
                { WinccExportField.InfoText, 15 },
            };

            private static readonly IEnumerable<WinccExportColumn> s_professionalV14Alarms = new WinccExportColumn[]
            {
                new WinccExportColumn("ID", null),
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Alarm text [en-US], Alarm text 1", null),
                new WinccExportColumn("FieldInfo [Alarm text 1]", WinccNoValue),
                new WinccExportColumn("Class", "Errors"),
                new WinccExportColumn("Trigger tag", null),
                new WinccExportColumn("Trigger bit", WinccZero),
                new WinccExportColumn("Trigger mode", "On rising edge"),
                new WinccExportColumn("Acknowledgement tag", WinccNoValue),
                new WinccExportColumn("Acknowledgement bit", WinccZero),
                new WinccExportColumn("Status tag", WinccNoValue),
                new WinccExportColumn("Status bit", WinccZero),
                new WinccExportColumn("Group", WinccNoValue),
                new WinccExportColumn("Priority", WinccZero),
                new WinccExportColumn("Single acknowledgement", WinccFalse),
                new WinccExportColumn("Info text [en-US], Info text", null),
                new WinccExportColumn("Additional text 1 [en-US], Alarm text 2", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 2]", WinccNoValue),
                new WinccExportColumn("Additional text 2 [en-US], Alarm text 3", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 3]", WinccNoValue),
                new WinccExportColumn("Additional text 3 [en-US], Alarm text 4", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 4]", WinccNoValue),
                new WinccExportColumn("Additional text 4 [en-US], Alarm text 5", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 5]", WinccNoValue),
                new WinccExportColumn("Additional text 5 [en-US], Alarm text 6", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 6]", WinccNoValue),
                new WinccExportColumn("Additional text 6 [en-US], Alarm text 7", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 7]", WinccNoValue),
                new WinccExportColumn("Additional text 7 [en-US], Alarm text 8", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 8]", WinccNoValue),
                new WinccExportColumn("Additional text 8 [en-US], Alarm text 9", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 9]", WinccNoValue),
                new WinccExportColumn("Additional text 9 [en-US], Alarm text 10", WinccNoValue),
                new WinccExportColumn("FieldInfo [Alarm text 10]", WinccNoValue),
                new WinccExportColumn("Alarm parameter 1", WinccNoValue),
                new WinccExportColumn("Alarm parameter 2", WinccNoValue),
                new WinccExportColumn("Alarm parameter 3", WinccNoValue),
                new WinccExportColumn("Alarm parameter 4", WinccNoValue),
                new WinccExportColumn("Alarm parameter 5", WinccNoValue),
                new WinccExportColumn("Alarm parameter 6", WinccNoValue),
                new WinccExportColumn("Alarm parameter 7", WinccNoValue),
                new WinccExportColumn("Alarm parameter 8", WinccNoValue),
                new WinccExportColumn("Alarm parameter 9", WinccNoValue),
                new WinccExportColumn("Alarm parameter 10", WinccNoValue),
                new WinccExportColumn("Alarm annunciation", WinccFalse),
                new WinccExportColumn("Display suppression mask", WinccZero),
                new WinccExportColumn("PLC number", WinccZero),
                new WinccExportColumn("CPU number", WinccZero)
            };

            private static readonly Dictionary<WinccExportField, int> s_comfortAdvancedV14AlarmsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Id, 0 },
                { WinccExportField.Name, 1 },
                { WinccExportField.AlarmText, 2 },
                { WinccExportField.TriggerTag, 5 },
                { WinccExportField.TriggerBit, 6 },
                { WinccExportField.InfoText, 13 },
            };

            private static readonly IEnumerable<WinccExportColumn> s_comfortAdvancedV14Alarms = new WinccExportColumn[]
            {
                new WinccExportColumn("ID", null),
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Alarm text [en-US], Alarm text", null),
                new WinccExportColumn("FieldInfo [Alarm text]", WinccNoValue),
                new WinccExportColumn("Class", "Errors"),
                new WinccExportColumn("Trigger tag", null),
                new WinccExportColumn("Trigger bit", null),
                new WinccExportColumn("Acknowledgement tag", WinccNoValue),
                new WinccExportColumn("Acknowledgement bit", WinccZero),
                new WinccExportColumn("Plc acknowledgement tag", WinccNoValue),
                new WinccExportColumn("Plc acknowledgement bit", WinccZero),
                new WinccExportColumn("Group", WinccNoValue),
                new WinccExportColumn("Report", WinccFalse),
                new WinccExportColumn("Info text [en-US], Info text", null)
            };

            private static readonly Dictionary<WinccExportField, int> s_comfortAdvancedV15TagsLookup = WinccConstants.s_comfortAdvancedV14TagsLookup;

            private static readonly IEnumerable<WinccExportColumn> s_comfortAdvancedV15Tags = new WinccExportColumn[]
            {
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Path", "Default tag table"),
                new WinccExportColumn("Connection", null),
                new WinccExportColumn("PLC tag", WinccNoValueCapital),
                new WinccExportColumn("DataType", null),
                new WinccExportColumn("Length", null),
                new WinccExportColumn("Coding", "Binary"),
                new WinccExportColumn("Access Method", "Absolute access"),
                new WinccExportColumn("Address", null),
                new WinccExportColumn("Indirect addressing", WinccFalse),
                new WinccExportColumn("Index tag", WinccNoValueCapital),
                new WinccExportColumn("Start value", WinccNoValueCapital),
                new WinccExportColumn("ID tag", "0"),
                new WinccExportColumn("Display name [en-US]", WinccNoValueCapital),
                new WinccExportColumn("Comment [en-US]", WinccNoValueCapital),
                new WinccExportColumn("Acquisition mode", "Continuous"),
                new WinccExportColumn("Acquisition cycle", "1 s"),
                new WinccExportColumn("Limit Upper 2 Type", WinccNone),
                new WinccExportColumn("Limit Upper 2", WinccNoValueCapital),
                new WinccExportColumn("Limit Upper 1 Type", WinccNone),
                new WinccExportColumn("Limit Upper 1", WinccNoValueCapital),
                new WinccExportColumn("Limit Lower 2 Type", WinccNone),
                new WinccExportColumn("Limit Lower 2", WinccNoValueCapital),
                new WinccExportColumn("Limit Lower 1 Type", WinccNone),
                new WinccExportColumn("Limit Lower 1", WinccNoValueCapital),
                new WinccExportColumn("Linear scaling", WinccFalse),
                new WinccExportColumn("End value PLC", "10"),
                new WinccExportColumn("Start value PLC", "0"),
                new WinccExportColumn("End value HMI", "100"),
                new WinccExportColumn("Start value HMI", "0"),
                new WinccExportColumn("Gmp relevant", WinccFalse),
                new WinccExportColumn("Confirmation Type", WinccNone),
                new WinccExportColumn("Mandatory Commenting", WinccFalse)
            };

            private static readonly Dictionary<WinccExportField, int> s_comfortAdvancedV15AlarmsLookup = WinccConstants.s_comfortAdvancedV14AlarmsLookup;

            private static readonly IEnumerable<WinccExportColumn> s_comfortAdvancedV15Alarms = WinccConstants.s_comfortAdvancedV14Alarms;

            #endregion

            #region Generation Methods

            public static IEnumerable<string> GetAlarmHeaders(WinccExportType exportType, TiaPortalVersion portalVersion)
            {
                switch (portalVersion)
                {
                    case TiaPortalVersion.V14:
                    case TiaPortalVersion.V14SP1:
                        switch (exportType)
                        {
                            case WinccExportType.ComfortAdvanced:
                                return WinccConstants.s_comfortAdvancedV14Alarms.Select(a => a.Header);
                            case WinccExportType.Professional:
                                return WinccConstants.s_professionalV14Alarms.Select(a => a.Header);
                        }
                        break;
                    case TiaPortalVersion.V15:
                        switch (exportType)
                        {
                            case WinccExportType.ComfortAdvanced:
                                return WinccConstants.s_comfortAdvancedV15Alarms.Select(a => a.Header);
                        }
                        break;
                }

                return null;
            }

            public static IEnumerable<string> GetTagHeaders(WinccExportType exportType, TiaPortalVersion portalVersion)
            {
                switch (portalVersion)
                {
                    case TiaPortalVersion.V14:
                    case TiaPortalVersion.V14SP1:
                        switch (exportType)
                        {
                            case WinccExportType.ComfortAdvanced:
                                return WinccConstants.s_comfortAdvancedV14Tags.Select(a => a.Header);
                            case WinccExportType.Professional:
                                return WinccConstants.s_professionalV14Tags.Select(a => a.Header);
                        }
                        break;
                    case TiaPortalVersion.V15:
                        switch (exportType)
                        {
                            case WinccExportType.ComfortAdvanced:
                                return WinccConstants.s_comfortAdvancedV15Tags.Select(a => a.Header);
                        }
                        break;
                }

                return null;
            }

            public static WinccExportRow GetAlarmRow(WinccExportType exportType, TiaPortalVersion portalVersion) => new WinccExportRow(exportType, portalVersion, false);

            public static WinccExportRow GetTagRow(WinccExportType exportType, TiaPortalVersion portalVersion) => new WinccExportRow(exportType, portalVersion, true);

            #endregion

            #region Helper Classes

            internal class WinccExportColumn
            {
                public string Header { get; set; }

                public string DefaultValue { get; set; }

                public WinccExportColumn(string header, string defaultValue)
                {
                    this.Header = header;
                    this.DefaultValue = defaultValue;
                }

            }

            internal class WinccExportRow : IEnumerable<string>
            {

                private readonly List<string> _values;
                private readonly Dictionary<WinccExportField, int> _lookup;

                public string this[WinccExportField field]
                {
                    get => this._values[this._lookup[field]];
                    set => this._values[this._lookup[field]] = value;
                }

                public WinccExportRow(WinccExportType exportType, TiaPortalVersion portalVersion, bool isTags = false)
                {
                    if (isTags)
                    {
                        switch (portalVersion)
                        {
                            case TiaPortalVersion.V14:
                            case TiaPortalVersion.V14SP1:
                                switch (exportType)
                                {
                                    case WinccExportType.ComfortAdvanced:
                                        this._values = new List<string>(WinccConstants.s_comfortAdvancedV14Tags.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants.s_comfortAdvancedV14TagsLookup;
                                        break;
                                    case WinccExportType.Professional:
                                        this._values = new List<string>(WinccConstants.s_professionalV14Tags.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants.s_professionalV14TagsLookup;
                                        break;
                                }
                                break;
                            case TiaPortalVersion.V15:
                                switch (exportType)
                                {
                                    case WinccExportType.ComfortAdvanced:
                                        this._values = new List<string>(WinccConstants.s_comfortAdvancedV15Tags.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants.s_comfortAdvancedV15TagsLookup;
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        switch (portalVersion)
                        {
                            case TiaPortalVersion.V14:
                            case TiaPortalVersion.V14SP1:
                                switch (exportType)
                                {
                                    case WinccExportType.ComfortAdvanced:
                                        this._values = new List<string>(WinccConstants.s_comfortAdvancedV14Alarms.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants.s_comfortAdvancedV14AlarmsLookup;
                                        break;
                                    case WinccExportType.Professional:
                                        this._values = new List<string>(WinccConstants.s_professionalV14Alarms.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants.s_professionalV14AlarmsLookup;
                                        break;
                                }
                                break;
                            case TiaPortalVersion.V15:
                                switch (exportType)
                                {
                                    case WinccExportType.ComfortAdvanced:
                                        this._values = new List<string>(WinccConstants.s_comfortAdvancedV15Alarms.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants.s_comfortAdvancedV15AlarmsLookup;
                                        break;
                                }
                                break;
                        }
                    }
                }

                public IEnumerator<string> GetEnumerator()
                {
                    return ((IEnumerable<string>)this._values).GetEnumerator();
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return ((IEnumerable<string>)this._values).GetEnumerator();
                }
            }

            #endregion

        }

	}
}
