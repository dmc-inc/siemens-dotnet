using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc.Logic.Base;

namespace Dmc.Siemens.Common.Plc.Logic.Instructions
{
	public sealed class Contact : InstructionBase
	{

		#region Public Properties

		private ReferenceBase _Operand;
		public ReferenceBase Operand
        {
            get => this._Operand;
            set => this.SetProperty(ref this._Operand, value);
        }

        #endregion

        #region Protected Properties

        protected override string _Name => "Contact";

		protected override Dictionary<string, ReferenceBase> _Interface
		{
			get
			{
				return new Dictionary<string, ReferenceBase>()
				{
					{"operand", this.Operand}
				};
			}
		}

		#endregion

	}
}
