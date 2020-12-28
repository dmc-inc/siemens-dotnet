using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dmc.IO;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Portal.Plc;
using Dmc.Siemens.Common.Export.Base;
using Dmc.Siemens.Portal;
using System.Collections;
using OfficeOpenXml;
using System.Text;

namespace Dmc.Siemens.Common.Export
{
	public static class Wincc7Configuration
    {

        #region Private Fields

        private const string AlarmWorksheetName = "Messages";
        private const string TagWorksheetName = "Tags";
        private const string MessageGroupSheetName = "Message Groups";

		#endregion

		#region Public Methods

		public static void Create(IEnumerable<IBlock> blocks, string path, PortalPlc parentPlc, WinccExportType exportType, TiaPortalVersion portalVersion)
		{
			if (blocks == null)
				throw new ArgumentNullException(nameof(blocks));
			IEnumerable<DataBlock> dataBlocks;
			if ((dataBlocks = blocks.OfType<DataBlock>())?.Count() <= 0)
				throw new ArgumentException("Blocks does not contain any valid DataBlocks.", nameof(blocks));

			Wincc7Configuration.CreateInternal(dataBlocks, path, parentPlc, exportType, portalVersion);
		}

		public static void Create(IBlock block, string path, PortalPlc parentPlc, WinccExportType exportType, TiaPortalVersion portalVersion)
		{
			Wincc7Configuration.Create(new[] { block }, path, parentPlc, exportType, portalVersion);
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
            var currentGroupRow = 2;
			string dataBlockName;
            string udtName;

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

			using (var document = new ExcelPackage())
			{
                var tagWorksheet = document.Workbook.Worksheets.Add(TagWorksheetName);
                var alarmWorksheet = document.Workbook.Worksheets.Add(AlarmWorksheetName);
                var groupWorksheet = document.Workbook.Worksheets.Add(MessageGroupSheetName);

                Wincc7Configuration.WriteXlsxHeaders(document.Workbook, exportType, portalVersion);

                var groupID = 1000;

                foreach (var db in dataBlocks)
				{
                    db.CalcluateAddresses(parentPlc);
                    dataBlockName = db.Name;

                    var groupName = AddSpacesToSentence(db.Name.Replace("dbErrors", string.Empty).Replace("dbError", string.Empty).Replace("_", string.Empty), true);
                    
                    groupID += 1;

                    var group = new Wincc7Group(groupName, groupID);

                    WriteWinCC7GroupRow(groupWorksheet, group);
                    foreach (var entry in db.Children)
					{
						ProcessDataEntry(alarmWorksheet, tagWorksheet, groupWorksheet, entry, "", group, db.Name);
					}
                    
                    
                }


				using (Stream file = new FileStream(path, FileMode.Create))
				{
					document.SaveAs(file);
				}
			}

			void ProcessDataEntry(ExcelWorksheet alarmWorksheet, ExcelWorksheet tagWorksheet, ExcelWorksheet groupWorksheet, DataEntry entry, string prependText, Wincc7Group group, string prependTag = "")
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
                else if (prependText.EndsWith("_"))
                    throw new Exception();
					//stackedTag = prependTag + "_" + entry.Name;
				else
					stackedTag = prependTag + "_" + entry.Name;

                var newId = group.ID + 1;

				switch (entry.DataType)
				{
					case DataType.BOOL:
						var alarmNumber = currentAlarmRow - 1;

                        WriteWinCC7TagRow(tagWorksheet, string.Concat(stackedTag, "_Ack"), string.Concat(stackedComment, " Ack"));

                        var row = Wincc7Constants.GetAlarmRow(exportType, portalVersion);
                        row[WinccExportField.Id] = alarmNumber.ToString();
                        row[WinccExportField.TriggerTag] = stackedTag;
                        row[WinccExportField.AckTag] = stackedTag + "_Ack";
                        row[WinccExportField.Group] = group.Name;
                        row[WinccExportField.GroupENU] = group.Name;
                        row[WinccExportField.GroupID] = group.ID.ToString();
                        row[WinccExportField.InfoText] = stackedComment;
                        


                        var column = 1;
                        foreach (var item in row)
                        {
                            alarmWorksheet.Cells[currentAlarmRow, column].Style.Numberformat.Format = "@";
                            alarmWorksheet.Cells[currentAlarmRow, column++].Value = item;
                        }

                        currentAlarmRow++;
                        

                        break;
					case DataType.UDT:
                        entry.CalcluateAddresses(parentPlc);

                        
                        var newGroup = new Wincc7Group(entry.Name.Replace("_", " "), newId, group);

                        WriteWinCC7GroupRow(groupWorksheet, newGroup);
                        foreach (var newEntry in entry.Children)
                        {
                            
                            ProcessDataEntry(alarmWorksheet, tagWorksheet, groupWorksheet, newEntry, entry.Name, newGroup, stackedTag);
                        }
                        break;
					case DataType.STRUCT:
                        entry.CalcluateAddresses(parentPlc);
                        if(entry.Name.Contains("PID"))
                        {
                            var groupName = entry.Name.Replace("_", " ") + "_PID";
                            var newGroupStruct = new Wincc7Group(groupName, newId, group);

                            WriteWinCC7GroupRow(groupWorksheet, newGroupStruct);
                        }
                        foreach (var newEntry in entry.Children)
						{
							ProcessDataEntry(alarmWorksheet, tagWorksheet, groupWorksheet, newEntry, prependText, group, stackedTag);
						}
						break;
					case DataType.ARRAY:
						TagHelper.ResolveArrayChildren(entry, parentPlc);

						// write a new entry for each of the children
						foreach (var child in entry.Children)
						{
							ProcessDataEntry(alarmWorksheet, tagWorksheet, groupWorksheet, child, prependText, group, stackedTag);
						}
						break;
					default:
						throw new SiemensException("Unsupported data type for WinCC alarms: " + entry.Name + ", " + entry.DataType.ToString());
				}

			}


