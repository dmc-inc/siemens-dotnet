using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Common.Base
{
	public abstract class AutomationObjectBase : NotifyPropertyChanged, IAutomationObject
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

		#endregion

	}
}
