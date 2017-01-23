using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMC.Siemens.Common.Base;

namespace DMC.Siemens.Common.PLC
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
