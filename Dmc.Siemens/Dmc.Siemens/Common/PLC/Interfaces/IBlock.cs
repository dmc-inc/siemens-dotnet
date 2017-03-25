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

namespace Dmc.Siemens.Common.PLC.Interfaces
{
    public interface IBlock : INotifyPropertyChanged, IParsableSource
    {

        #region Public Properties
		
        BlockType Type { get; }
		
        BlockLanguage Language { get; set; }
		
        int Number { get; set; }
		
		string Comment { get; set; }
		
        ProjectFolder ParentFolder { get; set; }

		string DataHeader { get; }

		#endregion
        
    }
}
