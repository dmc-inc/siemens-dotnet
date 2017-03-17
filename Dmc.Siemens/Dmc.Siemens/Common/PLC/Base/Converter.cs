using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Common.PLC.Base
{
    public static class Converter
    {

        #region Public Properties

        public static Dictionary<DataType, Type> DataTypeConversionTable { get; } = new Dictionary<DataType, Type>()
        {
            { DataType.BOOL, typeof(bool) },
            { DataType.BYTE, typeof(byte) },
            { DataType.CHAR, typeof(char) },
            { DataType.DATE, typeof(DateTime) },
            { DataType.DATE_AND_TIME, typeof(DateTime) },
            { DataType.DINT, typeof(int) },
            { DataType.DWORD, typeof(uint) },
            { DataType.INT, typeof(short) },
            { DataType.REAL, typeof(float) },
            { DataType.STRING, typeof(string) },
            { DataType.TIME, typeof(int) },
            { DataType.TIME_OF_DAY, typeof(DateTime) },
            { DataType.WORD, typeof(ushort) }
        };

        #endregion

        #region Public Methods

        public static bool TryParse(string value, DataType dataType, out object parsedValue)
        {
            try
            {
                value = value.StartsWith("16#") ? value.Substring(3) : value;

                switch (dataType)
                {
                    case DataType.BOOL:
                        if (value == "1")
                        {
                            parsedValue = true;
                        }
                        else if (value == "0")
                        {
                            parsedValue = false;
                        }
                        else
                        {
                            parsedValue = bool.Parse(value);
                        }
                        break;
                    case DataType.BYTE:
                        parsedValue = byte.Parse(value, NumberStyles.HexNumber);
                        break;
                    case DataType.CHAR:
                        parsedValue = char.Parse(value.Trim('\''));
                        break;
                    case DataType.DINT:
                        parsedValue = int.Parse(value);
                        break;
                    case DataType.DWORD:
                        parsedValue = uint.Parse(value, NumberStyles.HexNumber);
                        break;
                    case DataType.INT:
                        parsedValue = short.Parse(value);
                        break;
                    case DataType.REAL:
                        parsedValue = float.Parse(value);
                        break;
                    case DataType.STRING:
                        parsedValue = value.Trim('"');
                        break;
                    case DataType.TIME:
                        parsedValue = int.Parse(value);
                        break;
                    case DataType.WORD:
                        parsedValue = ushort.Parse(value, NumberStyles.HexNumber);
                        break;
                    default:
                        parsedValue = null;
                        return false;
                }
                return true;
            }
            catch (Exception e) when (e is ArgumentNullException || e is FormatException || e is OverflowException)
            {
                parsedValue = null;
                return false;
            }
        }

        public static string ToS7Notation(object value, DataType dataType)
        {
            object parsedValue;
            if (!TryParse(value.ToString(), dataType, out parsedValue))
            {
                return value.ToString();
            }

            switch (dataType)
            {
                case DataType.BOOL:
                    return ((bool)value) ? bool.TrueString : bool.FalseString;
                case DataType.BYTE:
                    return $"16#{((byte)value).ToString("X")}";
                case DataType.DWORD:
                    return $"16#{((uint)value).ToString("X")}";
                case DataType.WORD:
                    return $"16#{((ushort)value).ToString("X")}";
                case DataType.CHAR:
                    return $"'{value.ToString()}'";
                case DataType.STRING:
                    return $"\"{value.ToString()}\"";
                default:
                    return value.ToString();
            }

        }

        #endregion

    }
}
