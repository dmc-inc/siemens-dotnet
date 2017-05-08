using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Types;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Common.Base
{
	public abstract class ProjectBase : AutomationObjectBase, IProject
	{

		#region Public Properties
		
		private IEnumerable<IAutomationObject> _AutomationObjects;
		public IEnumerable<IAutomationObject> AutomationObjects
		{
			get
			{
				return this._AutomationObjects;
			}
			set
			{
				this.SetProperty(ref this._AutomationObjects, value);
			}
		}

		#endregion

		#region Private Properties

		private IEnumerable<IPlc> Plcs
		{
			get
			{
				return this.AutomationObjects?.OfType<IPlc>();
			}
		}

		#endregion

		#region Public Methods

		

		#endregion

	}
}
