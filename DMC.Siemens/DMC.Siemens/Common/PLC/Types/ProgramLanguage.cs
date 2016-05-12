using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common.PLC
{
    public enum ProgramLanguage
    {

        UNDEF = 0,  // Undefined
        STL = 1,
        LAD = 2,
        FBD = 3,
        SCL = 4,
        DB = 5,
        GRAPH = 6,   
        VAT = 15,
        SFM1 = 29,
        F_STL = 31,
        F_LAD = 32,
        F_FBD = 39,
        F_DB = 34,
        F_CALL = 35,
        TECH = 37,
        SFM2 = 40,
        SRC = 10000,        
        SYM = 10001,  // Symbol table
        HMI = 10002,  // HMI
        HWC = 10003,  // HW Config

    }
}
