using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Common.PLC
{
    public enum DataType
    {
        BOOL = 0x01,
        BYTE = 0x02,
        CHAR = 0x03,
        WORD = 0x04,  //KH
        INT = 0x05,   //KF
        DWORD = 0x06,
        DINT = 0x07,
        REAL = 0x08,
        DATE = 0x09,
        TIME_OF_DAY = 0x0A,
        TIME = 0x0b,
        S5TIME = 0x0c,
        DATE_AND_TIME = 0x0e,
        ARRAY = 0x10,
        STRUCT = 0x11,
        STRING = 0x13,

        POINTER = 0x14,
        ANY = 0x16,

        //The values below here are Only Legal for Parameters...

        BLOCK_FB = 0x17,
        BLOCK_FC = 0x18,
        BLOCK_DB = 0x19,
        BLOCK_SDB = 0x1a,
        COUNTER = 0x1c,
        TIMER = 0x1d,
        UNKNOWN = 0xff,

        //This are also Special Values
        UDT = 0x20,
        SFB = 0x21,
        FB = 0x22,

        //The following values are for S5 Datablocks
        S5_KH = 0xf02,
        S5_KF = 0xf05,
        S5_KM = 0xf01,
        S5_A = 0xf0b,
        S5_KG = 0xf08,
        S5_KT = 0xf06,
        S5_KZ = 0xf07,
        S5_KY = 0xf03,
        S5_KC = 0xf04,
        S5_C = 0xf0c,
    }
}
