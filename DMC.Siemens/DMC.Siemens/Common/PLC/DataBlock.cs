using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMC.Siemens.Common.Base;

namespace DMC.Siemens.Common.PLC
{
    public class DataBlock : DataEntity
    {

        protected override string DataHeader
        {
            get
            {
                return (this.IsOptimized) ? "VAR" : "STRUCT";
            }
        }

        public bool IsOptimized { get; set; }
        public bool IsInstance { get; set; }
        public bool IsDataType { get; set; }
        public string InstanceNumber { get; set; }

        public override IParsableSource ParseSource(TextReader reader)
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("S7_Optimized_Access"))
                {
                    this.IsOptimized = line.ToUpper().Contains("TRUE");
                    base.ParseSource(reader);
                    break;
                }
                else if (line.Contains("TITLE ="))
                {
                    this.IsOptimized = false;
                    base.ParseSource(reader);
                    break;
                }
            }

            this.Type = BlockType.DB;

            return this;

        }

    }
}
