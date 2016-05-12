using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMC.Siemens.Common
{
    public interface IParsableSource
    {

        string Name { get; set; }
        string Version { get; set; }

        IParsableSource ParseSource(TextReader reader);

    }
}
