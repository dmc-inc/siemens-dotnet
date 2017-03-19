using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Common.PLC.Types;

namespace Dmc.Siemens.Common.Interfaces
{
	public interface IProject : INotifyPropertyChanged
	{

		IEnumerable<IAutomationObject> AutomationObjects { get; }

		string Name { get; set; }
		
	}
}
