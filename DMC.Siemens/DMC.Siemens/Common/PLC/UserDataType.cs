using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.PLC
{
    public class UserDataType : DataEntity
    {

        protected override string DataHeader { get; } = "STRUCT";

        public override IParsableSource ParseSource(TextReader reader)
        {
            this.Type = BlockType.UDT;
            return base.ParseSource(reader);
        }

    }
}
