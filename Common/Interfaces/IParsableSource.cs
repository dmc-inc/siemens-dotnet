using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Common.Interfaces
{
    public interface IParsableSource : IAutomationObject
    {
		
        string Version { get; set; }

        IParsableSource ParseSource(TextReader reader);

    }
}
