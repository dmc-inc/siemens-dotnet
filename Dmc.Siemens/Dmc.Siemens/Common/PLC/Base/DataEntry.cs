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

		private DataType? _ArrayDataType;
		public DataType? ArrayDataType
		{
			get
			{
				return this._ArrayDataType;
			}
			set
			{
				this.SetProperty(ref this._ArrayDataType, value);
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
                newEntry.Name = splitString[0].Trim().Trim('"');

				if (type.Contains('"'))
				{
					int startUdt = type.IndexOf('"');
					int endUdt = type.LastIndexOf('"');
					if (startUdt >= 0 && endUdt >= 0)
					{
						newEntry.DataTypeName = type.Substring(startUdt + 1, endUdt - startUdt - 1);
						isUdt = true;
					}
				}
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
                    splitString = type.ToUpper().Split(new string[] { " OF " }, StringSplitOptions.RemoveEmptyEntries);

					string arrayType = string.Empty;
                    if (splitString.Length > 1 && !isUdt)
                    {
						arrayType = splitString[1].Trim();
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
						newEntry.ArrayDataType = DataType.UDT;
						newEntry.DataTypeName = arrayType;
					}
					else if (parsedType == DataType.STRING)
					{
						newEntry.ArrayDataType = parsedType;
						newEntry.StringLength = ParseStringLength(arrayType);
					}
					else
					{
						newEntry.ArrayDataType = parsedType;
					}
					
					type = "ARRAY";

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
                    // Invalid type detected
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

		public IDictionary<DataEntry, Address> CalcluateAddresses(IPlc plc)
		{
			return TagHelper.CalcluateAddresses(this, plc);
		}

		public Address CalculateSize(IPlc plc)
		{
			return TagHelper.CalculateSize(this, plc);
		}

		public void SetUdtStructure(IEnumerable<DataEntry> children)
		{
			if (this.DataType == DataType.UDT)
			{
				this.Children = new LinkedList<DataEntry>(children);
				this.DataType = DataType.STRUCT;
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
