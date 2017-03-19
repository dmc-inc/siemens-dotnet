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

		#region Protected Properties

		protected override string DataHeader { get; } = "STRUCT";

        #endregion

        #region Public Methods

        public override IParsableSource ParseSource(TextReader reader)
        {
            this.Type = BlockType.UserDataType;
            return base.ParseSource(reader);
        }

        #endregion
        
    }
}
