using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.PLC
{
    public class DataEntry
    {

        public string Name { get; set; }
        public DataType DataType { get; set; }
        public string DataTypeName { get; set; }
        public int? ArrayStartIndex { get; set; }
        public int? ArrayEndIndex { get; set; }
        public int? StringLength { get; set; }
        public string Comment { get; set; }
        public LinkedList<DataEntry> Children { get; set; } = new LinkedList<DataEntry>();

        public static DataEntry FromString(string dataEntry)
        {

            DataEntry newEntry = new DataEntry();

            String trimmedData = dataEntry.Trim();
            String type = "";
            String[] splitString;
            int length = 0;
            int arrayStart = 0;

            if (trimmedData.Contains("//"))
            {
                splitString = trimmedData.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                newEntry.Comment = splitString[1].Trim();
                trimmedData = splitString[0];
            }
            if (trimmedData.Contains(":"))
            {
                splitString = trimmedData.Split(':');
                type = splitString[1].Trim().Trim(';').Trim('\"').Trim();
                newEntry.Name = splitString[0].Trim();

                if (type.ToUpper().Contains("ARRAY"))
                {
                    newEntry.Name = newEntry.Name.Trim('\"');
                    splitString = type.Split(new string[] { "[", "..", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length == 4)
                    {
                        bool parsed = Int32.TryParse(splitString[2], out length);
                        bool parsed2 = Int32.TryParse(splitString[1], out arrayStart);

                        newEntry.ArrayStartIndex = arrayStart;
                        newEntry.ArrayEndIndex = length;
                    }
                    splitString = type.Split(new string[] { "of" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length > 1)
                    {
                        newEntry.DataTypeName = splitString[1].Trim().Trim('\"');
                    }

                    type = "ARRAY";

                }
                else if (type.ToUpper().Contains("STRING"))
                {
                    splitString = type.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length > 1)
                    {
                        bool parsed = Int32.TryParse(splitString[1], out length);
                    }
                    else
                    {
                        length = 254;
                        newEntry.Name = newEntry.Name.Trim('\"');
                    }

                    type = "STRING";
                    newEntry.StringLength = length;

                }

            }

            DataType t;
            bool validType = Enum.TryParse<DataType>(type, true, out t);
            if (!validType) { t = DataType.UDT; newEntry.DataTypeName = type; }

            newEntry.DataType = t;

            return newEntry;

        }
    }
}
