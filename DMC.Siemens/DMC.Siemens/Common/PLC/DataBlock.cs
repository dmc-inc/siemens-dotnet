using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.PLC
{
    public class DataBlock : Block
    {

        protected override string DataHeader
        {
            get
            {
                return (this.IsOptimized) ? "VAR" : "STRUCT";
            }
        }

        public LinkedList<DataEntry> Data { get; set; }
        public bool IsOptimized { get; set; }
        public bool IsInstance { get; set; }
        public bool IsDataType { get; set; }
        public string InstanceNumber { get; set; }

        public override IParsableSource ParseSource(TextReader reader)
        {
            string line;
            string[] split;
            bool isInData = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!isInData)
                {
                    if (line.Contains("S7_Optimized_Access"))
                    {
                        this.IsOptimized = line.ToUpper().Contains("TRUE");
                    }
                    else if (line.Contains("VERSION"))
                    {
                        split = line.Split(':');
                        if (split.Length > 1)
                        {
                            this.Version = split[1].Trim();
                        }
                    }
                    else if (line.Contains(this.DataHeader))
                    {
                        isInData = true;
                    }
                }
                else
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        if (line.Contains("END_" + this.DataHeader))
                        {
                            isInData = false;
                            break;
                        }
                        else
                        {
                            this.Data.AddLast(DataEntry.FromString(line));
                        }
                    }
                }

            }

            return this;

        }

    }
}
