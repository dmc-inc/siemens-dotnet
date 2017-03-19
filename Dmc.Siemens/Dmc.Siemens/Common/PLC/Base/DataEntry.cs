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
    public class DataEntry : NotifyPropertyChanged, IDataEntry
    {

		#region Constructors

		public DataEntry(string name = "", DataType dataType = DataType.UNKNOWN, string comment = null,
			IEnumerable<DataEntry> children = null, string dataTypeName = null, Constant<int> stringLength = default(Constant<int>),
			Constant<int> arrayStartIndex = default(Constant<int>), Constant<int> arrayEndIndex = default(Constant<int>))
		{
			this.Name = name;
			this.DataType = dataType;
			this.Comment = comment;
			this.Children = (children != null) ? new LinkedList<DataEntry>(children) : new LinkedList<DataEntry>();
			this.DataTypeName = dataTypeName;
			this.StringLength = stringLength;
			this.ArrayStartIndex = arrayStartIndex;
			this.ArrayEndIndex = arrayEndIndex;
		}

		#endregion

		#region Public Properties

		private string _Name;
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this.SetProperty(ref this._Name, value);
            }
        }

        private DataType _DataType;
        public DataType DataType
        {
            get
            {
                return this._DataType;
            }
            set
            {
                this.SetProperty(ref this._DataType, value);
            }
        }

        private string _DataTypeName;
        public string DataTypeName
        {
            get
            {
                return this._DataTypeName;
            }
            set
            {
                this.SetProperty(ref this._DataTypeName, value);
            }
        }

        public Constant<int> ArrayStartIndex { get; set; }
        public Constant<int> ArrayEndIndex { get; set; }
        public Constant<int> StringLength { get; set; }

        private string _Comment;
        public string Comment
        {
            get
            {
                return this._Comment;
            }
            set
            {
                this.SetProperty(ref this._Comment, value);
            }
        }

        public LinkedList<DataEntry> Children { get; set; }
		
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
                newEntry.Name = splitString[0].Trim();

                // Check to see if there is a UDT first
                if (type.Contains("\""))
                {
                    Match match = Regex.Match(type, "\"([^\"]*)\"");
                    // Use Regex to grab the value between the quotes
                    if (match.Success && match.Groups != null && match.Groups.Count > 1)
                    {
                        newEntry.DataTypeName = match.Groups[1].Value;
                        isUdt = true;
                    }

                }
                if (type.ToUpper().Replace(" ", string.Empty).Contains("ARRAY["))
                {
                    newEntry.Name = newEntry.Name.Trim('\"');
                    splitString = type.Split(new string[] { "[", "..", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length == 4)
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
                    splitString = type.ToUpper().Split(new string[] { " OF " }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length > 1 && !isUdt)
                    {
                        newEntry.DataTypeName = splitString[1].Trim();
                    }
                    else if (splitString.Length == 1)  // Search the next line for the array type if it hasn't already been defined
                    {
                        string line;
                        line = dataReader.ReadLine();

                        newEntry.DataTypeName = line.Trim().Trim(';').Trim('\"');
                    }
                    

                    type = "ARRAY";

                }
                else if (type.ToUpper().Contains("STRING"))
                {
                    splitString = type.Split(new string[] { "[", "]" }, StringSplitOptions.RemoveEmptyEntries);
                    if (splitString.Length > 1)
                    {
                        if (int.TryParse(splitString[1], out length)) // Check to make sure the string length is an integer
                        {
                            newEntry.StringLength = length;
                        }
                        else // If not, it's a constant referenced elsewhere
                        {
                            newEntry.StringLength = new Constant<int>(splitString[1].Trim('\"'));
                        }

                    }
                    else
                    {
                        newEntry.StringLength = 254;
                        newEntry.Name = newEntry.Name.Trim('\"');
                    }

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
                    // Invalid type detected
                }
            }

            newEntry.DataType = t;

            return newEntry;

        }

		public IDictionary<DataEntry, Address> CalcluateAddresses(IPlc plc)
		{
			return TagHelper.CalcluateAddresses(this, plc);
		}

		public int CalculateByteSize(IPlc plc)
		{
			// Check if it is a primitive first
			int size;
			if ((size = TagHelper.GetPrimitiveByteSize(this.DataType, (this.StringLength.HasValue) ? this.StringLength.Value : 0)) >= 0)
				return size;

			// Handle the cases where it is not
			switch (this.DataType)
			{
				case DataType.ARRAY:
					int arraySize = plc.GetConstantValue(this.ArrayEndIndex) - plc.GetConstantValue(this.ArrayStartIndex) + 1;
					DataType type;
					IDataEntry newEntry;
					if (Enum.TryParse(this.DataTypeName, out type))
					{
						newEntry = new DataEntry() { DataType = type };
					}
					else
					{
						newEntry = plc.GetUdtStructure(this.DataTypeName);
					}
					size = newEntry.CalculateByteSize(plc);
					if (size == 0)
					{
						int overflow = arraySize % 16;
						if (overflow > 0)
						{
							return ((arraySize / 16) * 2 + 2);
						}
						else
						{
							return (arraySize / 16);
						}
					}
					else if (size == 1)
					{
						if (arraySize % 2 > 0)
						{
							return (arraySize / 2 + 1);
						}
						else
						{
							return (arraySize / 2);
						}
					}
					else
					{
						return arraySize * size;
					}

				case DataType.STRUCT:
				case DataType.UDT:
					return plc.GetUdtStructure(this.DataTypeName).CalculateByteSize(plc);
				default:
					throw new SiemensException($"Invalid DataType: {this.DataType.ToString()}");
			}
		}

		IEnumerator<DataEntry> IEnumerable<DataEntry>.GetEnumerator()
		{
			return this.Children.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.Children.GetEnumerator();
		}

		#endregion

	}
}
