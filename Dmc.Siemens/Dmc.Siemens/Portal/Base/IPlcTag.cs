using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC;

namespace Dmc.Siemens.Portal.Base
{
    public interface IPlcTag : ITag
    {

        string LogicalAddress { get; set; }

        bool IsHmiVisible { get; set; }

        bool IsHmiAccessible { get; set; }

    }
}
