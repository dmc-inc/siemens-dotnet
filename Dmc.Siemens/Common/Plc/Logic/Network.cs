using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc.Logic.Base;
using Dmc.Wpf;

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
            get => this._Title;
            set => this.SetProperty(ref this._Title, value);
        }

        private string _Comment;
		public string Comment
        {
            get => this._Comment;
            set => this.SetProperty(ref this._Comment, value);
        }

        private ProgramLanguage _Language;
		public ProgramLanguage Language
        {
            get => this._Language;
            set => this.SetProperty(ref this._Language, value);
        }
        public ObservableCollection<InstructionBase> Instructions { get; } = new ObservableCollection<InstructionBase>();

        #endregion

    }
}
