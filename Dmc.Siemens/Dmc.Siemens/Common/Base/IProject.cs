using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Common.PLC.Types;

namespace Dmc.Siemens.Common.Base
{
	public interface IProject : INotifyPropertyChanged
	{

		IEnumerable<IAutomationObject> AutomationObjects { get; }

		string Name { get; set; }

		T GetConstantValue<T>(Constant<T> constant) where T : struct;

		DataEntry GetUdtStructure(string udtName);

	}
}
