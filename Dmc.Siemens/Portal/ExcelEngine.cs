using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Portal.Base;
using Dmc.IO;
using Dmc.Siemens.Portal.Plc;
using Dmc.Siemens.Common;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Portal.Hmi;
using OfficeOpenXml;

namespace Dmc.Siemens.Portal
{
    public static class ExcelEngine
    {

   //     private const string PLC_TAG_WORKSHEET_NAME = "PLC Tags";
   //     private const string CONSTANT_WORKSHEET_NAME = "Constants";
   //     private const string ALARM_WORKSHEET_NAME = "DiscreteAlarms";
   //     private const string HMI_TAG_WORKSHEET_NAME = "Hmi Tags";

   //     public static IEnumerable<PlcTagTable> PlcTagTableFromFile(string filePath)
   //     {

   //         if (!File.Exists(filePath))
   //             throw new ArgumentException($"{filePath} does not point to a valid file");

   //         using (Stream fileStream = File.OpenRead(filePath))
   //         {
   //             using (var document = new SLDocument(fileStream))
   //             {
   //                 return ImportSpreadsheet(document);
   //             }
   //         }
                
   //     }

   //     public static void PlcTagTableToFile(string path, IEnumerable<PlcTagTable> tagTables)
   //     {
   //         if (!FileHelpers.IsValidFilePath(path, "xlsx"))
   //             throw new ArgumentException($"{nameof(path)} ({path}) is not a valid file path");
   //         if (tagTables?.Count() == 0)
   //             throw new ArgumentException($"{nameof(tagTables)} cannot be empty");

   //         ExportToFile(path, tagTables);
   //     }

   //     public static void PlcTagTableToFile(string path, PlcTagTable tagTable)
   //     {
   //         PlcTagTableToFile(path, new[] { tagTable });
   //     }
		
   //     #region Import Plc Tag Table

   //     private static IEnumerable<PlcTagTable> ImportSpreadsheet(ExcelWorkbook workbook)
   //     {
   //         var tagTables = new HashSet<PlcTagTable>();
   //         var tagTableLookup = new Dictionary<string, PlcTagTable>();

			//// Check for and navigate to Plc tag tab
			//if (!ExcelEngine.ChangeActiveWorksheet(workbook, ExcelEngine.PLC_TAG_WORKSHEET_NAME))
			//	throw new SiemensException("File does not contain " + ExcelEngine.PLC_TAG_WORKSHEET_NAME + " tab.");

			//// Parse all tags, starting at the row after the headers
			//var row = 1;
			//PlcTag tag;
			//string parentTableName;
			//PlcTagTable tagTable;
			//while (!string.IsNullOrWhiteSpace(workbook.GetCellValueAsString(row, 0)))
			//{
			//	// Get or create the parent tag table
			//	parentTableName = workbook.GetCellValueAsString(row, 1);
			//	if (tagTableLookup.ContainsKey(parentTableName))
			//		tagTable = tagTableLookup[parentTableName];
			//	else
			//	{
			//		tagTable = new PlcTagTable(parentTableName);
			//		tagTableLookup.Add(parentTableName, tagTable);
			//		tagTables.Add(tagTable);
			//	}

			//	// Create a new tag
			//	tag = new PlcTag(tagTable, workbook.GetCellValueAsString(row, 0), workbook.GetCellValueAsString(row, 3), workbook.GetCellValueAsString(row, 4),
			//		TagHelper.ParseDataType(workbook.GetCellValueAsString(row, 2)), Convert.ToBoolean(workbook.GetCellValueAsString(row, 5)), Convert.ToBoolean(workbook.GetCellValueAsString(row, 6)));

			//	// Add the tag to the tables collection of tags
			//	tagTable?.PlcTags?.Add(tag);
			//}

			//// Check for and navigate to Constants tab
			//if (!ExcelEngine.ChangeActiveWorksheet(workbook, ExcelEngine.CONSTANT_WORKSHEET_NAME))
			//	throw new SiemensException("File does not contain " + ExcelEngine.CONSTANT_WORKSHEET_NAME + " tab.");

