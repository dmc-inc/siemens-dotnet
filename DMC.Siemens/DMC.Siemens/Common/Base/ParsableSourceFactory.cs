using DMC.Siemens.Common.PLC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.Base
{
    public static class ParsableSourceFactory
    {

        public static List<IParsableSource> ParseFromFile(string filePath)
        {
            TextReader reader = new StreamReader(filePath);
            return ParseFromTextReader(reader);
        }

        public static List<IParsableSource> ParseFromText(string text)
        {
            TextReader reader = new StringReader(text);
            return ParseFromTextReader(reader);
        }

        public static List<IParsableSource> ParseFromText(string[] text)
        {
            return ParseFromText(String.Join(Environment.NewLine, text));
        }

        public static List<IParsableSource> ParseFromText(IEnumerable<string> text)
        {
            return ParseFromText(String.Join(Environment.NewLine, text));
        }

        public static List<IParsableSource> ParseFromTextReader(TextReader reader)
        {
            List<IParsableSource> blockList = new List<IParsableSource>();

            bool isEndEntry = false;
            string entryKey = null;
            string line;
            IParsableSource newSource = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (!isEndEntry)
                {
                    KeyValuePair<string, Type> parsableEntry = ParsableTypes.FirstOrDefault(e => line.Contains(e.Key));
                    if (parsableEntry.Key != null)
                    {
                        newSource = (IParsableSource)Activator.CreateInstance(parsableEntry.Value);

                        string[] split = line.Split(' ');
                        if (split.Length > 1)
                        {
                            newSource.Name = split[1].Trim('\"');
                        }

                        newSource.ParseSource(reader);
                        entryKey = parsableEntry.Key;
                        isEndEntry = true;

                    }
                }
                else
                {
                    if (line.Contains("END_" + entryKey))
                    {
                        isEndEntry = false;
                        blockList.Add(newSource);
                    }
                }

            }

            return blockList;

        }

        private static readonly Dictionary<string, Type> ParsableTypes = new Dictionary<string, Type>()
        {
            { "DATA_BLOCK", typeof(DataBlock)},
            { "TYPE", typeof(UserDataType)}
        };

    }
}
