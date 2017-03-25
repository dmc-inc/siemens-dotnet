using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;

namespace Dmc.Siemens.Common.PLC
{
    public class UserDataType : DataEntity
    {

		#region Public Properties

		public override BlockType Type => BlockType.UserDataType;

		public override bool IsPrimitiveDataType => false;

		public override string DataHeader { get; } = "STRUCT";

		#endregion

		#region Public Methods

		public override IParsableSource ParseSource(TextReader reader)
        {
            return base.ParseSource(reader);
        }

        #endregion
        
    }
}
