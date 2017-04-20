using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC.Base;

namespace Dmc.Siemens.Portal.Base
{
    public interface IPlcTagTable : ITagTable
    {

        ICollection<IPlcTag> PlcTags { get; }

        ICollection<ConstantsEntry> Constants { get; }

    }
}
