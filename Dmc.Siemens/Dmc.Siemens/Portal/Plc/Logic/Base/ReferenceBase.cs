using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Types;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Portal.Plc.Logic.Base
{
	public abstract class ReferenceBase : AutomationObjectBase, IXmlSerializable, IAutomationObject
	{

		#region Public Properties
		
		private ReferenceScope _Scope;
		public ReferenceScope Scope
		{
			get
			{
				return this._Scope;
			}
			set
			{
				this.SetProperty(ref this._Scope, value);
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
		
		public LinkedList<DataEntry> Components { get; } = new LinkedList<DataEntry>();

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
