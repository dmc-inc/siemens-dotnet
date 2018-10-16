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

		private const string ALARM_WORKSHEET_NAME = "DiscreteAlarms";
		private const string TAG_WORKSHEET_NAME = "Hmi Tags";
		private static string WINCC_NO_VALUE = "<No value>";
		private static string WINCC_ZERO = "0";
		private static string WINCC_FALSE = "False";
		private static string WINCC_NONE = "None";

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
			SLStyle genericStyle = new SLStyle() { FormatCode = "@" };
			List<string> proTags = new List<string>();

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
						int alarmNumber = currentAlarmRow - 1;

						if (document.GetCurrentWorksheetName() != ALARM_WORKSHEET_NAME)
						{
							document.SelectWorksheet(ALARM_WORKSHEET_NAME);
						}

                        var row = WinccConstants.GetAlarmRow(exportType, TiaPortalVersion.V15);
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

                        int column = 1;
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

                var row = WinccConstants.GetTagRow(exportType, TiaPortalVersion.V15);
                row[WinccExportField.Name] = dataBlock.Name;
                row[WinccExportField.Connection] = connectionName;
                row[WinccExportField.DataType] = dataType;
                row[WinccExportField.Length] = ((wordLength + 1) * 2).ToString();
                row[WinccExportField.Address] = address;

                int column = 1;
                foreach (var item in row)
                {
                    document.SetCellStyle(currentTagRow, column, genericStyle);
                    document.SetCellValue(currentTagRow, column++, item);
                }
				
				currentTagRow++;

			}

			void WriteProfessionalTagRow(SLDocument document, string tag, string connectionName)
			{

				if (document.GetCurrentWorksheetName() != TAG_WORKSHEET_NAME)
				{
					document.SelectWorksheet(TAG_WORKSHEET_NAME);
				}

                var row = WinccConstants.GetTagRow(exportType, TiaPortalVersion.V15);
                row[WinccExportField.Name] = tag.Replace('.', '_');
                row[WinccExportField.Connection] = connectionName;
                row[WinccExportField.PlcTag] = tag;

                int column = 1;
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

            int column = 1;
            foreach (var header in WinccConstants.GetTagHeaders(exportType, TiaPortalVersion.V15))
            {
                document.SetCellValue(1, column++, header);
            }
			
			document.AddWorksheet(ALARM_WORKSHEET_NAME);

            column = 1;
            foreach (var header in WinccConstants.GetAlarmHeaders(exportType, TiaPortalVersion.V15))
            {
                document.SetCellValue(1, column++, header);
            }

		}
		
		#endregion

        private static class WinccConstants
        {

            #region Constants

            internal const string ALARM_WORKSHEET_NAME = "DiscreteAlarms";
            internal const string TAG_WORKSHEET_NAME = "Hmi Tags";
            internal const string WINCC_NO_VALUE = "<No value>";
            internal const string WINCC_NO_VALUE_CAPITAL = "<No Value>";
            internal const string WINCC_ZERO = "0";
            internal const string WINCC_FALSE = "False";
            internal const string WINCC_NONE = "None";
            internal const string WINCC_BOOL = "Bool";

            #endregion

            #region Column Mappings

            private static readonly Dictionary<WinccExportField, int> _professionalV14TagsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Name, 0 },
                { WinccExportField.Connection, 2 },
                { WinccExportField.PlcTag, 3 },
            };

            private static readonly IEnumerable<WinccExportColumn> ProfessionalV14Tags = new WinccExportColumn[]
            {
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Path", "Default tag table"),
                new WinccExportColumn("Connection", null),
                new WinccExportColumn("PLC tag", null),
                new WinccExportColumn("DataType", WINCC_BOOL),
                new WinccExportColumn("HMI DataType", WINCC_BOOL),
                new WinccExportColumn("Length", "1"),
                new WinccExportColumn("Coding", "Binary"),
                new WinccExportColumn("Access Method", "Symbolic access"),
                new WinccExportColumn("Address", WINCC_NO_VALUE),
                new WinccExportColumn("Start value", WINCC_NO_VALUE),
                new WinccExportColumn("Quality Code", WINCC_NO_VALUE),
                new WinccExportColumn("Persistency", WINCC_FALSE),
                new WinccExportColumn("Substitute value", WINCC_FALSE),
                new WinccExportColumn("Tag value [en-US]", WINCC_NO_VALUE),
                new WinccExportColumn("Update Mode", "Client'=/Server wide"),
                new WinccExportColumn("Comment [en-US]", WINCC_NO_VALUE),
                new WinccExportColumn("Limit Upper 2 Type", WINCC_NONE),
                new WinccExportColumn("Limit Upper 2", WINCC_NO_VALUE),
                new WinccExportColumn("Limit Lower 2 Type", WINCC_NONE),
                new WinccExportColumn("Limit Lower 2", WINCC_NO_VALUE),
                new WinccExportColumn("Linear scaling", WINCC_FALSE),
                new WinccExportColumn("End value PLC", "10"),
                new WinccExportColumn("Start value PLC", WINCC_ZERO),
                new WinccExportColumn("End value HMI", "100"),
                new WinccExportColumn("Start value HMI", WINCC_ZERO),
                new WinccExportColumn("Synchronization", WINCC_FALSE)
            };

            private static readonly Dictionary<WinccExportField, int> _comfortAdvancedV14TagsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Name, 0 },
                { WinccExportField.Connection, 2 },
                { WinccExportField.DataType, 4 },
                { WinccExportField.Length, 5 },
                { WinccExportField.Address, 8 },
            };

            private static readonly IEnumerable<WinccExportColumn> ComfortAdvancedV14Tags = new WinccExportColumn[]
            {
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Path", "Default tag table"),
                new WinccExportColumn("Connection", null),
                new WinccExportColumn("Plc tag", WINCC_NO_VALUE),
                new WinccExportColumn("DataType", null),
                new WinccExportColumn("Length", null),
                new WinccExportColumn("Coding", "Binary"),
                new WinccExportColumn("Access Method", "Absolute access"),
                new WinccExportColumn("Address", null),
                new WinccExportColumn("Indirect addressing", WINCC_FALSE),
                new WinccExportColumn("Index tag", WINCC_NO_VALUE),
                new WinccExportColumn("Start value", WINCC_NO_VALUE),
                new WinccExportColumn("ID tag", "0"),
                new WinccExportColumn("Display name [en-US]", WINCC_NO_VALUE),
                new WinccExportColumn("Comment [en-US]", WINCC_NO_VALUE),
                new WinccExportColumn("Acquisition mode", "Continuous"),
                new WinccExportColumn("Acquisition cycle", "1 s"),
                new WinccExportColumn("Range Maximum Type", WINCC_NONE),
                new WinccExportColumn("Range Maximum", WINCC_NO_VALUE),
                new WinccExportColumn("Range Minimum Type", WINCC_NONE),
                new WinccExportColumn("Range Minimum", WINCC_NO_VALUE),
                new WinccExportColumn("Linear scaling", WINCC_FALSE),
                new WinccExportColumn("End value Plc", "10"),
                new WinccExportColumn("Start value Plc", "0"),
                new WinccExportColumn("End value HMI", "100"),
                new WinccExportColumn("Start value HMI", "0"),
                new WinccExportColumn("Gmp relevant", WINCC_FALSE),
                new WinccExportColumn("Confirmation Type", WINCC_NONE),
                new WinccExportColumn("Mandatory Commenting", WINCC_FALSE)
            };

            private static readonly Dictionary<WinccExportField, int> _professionalV14AlarmsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Id, 0 },
                { WinccExportField.Name, 1 },
                { WinccExportField.AlarmText, 2 },
                { WinccExportField.TriggerTag, 5 },
                { WinccExportField.InfoText, 15 },
            };

            private static readonly IEnumerable<WinccExportColumn> ProfessionalV14Alarms = new WinccExportColumn[]
            {
                new WinccExportColumn("ID", null),
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Alarm text [en-US], Alarm text 1", null),
                new WinccExportColumn("FieldInfo [Alarm text 1]", WINCC_NO_VALUE),
                new WinccExportColumn("Class", "Errors"),
                new WinccExportColumn("Trigger tag", null),
                new WinccExportColumn("Trigger bit", WINCC_ZERO),
                new WinccExportColumn("Trigger mode", "On rising edge"),
                new WinccExportColumn("Acknowledgement tag", WINCC_NO_VALUE),
                new WinccExportColumn("Acknowledgement bit", WINCC_ZERO),
                new WinccExportColumn("Status tag", WINCC_NO_VALUE),
                new WinccExportColumn("Status bit", WINCC_ZERO),
                new WinccExportColumn("Group", WINCC_NO_VALUE),
                new WinccExportColumn("Priority", WINCC_ZERO),
                new WinccExportColumn("Single acknowledgement", WINCC_FALSE),
                new WinccExportColumn("Info text [en-US], Info text", null),
                new WinccExportColumn("Additional text 1 [en-US], Alarm text 2", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 2]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 2 [en-US], Alarm text 3", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 3]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 3 [en-US], Alarm text 4", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 4]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 4 [en-US], Alarm text 5", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 5]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 5 [en-US], Alarm text 6", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 6]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 6 [en-US], Alarm text 7", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 7]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 7 [en-US], Alarm text 8", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 8]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 8 [en-US], Alarm text 9", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 9]", WINCC_NO_VALUE),
                new WinccExportColumn("Additional text 9 [en-US], Alarm text 10", WINCC_NO_VALUE),
                new WinccExportColumn("FieldInfo [Alarm text 10]", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 1", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 2", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 3", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 4", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 5", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 6", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 7", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 8", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 9", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm parameter 10", WINCC_NO_VALUE),
                new WinccExportColumn("Alarm annunciation", WINCC_FALSE),
                new WinccExportColumn("Display suppression mask", WINCC_ZERO),
                new WinccExportColumn("PLC number", WINCC_ZERO),
                new WinccExportColumn("CPU number", WINCC_ZERO)
            };

            private static readonly Dictionary<WinccExportField, int> _comfortAdvancedV14AlarmsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Id, 0 },
                { WinccExportField.Name, 1 },
                { WinccExportField.AlarmText, 2 },
                { WinccExportField.TriggerTag, 5 },
                { WinccExportField.TriggerBit, 6 },
                { WinccExportField.InfoText, 13 },
            };

            private static readonly IEnumerable<WinccExportColumn> ComfortAdvancedV14Alarms = new WinccExportColumn[]
            {
                new WinccExportColumn("ID", null),
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Alarm text [en-US], Alarm text", null),
                new WinccExportColumn("FieldInfo [Alarm text]", WINCC_NO_VALUE),
                new WinccExportColumn("Class", "Errors"),
                new WinccExportColumn("Trigger tag", null),
                new WinccExportColumn("Trigger bit", null),
                new WinccExportColumn("Acknowledgement tag", WINCC_NO_VALUE),
                new WinccExportColumn("Acknowledgement bit", WINCC_ZERO),
                new WinccExportColumn("Plc acknowledgement tag", WINCC_NO_VALUE),
                new WinccExportColumn("Plc acknowledgement bit", WINCC_ZERO),
                new WinccExportColumn("Group", WINCC_NO_VALUE),
                new WinccExportColumn("Report", WINCC_FALSE),
                new WinccExportColumn("Info text [en-US], Info text", null)
            };

            private static readonly Dictionary<WinccExportField, int> _comfortAdvancedV15TagsLookup = WinccConstants._comfortAdvancedV14TagsLookup;

            private static readonly IEnumerable<WinccExportColumn> ComfortAdvancedV15Tags = new WinccExportColumn[]
            {
                new WinccExportColumn("Name", null),
                new WinccExportColumn("Path", "Default tag table"),
                new WinccExportColumn("Connection", null),
                new WinccExportColumn("PLC tag", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("DataType", null),
                new WinccExportColumn("Length", null),
                new WinccExportColumn("Coding", "Binary"),
                new WinccExportColumn("Access Method", "Absolute access"),
                new WinccExportColumn("Address", null),
                new WinccExportColumn("Indirect addressing", WINCC_FALSE),
                new WinccExportColumn("Index tag", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("Start value", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("ID tag", "0"),
                new WinccExportColumn("Display name [en-US]", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("Comment [en-US]", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("Acquisition mode", "Continuous"),
                new WinccExportColumn("Acquisition cycle", "1 s"),
                new WinccExportColumn("Limit Upper 2 Type", WINCC_NONE),
                new WinccExportColumn("Limit Upper 2", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("Limit Upper 1 Type", WINCC_NONE),
                new WinccExportColumn("Limit Upper 1", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("Limit Lower 2 Type", WINCC_NONE),
                new WinccExportColumn("Limit Lower 2", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("Limit Lower 1 Type", WINCC_NONE),
                new WinccExportColumn("Limit Lower 1", WINCC_NO_VALUE_CAPITAL),
                new WinccExportColumn("Linear scaling", WINCC_FALSE),
                new WinccExportColumn("End value PLC", "10"),
                new WinccExportColumn("Start value PLC", "0"),
                new WinccExportColumn("End value HMI", "100"),
                new WinccExportColumn("Start value HMI", "0"),
                new WinccExportColumn("Gmp relevant", WINCC_FALSE),
                new WinccExportColumn("Confirmation Type", WINCC_NONE),
                new WinccExportColumn("Mandatory Commenting", WINCC_FALSE)
            };

            private static readonly Dictionary<WinccExportField, int> _comfortAdvancedV15AlarmsLookup = WinccConstants._comfortAdvancedV14AlarmsLookup;

            private static readonly IEnumerable<WinccExportColumn> ComfortAdvancedV15Alarms = WinccConstants.ComfortAdvancedV14Alarms;

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
                                return WinccConstants.ComfortAdvancedV14Alarms.Select(a => a.Header);
                            case WinccExportType.Professional:
                                return WinccConstants.ProfessionalV14Alarms.Select(a => a.Header);
                        }
                        break;
                    case TiaPortalVersion.V15:
                        switch (exportType)
                        {
                            case WinccExportType.ComfortAdvanced:
                                return WinccConstants.ComfortAdvancedV15Alarms.Select(a => a.Header);
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
                                return WinccConstants.ComfortAdvancedV14Tags.Select(a => a.Header);
                            case WinccExportType.Professional:
                                return WinccConstants.ProfessionalV14Tags.Select(a => a.Header);
                        }
                        break;
                    case TiaPortalVersion.V15:
                        switch (exportType)
                        {
                            case WinccExportType.ComfortAdvanced:
                                return WinccConstants.ComfortAdvancedV15Tags.Select(a => a.Header);
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
                    get { return this._values[this._lookup[field]]; }
                    set { this._values[this._lookup[field]] = value; }
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
                                        this._values = new List<string>(WinccConstants.ComfortAdvancedV14Tags.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants._comfortAdvancedV14TagsLookup;
                                        break;
                                    case WinccExportType.Professional:
                                        this._values = new List<string>(WinccConstants.ProfessionalV14Tags.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants._professionalV14TagsLookup;
                                        break;
                                }
                                break;
                            case TiaPortalVersion.V15:
                                switch (exportType)
                                {
                                    case WinccExportType.ComfortAdvanced:
                                        this._values = new List<string>(WinccConstants.ComfortAdvancedV15Tags.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants._comfortAdvancedV15TagsLookup;
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
                                        this._values = new List<string>(WinccConstants.ComfortAdvancedV14Alarms.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants._comfortAdvancedV14AlarmsLookup;
                                        break;
                                    case WinccExportType.Professional:
                                        this._values = new List<string>(WinccConstants.ProfessionalV14Alarms.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants._professionalV14AlarmsLookup;
                                        break;
                                }
                                break;
                            case TiaPortalVersion.V15:
                                switch (exportType)
                                {
                                    case WinccExportType.ComfortAdvanced:
                                        this._values = new List<string>(WinccConstants.ComfortAdvancedV15Alarms.Select(a => a.DefaultValue));
                                        this._lookup = WinccConstants._comfortAdvancedV15AlarmsLookup;
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
