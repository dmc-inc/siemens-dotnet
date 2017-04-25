using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.Plc
{
    public abstract class DataEntity : DataObject, IBlock
    {

		#region Public Properties

		public override DataType DataType => DataType.STRUCT;
		
		public BlockLanguage Language
		{
			get
			{
				return BlockLanguage.STL;
			}
			set
			{
				throw new InvalidOperationException("Cannot change the BlockLanguage of a DataEntity");
			}
		}

		public abstract BlockType Type { get; }

		private int _Number;
		public int Number
		{
			get
			{
				return this._Number;
			}
			set
			{
				this.SetProperty(ref this._Number, value);
			}
		}

		private ProjectFolder _ParentFolder;
		public ProjectFolder ParentFolder
		{
			get
			{
				return this._ParentFolder;
			}
			set
			{
				this.SetProperty(ref this._ParentFolder, value);
			}
		}

		public abstract string DataHeader { get; }

		private string _Version;
		public string Version
		{
			get
			{
				return this._Version;
			}
			set
			{
				this.SetProperty(ref this._Version, value);
			}
		}

		#endregion

		#region Public Methods

		public virtual IParsableSource ParseSource(TextReader reader)
        {
            string line;
            string[] split;
            bool isInData = false;

            while ((line = reader.ReadLine()) != null)
            {
                if (!isInData)
                {
                    if (line.Contains("VERSION"))
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
                            this.Children.AddLast(DataEntry.FromString(line, reader));
                        }
                    }
                }

            }

            return this;

        }

		public abstract void Export(string path);

		#region IXmlSerializable

		public XmlSchema GetSchema()
		{
			return null;
		}

		public abstract void ReadXml(XmlReader reader);

		public abstract void WriteXml(XmlWriter writer);

		#endregion

		#endregion

	}
}
