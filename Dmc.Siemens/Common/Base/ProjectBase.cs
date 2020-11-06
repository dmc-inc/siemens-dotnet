using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Types;
using Dmc.Wpf;

namespace Dmc.Siemens.Common.Base
{
	public abstract class ProjectBase : AutomationObjectBase, IProject
	{

		private IEnumerable<IAutomationObject> _automationObjects;
		public IEnumerable<IAutomationObject> AutomationObjects
        {
            get => this._automationObjects;
            set => this.SetProperty(ref this._automationObjects, value);
        }

        private IEnumerable<IPlc> Plcs => this.AutomationObjects?.OfType<IPlc>();

    }
}