            void WriteWinCC7GroupRow(ExcelWorksheet worksheet, Wincc7Group group)
            {
                

                var row = Wincc7Constants.GetGroupRow(exportType, portalVersion);
                row[WinccExportField.Name] = group.Name;
                row[WinccExportField.Id] = group.ID.ToString();
                if (group.Parent != null)
                {
                    row[WinccExportField.Parent] = group.Parent.Name;
                }
                else
                {
                    row[WinccExportField.Parent] = "";
                }
                
                row[WinccExportField.Layer] = group.Layer.ToString();

                var column = 1;
                foreach (var item in row)
                {
                    worksheet.Cells[currentGroupRow, column].Style.Numberformat.Format = "@";
                    worksheet.Cells[currentGroupRow, column++].Value = item;
                }

                currentGroupRow++;
                

            }

            void WriteWinCC7TagRow(ExcelWorksheet worksheet, string name, string comment)
            {


                var row = Wincc7Constants.GetTagRow(exportType, portalVersion);
                row[WinccExportField.Name] = name;
                row[WinccExportField.InfoText] = comment;
                row[WinccExportField.Connection] = "Internal Tags";
                row[WinccExportField.Group] = "AlarmACKTags";

                var column = 1;
                foreach (var item in row)
                {
                    worksheet.Cells[currentTagRow, column].Style.Numberformat.Format = "@";
                    worksheet.Cells[currentTagRow, column++].Value = item;
                }

                currentTagRow++;


            }

        }

        private static void WriteXlsxHeaders(ExcelWorkbook workbook, WinccExportType exportType, TiaPortalVersion version)
		{
            var column = 1;
            var tagWorksheet = workbook.Worksheets[TagWorksheetName];
            foreach (var header in Wincc7Constants.GetTagHeaders(exportType, version))
            {
                tagWorksheet.Cells[1, column++].Value = header;
            }
			
			var alarmWorksheet = workbook.Worksheets[AlarmWorksheetName];

            column = 1;
            foreach (var header in Wincc7Constants.GetAlarmHeaders(exportType, version))
            {
                alarmWorksheet.Cells[1, column++].Value = header;
            }

            var groupWorksheet = workbook.Worksheets[MessageGroupSheetName];

            column = 1;
            foreach (var header in Wincc7Constants.GetGroupHeaders(exportType, version))
            {
                groupWorksheet.Cells[1, column++].Value = header;
            }

        }

        private static string AddSpacesToSentence(string text, bool preserveAcronyms)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            var newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (var i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if (char.IsUpper(text[i - 1]))
                        if (preserveAcronyms && i < text.Length - 1 && !char.IsUpper(text[i - 1]))
                            newText.Append(' ');
                        else
                            ;
                    else if (text[i - 1] != ' ')
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString();
        }


        #endregion

        private static class Wincc7Constants
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

