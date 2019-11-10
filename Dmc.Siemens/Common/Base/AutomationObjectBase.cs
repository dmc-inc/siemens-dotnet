using Dmc.Siemens.Common.Interfaces;
using Dmc.Wpf;

namespace Dmc.Siemens.Common.Base
{
	public abstract class AutomationObjectBase : NotifyPropertyChanged, IAutomationObject
	{

		private string _name;
		public string Name
        {
            get => this._name;
            set => this.SetProperty(ref this._name, value);
        }

    }
}
