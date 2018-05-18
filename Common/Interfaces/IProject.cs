using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Types;

namespace Dmc.Siemens.Common.Interfaces
{
	public interface IProject : INotifyPropertyChanged
	{

		IEnumerable<IAutomationObject> AutomationObjects { get; }

		string Name { get; set; }
		
	}
}