            private static readonly Dictionary<WinccExportField, int> s_winCC7GroupsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Name, 0 },
                { WinccExportField.Id, 1 },
                { WinccExportField.Parent, 2 },
                { WinccExportField.Layer, 3 },
            };

            private static readonly IEnumerable<Wincc7ExportColumn> s_winCC7Groups = new Wincc7ExportColumn[]
            {
                new Wincc7ExportColumn("Name",null),
                new Wincc7ExportColumn("Message Groups (ID)",null),
                new Wincc7ExportColumn("Source",null),
                new Wincc7ExportColumn("Layer",null),
                new Wincc7ExportColumn("Status tag",""),
                new Wincc7ExportColumn("Status bit","0"),
                new Wincc7ExportColumn("Lock tag",""),
                new Wincc7ExportColumn("Lock bit","0"),
                new Wincc7ExportColumn("Acknowledgment tag",""),
                new Wincc7ExportColumn("Acknowledgment bit","0"),
                new Wincc7ExportColumn("Hide tag",""),
                new Wincc7ExportColumn("Author","0"),


        };


            private static readonly Dictionary<WinccExportField, int> s_winCC7AlarmsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Id, 0 },
                { WinccExportField.TriggerTag, 1 },
                { WinccExportField.AckTag, 5 },
                { WinccExportField.Group, 9 },
                { WinccExportField.GroupID, 11 },
                { WinccExportField.GroupENU, 13 },
                { WinccExportField.ProcessAreaID, 14 },
                { WinccExportField.ProcessAreaENU, 16 },
                { WinccExportField.InfoText, 19 },
            };

            private static readonly IEnumerable<Wincc7ExportColumn> s_winCC7Alarms = new Wincc7ExportColumn[]
            {
                new Wincc7ExportColumn("Number",null),
                new Wincc7ExportColumn("Message tag",null),
                new Wincc7ExportColumn("Message bit","0"),
                new Wincc7ExportColumn("Status tag",""),
                new Wincc7ExportColumn("Status bit","0"),
                new Wincc7ExportColumn("Acknowledgment tag",null),
                new Wincc7ExportColumn("Acknowledgment bit","0"),
                new Wincc7ExportColumn("Message class",""),
                new Wincc7ExportColumn("Message Type","Alarm"),
                new Wincc7ExportColumn("Message Group", null),
                new Wincc7ExportColumn("Priority","2"),
                new Wincc7ExportColumn("Message Group (ID)",null),
                new Wincc7ExportColumn("Message Group (DEU)",""),
                new Wincc7ExportColumn("Message Group (ENU)",null),
                new Wincc7ExportColumn("Process Area (ID)",""),
                new Wincc7ExportColumn("Process Area (DEU)",""),
                new Wincc7ExportColumn("Process Area (ENU)",""),
                new Wincc7ExportColumn("Message Text (ID)",""),
                new Wincc7ExportColumn("Message Text (DEU)",""),
                new Wincc7ExportColumn("Message Text (ENU)",null),
                new Wincc7ExportColumn("Unit (ID)","0"),
                new Wincc7ExportColumn("Unit (DEU)",""),
                new Wincc7ExportColumn("Unit (ENU)",""),
                new Wincc7ExportColumn("Value text group (ID)","0"),
                new Wincc7ExportColumn("Value text group (DEU)",""),
                new Wincc7ExportColumn("Value text group (ENU)",""),
                new Wincc7ExportColumn("Value",""),
                new Wincc7ExportColumn("Location",""),
                new Wincc7ExportColumn("Cause",""),
                new Wincc7ExportColumn("Status (Validity)",""),
                new Wincc7ExportColumn("Additional cause",""),
                new Wincc7ExportColumn("User Name DAF",""),
                new Wincc7ExportColumn("Supplementary Information",""),
                new Wincc7ExportColumn("T.st.",""),
                new Wincc7ExportColumn("SICAM Flag",""),
                new Wincc7ExportColumn("SICAM Coloring",""),
                new Wincc7ExportColumn("Info text",""),
                new Wincc7ExportColumn("Single acknowledgment","0"),
                new Wincc7ExportColumn("Central signaling device","0"),
                new Wincc7ExportColumn("Archived","1"),
                new Wincc7ExportColumn("Falling edge","0"),
                new Wincc7ExportColumn("Triggers action","0"),
                new Wincc7ExportColumn("Extended associated value data","0"),
                new Wincc7ExportColumn("Help","0"),
                new Wincc7ExportColumn("Hide mask","1"),
                new Wincc7ExportColumn("Format DLL",""),
                new Wincc7ExportColumn("Loop In Alarm","0"),
                new Wincc7ExportColumn("Function name",""),
                new Wincc7ExportColumn("Function parameters",""),
                new Wincc7ExportColumn("Controller number","0"),
                new Wincc7ExportColumn("CPU Number","0"),
                new Wincc7ExportColumn("Address",""),
                new Wincc7ExportColumn("Version","0"),
                new Wincc7ExportColumn("Author ID","0"),
                new Wincc7ExportColumn("Connection",""),
                new Wincc7ExportColumn("Author","0"),
                new Wincc7ExportColumn("Response time","0"),
                new Wincc7ExportColumn("Description",""),
                new Wincc7ExportColumn("Description (DEU)",""),
                new Wincc7ExportColumn("Description (ENU)",""),
                new Wincc7ExportColumn("Reasons",""),
                new Wincc7ExportColumn("Reasons (DEU)",""),
                new Wincc7ExportColumn("Reasons (ENU)",""),
                new Wincc7ExportColumn("Action",""),
                new Wincc7ExportColumn("Action (DEU)",""),
                new Wincc7ExportColumn("Action (ENU)",""),
                new Wincc7ExportColumn("Effect",""),
                new Wincc7ExportColumn("Effect (DEU)",""),
                new Wincc7ExportColumn("Effect (ENU)",""),




            };

            private static readonly Dictionary<WinccExportField, int> s_winCC7TagsLookup = new Dictionary<WinccExportField, int>
            {
                { WinccExportField.Name, 0 },
                { WinccExportField.InfoText, 1 },
                { WinccExportField.Connection, 5 },
                { WinccExportField.Group, 6 },

            };

            private static readonly IEnumerable<Wincc7ExportColumn> s_winCC7Tags = new Wincc7ExportColumn[]
            {
                new Wincc7ExportColumn("Name",null),
                new Wincc7ExportColumn("Comment",null),
                new Wincc7ExportColumn("Data type","Binary Tag"),
                new Wincc7ExportColumn("Length","1"),
                new Wincc7ExportColumn("Format adaptation",""),
                new Wincc7ExportColumn("Connection",null),
                new Wincc7ExportColumn("Group",null),
                new Wincc7ExportColumn("Address",""),
                new Wincc7ExportColumn("Linear scaling","0"),
                new Wincc7ExportColumn("AS value range from",""),
                new Wincc7ExportColumn("AS value range to",""),
                new Wincc7ExportColumn("OS value range from",""),
                new Wincc7ExportColumn("OS value range to",""),
                new Wincc7ExportColumn("Low limit",""),
                new Wincc7ExportColumn("High limit",""),
                new Wincc7ExportColumn("Start value",""),
                new Wincc7ExportColumn("Substitute value",""),
                new Wincc7ExportColumn("Substitute value at low limit","0"),
                new Wincc7ExportColumn("Substitute value at high limit","0"),
                new Wincc7ExportColumn("Substitute value as start value","0"),
                new Wincc7ExportColumn("Substitute value on connection errors","0"),
                new Wincc7ExportColumn("Computer-local","0"),
                new Wincc7ExportColumn("Synchronization","1"),
                new Wincc7ExportColumn("Runtime persistence","1"),
                new Wincc7ExportColumn("AS tag name",""),
                new Wincc7ExportColumn("Name space",""),
                new Wincc7ExportColumn("OPC write protection","0"),
                new Wincc7ExportColumn("OPC read protection","0"),
                new Wincc7ExportColumn("Good Manufacturing Practices","0"),
                new Wincc7ExportColumn("WinCC Cloud","0"),
                new Wincc7ExportColumn("WinCC Cloud Cycle","1 min"),

            };

            #endregion

            #region Generation Methods

            public static IEnumerable<string> GetAlarmHeaders(WinccExportType exportType, TiaPortalVersion portalVersion)
            {
                switch (portalVersion)
                {
                    case TiaPortalVersion.V14:
                    case TiaPortalVersion.V14SP1:
                    case TiaPortalVersion.V15:
                        switch (exportType)
                        {
                            case WinccExportType.Seven:
                                return Wincc7Constants.s_winCC7Alarms.Select(a => a.Header);
                                break;
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
                    case TiaPortalVersion.V15:
                        switch (exportType)
                        {
                            case WinccExportType.Seven:
                                return Wincc7Constants.s_winCC7Tags.Select(a => a.Header);
                                break;
                        }
                        break;

                }

                return null;
            }

            public static IEnumerable<string> GetGroupHeaders(WinccExportType exportType, TiaPortalVersion portalVersion)
            {
                switch (portalVersion)
                {
                    case TiaPortalVersion.V14:
                    case TiaPortalVersion.V14SP1:
                    case TiaPortalVersion.V15:
                        switch (exportType)
                        {
                            case WinccExportType.Seven:
                                return Wincc7Constants.s_winCC7Groups.Select(a => a.Header);
                                break;
                        }
                        break;

                }

                return null;
            }

            public static Wincc7ExportRow GetAlarmRow(WinccExportType exportType, TiaPortalVersion portalVersion) => new Wincc7ExportRow(exportType, portalVersion, Wincc7RowType.Alarms);

            public static Wincc7ExportRow GetTagRow(WinccExportType exportType, TiaPortalVersion portalVersion) => new Wincc7ExportRow(exportType, portalVersion, Wincc7RowType.Tags);

            public static Wincc7ExportRow GetGroupRow(WinccExportType exportType, TiaPortalVersion portalVersion) => new Wincc7ExportRow(exportType, portalVersion, Wincc7RowType.Groups);

            #endregion

            #region Helper Classes


            internal class Wincc7ExportColumn
            {
                public string Header { get; set; }

                public string DefaultValue { get; set; }

                public Wincc7ExportColumn(string header, string defaultValue)
                {
                    this.Header = header;
                    this.DefaultValue = defaultValue;
                }

            }



            internal class Wincc7ExportRow : IEnumerable<string>
            {

                private readonly List<string> _values;
                private readonly Dictionary<WinccExportField, int> _lookup;

                public string this[WinccExportField field]
                {
                    get => this._values[this._lookup[field]];
                    set => this._values[this._lookup[field]] = value;
                }

                public Wincc7ExportRow(WinccExportType exportType, TiaPortalVersion portalVersion, Wincc7RowType rowType = Wincc7RowType.Tags)
                {
                    switch (rowType)
                    {
                        case Wincc7RowType.Tags:

                            switch (portalVersion)
                            {
                                case TiaPortalVersion.V14:
                                case TiaPortalVersion.V14SP1:
                                case TiaPortalVersion.V15:
                                    switch (exportType)
                                    {
                                        case WinccExportType.Seven:
                                            this._values = new List<string>(Wincc7Constants.s_winCC7Tags.Select(a => a.DefaultValue));
                                            this._lookup = Wincc7Constants.s_winCC7TagsLookup;
                                            break;
                                    }
                                    break;
                            }
                            break;

                        case Wincc7RowType.Alarms:

                            switch (portalVersion)
                            {
                                case TiaPortalVersion.V14:
                                case TiaPortalVersion.V14SP1:
                                case TiaPortalVersion.V15:
                                    switch (exportType)
                                    {
                                        case WinccExportType.Seven:
                                            this._values = new List<string>(Wincc7Constants.s_winCC7Alarms.Select(a => a.DefaultValue));
                                            this._lookup = Wincc7Constants.s_winCC7AlarmsLookup;
                                            break;
                                    }
                                    break;
                            }
                            break;
                        case Wincc7RowType.Groups:
                            switch (portalVersion)
                            {
                                case TiaPortalVersion.V14:
                                case TiaPortalVersion.V14SP1:
                                case TiaPortalVersion.V15:
                                    switch (exportType)
                                    {
                                        case WinccExportType.Seven:
                                            this._values = new List<string>(Wincc7Constants.s_winCC7Groups.Select(a => a.DefaultValue));
                                            this._lookup = Wincc7Constants.s_winCC7GroupsLookup;
                                            break;
                                    }
                                    break;
                            }
                            break;

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

        internal class Wincc7Group
        {
            public Wincc7Group Parent { get; set; }

            public string Name { get; set; }

            public int ID { get; set; }

            public int Layer
            {
                get
                {
                    if (this.Parent == null)
                    {
                        return 1;
                    }
                    else
                        return this.Parent.Layer + 1;
                }
            }

            public Wincc7Group(string name, int iD, Wincc7Group parent = null)
            {
                this.Name = name;
                this.ID = iD;
                this.Parent = parent;
            }
        }

    }
}
