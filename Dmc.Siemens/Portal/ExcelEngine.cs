using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Portal.Base;
using Dmc.IO;
using SpreadsheetLight;
using Dmc.Siemens.Portal.Plc;
using Dmc.Siemens.Common;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Portal.Hmi;

namespace Dmc.Siemens.Portal
{
    public static class ExcelEngine
    {

        #region Private Fields

        private const string PLC_TAG_WORKSHEET_NAME = "PLC Tags";
        private const string CONSTANT_WORKSHEET_NAME = "Constants";
        private const string ALARM_WORKSHEET_NAME = "DiscreteAlarms";
        private const string HMI_TAG_WORKSHEET_NAME = "Hmi Tags";

        #endregion

        #region Public Methods

        public static IEnumerable<PlcTagTable> PlcTagTableFromFile(string filePath)
        {

            if (!File.Exists(filePath))
                throw new ArgumentException($"{filePath} does not point to a valid file");

            using (Stream fileStream = File.OpenRead(filePath))
            {
                using (SLDocument document = new SLDocument(fileStream))
                {
                    return ImportSpreadsheet(document);
                }
            }
                
        }

        public static void PlcTagTableToFile(string path, IEnumerable<PlcTagTable> tagTables)
        {
            if (!FileHelpers.CheckValidFilePath(path, "xlsx"))
                throw new ArgumentException($"{nameof(path)} ({path}) is not a valid file path");
            if (tagTables?.Count() == 0)
                throw new ArgumentException($"{nameof(tagTables)} cannot be empty");

            ExportToFile(path, tagTables);
        }

        public static void PlcTagTableToFile(string path, PlcTagTable tagTable)
        {
            PlcTagTableToFile(path, new[] { tagTable });
        }
		
        #endregion

        #region Private Methods

        #region Import Plc Tag Table

        private static IEnumerable<PlcTagTable> ImportSpreadsheet(SLDocument document)
        {
            HashSet<PlcTagTable> tagTables = new HashSet<PlcTagTable>();
            Dictionary<string, PlcTagTable> tagTableLookup = new Dictionary<string, PlcTagTable>();

			// Check for and navigate to Plc tag tab
			if (!ExcelEngine.ChangeActiveWorksheet(document, ExcelEngine.PLC_TAG_WORKSHEET_NAME))
				throw new SiemensException("File does not contain " + ExcelEngine.PLC_TAG_WORKSHEET_NAME + " tab.");

			// Parse all tags, starting at the row after the headers
			int row = 1;
			PlcTag tag;
			string parentTableName;
			PlcTagTable tagTable;
			while (!string.IsNullOrWhiteSpace(document.GetCellValueAsString(row, 0)))
			{
				// Get or create the parent tag table
				parentTableName = document.GetCellValueAsString(row, 1);
				if (tagTableLookup.ContainsKey(parentTableName))
					tagTable = tagTableLookup[parentTableName];
				else
				{
					tagTable = new PlcTagTable(parentTableName);
					tagTableLookup.Add(parentTableName, tagTable);
					tagTables.Add(tagTable);
				}

				// Create a new tag
				tag = new PlcTag(tagTable, document.GetCellValueAsString(row, 0), document.GetCellValueAsString(row, 3), document.GetCellValueAsString(row, 4),
					TagHelper.ParseDataType(document.GetCellValueAsString(row, 2)), Convert.ToBoolean(document.GetCellValueAsString(row, 5)), Convert.ToBoolean(document.GetCellValueAsString(row, 6)));

				// Add the tag to the tables collection of tags
				tagTable?.PlcTags?.Add(tag);
			}

			// Check for and navigate to Constants tab
			if (!ExcelEngine.ChangeActiveWorksheet(document, ExcelEngine.CONSTANT_WORKSHEET_NAME))
				throw new SiemensException("File does not contain " + ExcelEngine.CONSTANT_WORKSHEET_NAME + " tab.");

			// Parse all constants
			row = 1;
			ConstantsEntry constant;
			while (!string.IsNullOrWhiteSpace(document.GetCellValueAsString(row, 0)))
			{
				// Get or create the parent tag table
				parentTableName = document.GetCellValueAsString(row, 1);
				if (tagTableLookup.ContainsKey(parentTableName))
					tagTable = tagTableLookup[parentTableName];
				else
				{
					tagTable = new PlcTagTable(parentTableName);
					tagTableLookup.Add(parentTableName, tagTable);
					tagTables.Add(tagTable);
				}

				// Parse constant value
				DataType dataType = TagHelper.ParseDataType(document.GetCellValueAsString(row, 2));
				SiemensConverter.TryParse(document.GetCellValueAsString(row, 3), dataType, out object value);

				// Create a new constants entry
				constant = new ConstantsEntry(document.GetCellValueAsString(row, 0), value, dataType);

				tagTable?.Constants?.Add(constant);
			}

			return tagTables;
        }

