using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Common.PLC.Types;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Common.Base
{
	public abstract class ProjectBase : NotifyPropertyChanged, IProject
	{

		#region Public Properties

		private string _Name;
		public string Name
		{
			get
			{
				return this._Name;
			}
			set
			{
				this.SetProperty(ref this._Name, value);
			}
		}

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

		#region Public Methods

		public T GetConstantValue<T>(Constant<T> constant) where T : struct
		{
			throw new NotImplementedException();
		}

		public DataEntry GetUdtStructure(string udtName)
		{
			throw new NotImplementedException();
		}

		#endregion

	}
}
