using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Portal.Project
{
	public class PortalProject : NotifyPropertyChanged, IProject
	{
		public IEnumerable<IAutomationObject> AutomationObjects => throw new NotImplementedException();

		public string Name { get; set; }

	}
}
