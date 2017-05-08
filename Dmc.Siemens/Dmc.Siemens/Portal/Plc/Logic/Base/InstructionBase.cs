using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Portal.Plc.Logic.Base
{
	public abstract class InstructionBase : NotifyPropertyChanged, IXmlSerializable, IAutomationObject
	{

		#region Public Properties

		protected abstract string _Name { get; }
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				throw new InvalidOperationException("Cannot set the name of an instruction/");
			}
		}
		
		#endregion

		#region Public Methods

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
