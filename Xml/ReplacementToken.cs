using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf;

namespace Dmc.Siemens.Xml
{
	public class ReplacementToken : NotifyPropertyChanged
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

		private string _Key;
		public string Key
		{
			get
			{
				return this._Key;
			}
			set
			{
				this.SetProperty(ref this._Key, value);
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

		#region Constructors

		public ReplacementToken(string key, string value = null, string name = null)
		{
			this.Key = key;
			this.Name = name;
			this.Value = value;
		}

		#endregion

		#region Methods
		
		public override int GetHashCode()
		{
			return this.Key.GetHashCode();
		}

		#endregion

	}
}
