using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.PLC.Types;
using Dmc.Siemens.Portal.Base;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Common.PLC
{
    public class DataEntry : DataObject
    {

		#region Constructors

		public DataEntry(string name = "", DataType dataType = DataType.UNKNOWN, string comment = null,
			IEnumerable<DataEntry> children = null, string dataTypeName = null, Constant<int> stringLength = default(Constant<int>),
			DataEntry arrayDataEntry = null, Constant<int> arrayStartIndex = default(Constant<int>), Constant<int> arrayEndIndex = default(Constant<int>))
			: base(name, dataType, comment, children)
		{
			this.DataTypeName = dataTypeName;
			this.StringLength = stringLength;
			this.ArrayDataEntry = arrayDataEntry;
			this.ArrayStartIndex = arrayStartIndex;
			this.ArrayEndIndex = arrayEndIndex;
		}

		#endregion

		#region Public Properties

		private DataEntry _ArrayDataEntry;
		public DataEntry ArrayDataEntry
		{
			get
			{
				return this._ArrayDataEntry;
			}
			set
			{
				this.SetProperty(ref this._ArrayDataEntry, value);
			}
		}
		
        public Constant<int> ArrayStartIndex { get; set; }
        public Constant<int> ArrayEndIndex { get; set; }
        public Constant<int> StringLength { get; set; }
		
		#endregion

		#region Public Methods

		public static DataEntry FromString(string dataEntry, TextReader dataReader)
        {

            DataEntry newEntry = new DataEntry();

            string trimmedData = dataEntry.Trim();
            string type = "";
            string[] splitString;
            int length = 0;
            int arrayStart = 0;
            bool isUdt = false;

            if (trimmedData.Contains("//"))
            {
                splitString = trimmedData.Split(new string[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                newEntry.Comment = splitString[1].Trim();
                trimmedData = splitString[0];
            }
            if (trimmedData.Contains(":"))
            {
                splitString = trimmedData.Split(':');
                type = splitString[1].Trim().Trim(';');
                newEntry.Name = splitString[0].Trim().Trim('"');
				
				if (type.ToUpper().Replace(" ", string.Empty).Contains("ARRAY["))
                {
                    splitString = type.Split(new string[] { "[", "..", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length >= 4)
                    {
                        if (int.TryParse(splitString[2], out length)) // See if the array end index is an integer
                        {
                            newEntry.ArrayEndIndex = length;
                        }
                        else // If not, the array index is a constant defined elsewhere
                        {
                            newEntry.ArrayEndIndex = new Constant<int>(splitString[2].Trim('\"'));
                        }
                        if (int.TryParse(splitString[1], out arrayStart)) // Do the same as above for the start index
                        {
                            newEntry.ArrayStartIndex = arrayStart;
                        }
                        else
                        {
                            newEntry.ArrayStartIndex = new Constant<int>(splitString[1].Trim('\"'));
                        }

                    }
                    splitString = type.Replace(" of ", " OF ").Split(new string[] { " OF " }, StringSplitOptions.RemoveEmptyEntries);

					string arrayType = string.Empty;
                    if (splitString.Length > 1)
                    {
						arrayType = splitString[1].Trim().Trim('"');
                    }
                    else if (splitString.Length == 1)  // Search the next line for the array type if it hasn't already been defined
                    {
                        string line;
                        line = dataReader.ReadLine();

                        arrayType = line.Trim().Trim(';').Trim('\"');
                    }

					var parsedType = TagHelper.ParseDataType(arrayType);
					if (parsedType == DataType.UNKNOWN)
					{
						newEntry.ArrayDataEntry = new DataEntry(dataType: DataType.UDT, dataTypeName: arrayType);
					}
					else if (parsedType == DataType.STRING)
					{
						newEntry.ArrayDataEntry = new DataEntry(dataType: DataType.STRING, stringLength: ParseStringLength(arrayType));
					}
					else
					{
						newEntry.ArrayDataEntry = new DataEntry(dataType: parsedType);
					}
					
					type = "ARRAY";

                }
				else if (type.Contains('"'))
				{
					int startUdt = type.IndexOf('"');
					int endUdt = type.LastIndexOf('"');
					if (startUdt >= 0 && endUdt >= 0)
					{
						newEntry.DataTypeName = type.Substring(startUdt + 1, endUdt - startUdt - 1);
						isUdt = true;
					}
				}
				else if (type.ToUpper().Contains("STRING"))
                {
					newEntry.StringLength = ParseStringLength(type);

                    type = "STRING";

                }
                else if (type.ToUpper().Contains("STRUCT"))
                {
                    string line;
                    while ((line = dataReader.ReadLine()) != null && !line.Contains("END_STRUCT"))
                    {
                        newEntry.Children.AddLast(DataEntry.FromString(line, dataReader));
                    }
                }

            }

            DataType t;
            if (isUdt && type != "ARRAY")
            {
                t = DataType.UDT;
            }
            else
            {
                if (!Enum.TryParse<DataType>(type, true, out t))
                {
					throw new SiemensException("Invalid type detected: " + type);
                }
            }

            newEntry.DataType = t;

            return newEntry;

			Constant<int> ParseStringLength(string typeString)
			{
				splitString = typeString.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
				if (splitString.Length > 1)
				{
					if (int.TryParse(splitString[1], out length)) // Check to make sure the string length is an integer
					{
						return length;
					}
					else // If not, it's a constant referenced elsewhere
					{
						return new Constant<int>(splitString[1].Trim('\"'));
					}
				}
				else
				{
					return 254;
				}
			}

        }

		public override Address CalculateSize(IPortalPlc plc)
		{
			// we only need to override size calculations on string and array
			// DataObject knows how to calculate the size of everything else
			switch (this.DataType)
			{
				case DataType.ARRAY:
					// check to make sure we have a valid ArrayDataEntry
					if (this.ArrayDataEntry == null || this.ArrayDataEntry.DataType == DataType.UNKNOWN)
						throw new SiemensException("Array '" + this.Name + "' does not have a valid ArrayDataType");

					// 2D arrays are not yet supported by Siemens
					if (this.ArrayDataEntry.DataType == DataType.ARRAY)
						throw new SiemensException("Siemens does not support 2D arrays. Array '" + this.Name + "' is invalid.");

					// check to make sure we are able to get the indices of the array
					if ((!this.ArrayStartIndex.HasValue || !this.ArrayEndIndex.HasValue) && plc == null)
						throw new ArgumentNullException(nameof(plc), "PLC cannot be null when the indices of an array are user defined constant values");

					// resolve the actual start and end indices
					int arrayStart = plc?.GetConstantValue(this.ArrayStartIndex) ?? this.ArrayStartIndex.Value;
					int arrayEnd = plc?.GetConstantValue(this.ArrayEndIndex) ?? this.ArrayEndIndex.Value;

					// calculate the array dimension and the size of each index
					int arraySize = arrayEnd - arrayStart + 1;
					Address arraySubTypeSize = this.ArrayDataEntry.CalculateSize(plc);
					// check for the case that this is an array of bools
					if (arraySubTypeSize.Byte == 0)
					{
						int overflow = arraySize % 16;
						if (overflow > 0)
						{
							return TagHelper.IncrementAddress(new Address(arraySize / 16, overflow));
						}
						else
						{
							return new Address(arraySize / 16);
						}
					}
					// check for the case that this is an array of bytes
					else if (arraySubTypeSize.Byte == 1)
					{
						if (arraySize % 2 > 0)
						{
							return new Address(arraySize / 2 + 1);
						}
						else
						{
							return new Address(arraySize / 2);
						}
					}
					// all other cases are types of size larger than a word
					else
					{
						return new Address(arraySize * arraySubTypeSize.Byte);
					}

				case DataType.STRING:
					// first to check to make sure we are able to get the size of the string
					if (!this.StringLength.HasValue && plc == null)
						throw new ArgumentNullException(nameof(plc), "PLC cannot be null if the string length is a user defined constant value");

					return new Address(TagHelper.GetPrimitiveByteSize(this.DataType, plc?.GetConstantValue(this.StringLength) ?? this.StringLength.Value));

				default:
					return base.CalculateSize(plc);
			}
		}

		#endregion

	}
}