        #endregion

        #region Export Tag Tables

        private static void ExportToFile(string path, IEnumerable<ITagTable> tagTables)
        {

            using (SLDocument document = new SLDocument())
            {
                foreach (ITagTable tagTable in tagTables)
                {
                    if (tagTable is PlcTagTable)
                        ExportPlcTagTableToFile(document, tagTable as PlcTagTable);
                    else if (tagTable is HmiTagTable)
                        ExportHmiTagTableToFile(document, tagTable as HmiTagTable);
                }

                using (FileStream file = File.Open(path, FileMode.Create))
                {
                    document.SaveAs(file);
                }
            }
        }

        private static void ExportPlcTagTableToFile(SLDocument document, PlcTagTable tagTable)
        {
            if (tagTable?.PlcTags?.Count() > 0)
            {
                foreach (PlcTag tag in tagTable.PlcTags)
                {
                    AddTagToDocument(document, tag);
                }
            }
            if (tagTable?.Constants?.Count() > 0)
            {
                foreach (ConstantsEntry constant in tagTable.Constants)
                {
                    AddConstantToDocument(document, constant);
                }
            }
        }

        private static void ExportHmiTagTableToFile(SLDocument document, HmiTagTable tagTable)
        {
            if (tagTable?.HmiTags?.Count() > 0)
            {
                foreach (HmiTag tag in tagTable.HmiTags)
                {
                    //AddTagToDocument(document, tag);
                }
            }
        }

        private static void AddTagToDocument(SLDocument document, PlcTag tag)
        {

        }

        private static void AddConstantToDocument(SLDocument document, ConstantsEntry constant)
        {

        }

        #endregion

        #region Setup Methods

        private static bool ChangeActiveWorksheet(SLDocument document, string worksheetName, bool renameCurrentWorksheet = false)
        {
            string name = document.GetCurrentWorksheetName();
            // If the current worksheet name is the one we want, dont do anything
            if (name != worksheetName)
            {
                // If rename is requested, rename and select
                if (renameCurrentWorksheet)
                {
                    document.RenameWorksheet(name, worksheetName);
                    return document.SelectWorksheet(worksheetName);
                }
                // If not rename, then check to see if the worksheet already exists in the document
                // If it doesn't exist, create it.  If it does, change to it
                else if (!document.GetWorksheetNames().Contains(worksheetName))
                {
                    return document.AddWorksheet(worksheetName);
                }
                else 
                {
                    return document.SelectWorksheet(worksheetName);
                }
            }
			else
			{
				return true;
			}
        }

        private static void WritePlcTagHeaders(SLDocument document, bool renameCurrentWorksheet)
        {

            ChangeActiveWorksheet(document, PLC_TAG_WORKSHEET_NAME, renameCurrentWorksheet);

            // Write headers
            document.SetCellValue(1, 1, "Name");
            document.SetCellValue(1, 2, "Path");
            document.SetCellValue(1, 3, "Connection");
            document.SetCellValue(1, 4, "Plc tag");
            document.SetCellValue(1, 5, "DataType");
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

        private static void WriteAlarmTagHeaders(SLDocument document, bool renameCurrentWorksheet)
        {
            ChangeActiveWorksheet(document, ALARM_WORKSHEET_NAME, renameCurrentWorksheet);

            document.SetCellValue(1, 1, "ID");
            document.SetCellValue(1, 2, "Name");
            document.SetCellValue(1, 3, "Alarm text [en-US], Alarm text");
            document.SetCellValue(1, 4, "FieldInfo [Alarm text]");
            document.SetCellValue(1, 5, "Class");
            document.SetCellValue(1, 6, "Trigger tag");
            document.SetCellValue(1, 7, "Trigger bit");
            document.SetCellValue(1, 8, "Acknowledgement tag");
            document.SetCellValue(1, 9, "Acknowledgement bit");
            document.SetCellValue(1, 10, "Plc acknowledgement tag");
            document.SetCellValue(1, 11, "Plc acknowledgement bit");
            document.SetCellValue(1, 12, "Group");
            document.SetCellValue(1, 13, "Report");
            document.SetCellValue(1, 14, "Info text [en-US], Info text");
        }

        #endregion

        #endregion

    }
}
