using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Wpf;

namespace Dmc.Siemens.Common.Plc.Logic.Base
{
	public abstract class InstructionBase : NotifyPropertyChanged, IXmlSerializable, IAutomationObject
	{

		#region Public Properties

		protected abstract string _Name { get; }
		public string Name
        {
            get => this._Name;
            set => throw new InvalidOperationException("Cannot set the name of an instruction/");
        }

        protected abstract Dictionary<string, ReferenceBase> _Interface { get; }
        public Dictionary<string, ReferenceBase> Interface => this._Interface;

        #endregion

        #region Public Methods

        public XmlSchema GetSchema()
		{
			return null;
		}

		public virtual void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public virtual void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
