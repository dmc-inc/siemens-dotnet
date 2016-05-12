using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.PLC
{
    public class Symbol
    {
        public string Name { get; set; }
        public string Comment { get; set; }
        public string Operand { get; set; }
        public string OperandIEC { get; set; }
        public string DataType { get; set; }

        public override string ToString()
        {
            return Name + " (" + Operand + ")";
        }
    }
}
