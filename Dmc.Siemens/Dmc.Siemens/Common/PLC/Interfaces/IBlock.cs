using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Dmc.Siemens.Common.Plc.Interfaces
{
    public interface IBlock : INotifyPropertyChanged, IParsableSource, IExportable, IXmlSerializable
    {

        #region Public Properties
		
        BlockType Type { get; }

		ProgramLanguage Language { get; set; }
		
        int Number { get; set; }
		
		string Comment { get; set; }

		string DataHeader { get; }

		#endregion
        
    }
}
