using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMC.Siemens.Common.PLC.Base;

namespace DMC.Siemens.Portal.Base
{
    public interface IPlcTagTable : ITagTable
    {

        IEnumerable<IPlcTag> PlcTags { get; }

        IEnumerable<ConstantsEntry> Constants { get; }

    }
}
