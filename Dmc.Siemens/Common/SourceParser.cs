﻿using Dmc.Siemens.Common.Plc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using System.Collections.ObjectModel;

namespace Dmc.Siemens.Common
{
    public static class SourceParser
    {

        #region Private Fields

        private static readonly IReadOnlyDictionary<string, Type> s_parsableTypes = new Dictionary<string, Type>
        {
            { "DATA_BLOCK", typeof(DataBlock)},
            { "TYPE", typeof(UserDataType)}
        };

        #endregion

        #region Public Methods

        public static List<IParsableSource> FromFile(string filePath)
        {
            TextReader reader = new StreamReader(filePath);
            return FromTextReader(reader);
        }

        public static List<IParsableSource> FromText(string text)
        {
            TextReader reader = new StringReader(text);
            return FromTextReader(reader);
        }

        public static List<IParsableSource> FromText(IEnumerable<string> text)
        {
            return FromText(string.Join(Environment.NewLine, text));
        }

        public static List<IParsableSource> FromTextReader(TextReader reader)
        {
            var sourceList = new List<IParsableSource>();

            var isEndEntry = false;
            string entryKey = null;
            string line;
            IParsableSource newSource = null;
            while ((line = reader.ReadLine()) != null)
            {
                if (!isEndEntry)
                {
                    var parsableEntry = s_parsableTypes.FirstOrDefault(e => line.Contains(e.Key));
                    if (parsableEntry.Key != null)
                    {
                        newSource = (IParsableSource)Activator.CreateInstance(parsableEntry.Value);

                        var split = line.Split(' ');
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
                        sourceList.Add(newSource);
                    }
                }

            }

            return sourceList;

        }

        #endregion

    }
}