			//// Parse all constants
			//row = 1;
			//ConstantsEntry constant;
			//while (!string.IsNullOrWhiteSpace(workbook.GetCellValueAsString(row, 0)))
			//{
			//	// Get or create the parent tag table
			//	parentTableName = workbook.GetCellValueAsString(row, 1);
			//	if (tagTableLookup.ContainsKey(parentTableName))
			//		tagTable = tagTableLookup[parentTableName];
			//	else
			//	{
			//		tagTable = new PlcTagTable(parentTableName);
			//		tagTableLookup.Add(parentTableName, tagTable);
			//		tagTables.Add(tagTable);
			//	}

			//	// Parse constant value
			//	var dataType = TagHelper.ParseDataType(workbook.GetCellValueAsString(row, 2));
			//	SiemensConverter.TryParse(workbook.GetCellValueAsString(row, 3), dataType, out var value);

			//	// Create a new constants entry
			//	constant = new ConstantsEntry(workbook.GetCellValueAsString(row, 0), value, dataType);

			//	tagTable?.Constants?.Add(constant);
			//}

			//return tagTables;
   //     }

   //     #endregion

   //     #region Export Tag Tables

   //     private static void ExportToFile(string path, IEnumerable<ITagTable> tagTables)
   //     {

   //         using (var document = new ExcelPackage())
   //         {
   //             foreach (var tagTable in tagTables)
   //             {
   //                 if (tagTable is PlcTagTable)
   //                     ExportPlcTagTableToFile(document.Workbook, tagTable as PlcTagTable);
   //                 else if (tagTable is HmiTagTable)
   //                     ExportHmiTagTableToFile(document.Workbook, tagTable as HmiTagTable);
   //             }

   //             using (var file = File.Open(path, FileMode.Create))
   //             {
   //                 document.SaveAs(file);
   //             }
   //         }
   //     }

   //     private static void ExportPlcTagTableToFile(ExcelWorkbook workbook, PlcTagTable tagTable)
   //     {
   //         if (tagTable?.PlcTags?.Count() > 0)
   //         {
   //             foreach (var tag in tagTable.PlcTags)
   //             {
   //                 //AddTagToDocument(document, tag);
   //             }
   //         }
   //         if (tagTable?.Constants?.Count() > 0)
   //         {
   //             foreach (var constant in tagTable.Constants)
   //             {
   //                 //AddConstantToDocument(document, constant);
   //             }
   //         }
   //     }

   //     private static void ExportHmiTagTableToFile(ExcelWorkbook workbook, HmiTagTable tagTable)
   //     {
   //         if (tagTable?.HmiTags?.Count() > 0)
   //         {
   //             foreach (var tag in tagTable.HmiTags)
   //             {
   //                 //AddTagToDocument(document, tag);
   //             }
   //         }
   //     }

   //     //private static void AddTagToDocument(SLDocument document, PlcTag tag)
   //     //{

   //     //}

   //     //private static void AddConstantToDocument(SLDocument document, ConstantsEntry constant)
   //     //{

   //     //}

   //     #endregion

   //     #region Setup Methods

   //     private static bool ChangeActiveWorksheet(ExcelWorkbook workbook, string worksheetName, bool renameCurrentWorksheet = false)
   //     {
   //         var name = workbook.GetCurrentWorksheetName();
   //         // If the current worksheet name is the one we want, dont do anything
   //         if (name != worksheetName)
   //         {
   //             // If rename is requested, rename and select
   //             if (renameCurrentWorksheet)
   //             {
   //                 workbook.RenameWorksheet(name, worksheetName);
   //                 return workbook.SelectWorksheet(worksheetName);
   //             }
   //             // If not rename, then check to see if the worksheet already exists in the document
   //             // If it doesn't exist, create it.  If it does, change to it
   //             else if (!workbook.GetWorksheetNames().Contains(worksheetName))
   //             {
   //                 return workbook.AddWorksheet(worksheetName);
   //             }
   //             else 
   //             {
   //                 return workbook.SelectWorksheet(worksheetName);
   //             }
   //         }
			//else
			//{
			//	return true;
			//}
   //     }

