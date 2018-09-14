using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Wpf;

namespace Dmc.Siemens.Common.Plc.Base
{
	public abstract class BlockBase : NotifyPropertyChanged, IBlock
	{

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

		public abstract BlockType Type { get; }

		private ProgramLanguage _Language;
		public virtual ProgramLanguage Language
		{
			get
			{
				return this._Language;
			}
			set
			{
				this.SetProperty(ref this._Language, value);
			}
		}

		private int _Number;
		public virtual int Number
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

		public abstract void Export(string path);

		public XmlSchema GetSchema()
		{
			return null;
		}

		public abstract IParsableSource ParseSource(TextReader reader);

		public abstract void ReadXml(XmlReader reader);

		public abstract void WriteXml(XmlWriter writer);

		public virtual T Clone<T>()
			where T : BlockBase
		{
			using (var stream = new MemoryStream())
			{
				var formatter = new BinaryFormatter() { Context = new StreamingContext(StreamingContextStates.Clone) };
				formatter.Serialize(stream, this);
				stream.Position = 0;

				return (formatter.Deserialize(stream) as T);
			}
		}

		#endregion

	}
}
