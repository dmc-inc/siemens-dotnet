using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;

namespace Dmc.Siemens.Common.Plc
{
    public class UserDataType : DataEntity
    {

		#region Public Properties

		public override bool IsPrimitiveDataType => false;

		public override string DataHeader { get; } = "STRUCT";
		
		#endregion

		#region Public Methods

		public override IParsableSource ParseSource(TextReader reader)
        {
            return base.ParseSource(reader);
        }

		public override void Export(string path)
		{
			throw new NotImplementedException();
		}

		public override void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public override void WriteXml(XmlWriter writer)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
