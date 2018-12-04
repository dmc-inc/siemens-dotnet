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
using System.Reflection;
using System.Text.RegularExpressions;

namespace Dmc.Siemens.Common.Plc
{
    public abstract class DataEntity : DataObject, IParsableSource
    {

		#region Constructors

		public DataEntity(string name = "", DataType dataType = DataType.UNKNOWN, string comment = null, IEnumerable<DataEntry> children = null) : base(name, dataType, comment, children)
		{
			if (this.Children == null)
				this.Children = new LinkedList<DataEntry>();
		}

		#endregion

		#region Public Properties

		public override DataType DataType => DataType.STRUCT;
		
		public ProgramLanguage Language
		{
			get
			{
				return ProgramLanguage.STL;
			}
			set
			{
				throw new InvalidOperationException("Cannot change the BlockLanguage of a DataEntity");
			}
		}

		private int _number;
		public int Number
		{
			get
			{
				return this._number;
			}
			set
			{
				this.SetProperty(ref this._number, value);
			}
		}

		private ProjectFolder _parentFolder;
		public ProjectFolder ParentFolder
		{
			get
			{
				return this._parentFolder;
			}
			set
			{
				this.SetProperty(ref this._parentFolder, value);
			}
		}

		public abstract string DataHeader { get; }

		private string _version;
        [SourceMetadata("VERSION")]
		public string Version
		{
			get
			{
				return this._version;
			}
			set
			{
				this.SetProperty(ref this._version, value);
			}
		}
        
		#endregion

		#region Public Methods

		public virtual IParsableSource ParseSource(TextReader reader)
        {
            string line;
            string[] split;
            bool isInData = false;

            IEnumerable<(SourceMetadataAttribute attribute, PropertyInfo property)> metadata = this.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => (p.GetCustomAttribute<SourceMetadataAttribute>(true), p)).Where(x => !(x.Item1 is null));

            while ((line = reader.ReadLine()) != null)
            {
                if (!isInData)
                {
                    foreach (var (attribute, property) in metadata)
                    {
                        if (line.Contains(attribute.Keyword))
                        {
                            split = line.Split(attribute.Separator);
                            if (split.Length > 1)
                            {
                                try
                                {
                                    var value = split[1].Trim();
                                    if (!string.IsNullOrWhiteSpace(attribute.ValuePattern))
                                        value = Regex.Match(value, attribute.ValuePattern).Value;

                                    property.SetValue(this, Convert.ChangeType(value, property.PropertyType));
                                }
                                // Who cares if we can't set this property
                                catch {  }
                            }
                        }
                    }
                    if (line.Contains(this.DataHeader))
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