   //     private static void WritePlcTagHeaders(ExcelWorkbook workbook, bool renameCurrentWorksheet)
   //     {

   //         var worksheet = workbook.Worksheets[PLC_TAG_WORKSHEET_NAME]
   //         ChangeActiveWorksheet(workbook, PLC_TAG_WORKSHEET_NAME, renameCurrentWorksheet);

   //         // Write headers
   //         workbook.SetCellValue(1, 1, "Name");
   //         workbook.SetCellValue(1, 2, "Path");
   //         workbook.SetCellValue(1, 3, "Connection");
   //         workbook.SetCellValue(1, 4, "Plc tag");
   //         workbook.SetCellValue(1, 5, "DataType");
   //         workbook.SetCellValue(1, 6, "Length");
   //         workbook.SetCellValue(1, 7, "Coding");
   //         workbook.SetCellValue(1, 8, "Access Method");
   //         workbook.SetCellValue(1, 9, "Address");
   //         workbook.SetCellValue(1, 10, "Indirect addressing");
   //         workbook.SetCellValue(1, 11, "Index tag");
   //         workbook.SetCellValue(1, 12, "Start value");
   //         workbook.SetCellValue(1, 13, "ID tag");
   //         workbook.SetCellValue(1, 14, "Display name [en-US]");
   //         workbook.SetCellValue(1, 15, "Comment [en-US]");
   //         workbook.SetCellValue(1, 16, "Acquisition mode");
   //         workbook.SetCellValue(1, 17, "Acquisition cycle");
   //         workbook.SetCellValue(1, 18, "Range Maximum Type");
   //         workbook.SetCellValue(1, 19, "Range Maximum");
   //         workbook.SetCellValue(1, 20, "Range Minimum Type");
   //         workbook.SetCellValue(1, 21, "Range Minimum");
   //         workbook.SetCellValue(1, 22, "Linear scaling");
   //         workbook.SetCellValue(1, 23, "End value Plc");
   //         workbook.SetCellValue(1, 24, "Start value Plc");
   //         workbook.SetCellValue(1, 25, "End value HMI");
   //         workbook.SetCellValue(1, 26, "Start value HMI");
   //         workbook.SetCellValue(1, 27, "Gmp relevant");
   //         workbook.SetCellValue(1, 28, "Confirmation Type");
   //         workbook.SetCellValue(1, 29, "Mandatory Commenting");
            
   //     }

   //     private static void WriteAlarmTagHeaders(SLDocument document, bool renameCurrentWorksheet)
   //     {
   //         ChangeActiveWorksheet(document, ALARM_WORKSHEET_NAME, renameCurrentWorksheet);

   //         document.SetCellValue(1, 1, "ID");
   //         document.SetCellValue(1, 2, "Name");
   //         document.SetCellValue(1, 3, "Alarm text [en-US], Alarm text");
   //         document.SetCellValue(1, 4, "FieldInfo [Alarm text]");
   //         document.SetCellValue(1, 5, "Class");
   //         document.SetCellValue(1, 6, "Trigger tag");
   //         document.SetCellValue(1, 7, "Trigger bit");
   //         document.SetCellValue(1, 8, "Acknowledgement tag");
   //         document.SetCellValue(1, 9, "Acknowledgement bit");
   //         document.SetCellValue(1, 10, "Plc acknowledgement tag");
   //         document.SetCellValue(1, 11, "Plc acknowledgement bit");
   //         document.SetCellValue(1, 12, "Group");
   //         document.SetCellValue(1, 13, "Report");
   //         document.SetCellValue(1, 14, "Info text [en-US], Info text");
   //     }

   //     #endregion

    }
}
