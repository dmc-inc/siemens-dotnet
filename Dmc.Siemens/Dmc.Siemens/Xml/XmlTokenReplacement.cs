using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Xml
{
	public class XmlTokenReplacement : NotifyPropertyChanged
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

		private string _Value;
		public string Value
		{
			get
			{
				return this._Value;
			}
			set
			{
				this.SetProperty(ref this._Value, value);
			}
		}

		#endregion

	}
}
