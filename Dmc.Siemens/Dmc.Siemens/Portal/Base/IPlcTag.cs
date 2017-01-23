using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMC.Siemens.Common.PLC;

namespace DMC.Siemens.Portal.Base
{
    public interface IPlcTag : ITag
    {

        string LogicalAddress { get; set; }

        bool IsHmiVisible { get; set; }

        bool IsHmiAccessible { get; set; }

    }
}
