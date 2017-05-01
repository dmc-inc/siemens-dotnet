using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Wpf.Base;

namespace Dmc.Siemens.Common.Plc.Logic
{
	public class Network : NotifyPropertyChanged
	{

		#region Constructors

		public Network()
		{

		}

		#endregion

		#region Public Properties

		private string _Title;
		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				this.SetProperty(ref this._Title, value);
			}
		}

		private string _Comment;
		public string Comment
		{
			get
			{
				return this._Comment;
			}
			set
			{
				this.SetProperty(ref this._Comment, value);
			}
		}

		private ProgramLanguage _Language;
		public ProgramLanguage Language
		{
			get
			{
				return this._Language;
			}
			set
			{
				this.SetProperty(ref this._Language, value);
			}
		}

		#endregion

	}
}
