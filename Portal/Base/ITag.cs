using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc;

namespace Dmc.Siemens.Portal.Base
{
    public interface ITag
    {

        string Name { get; set; }

        DataType DataType { get; set; }

        string Comment { get; set; }

        ITagTable TagTable { get; set; }

    }
}
