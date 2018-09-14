using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Logic.Base;

namespace Dmc.Siemens.Common.Plc.Logic.Instructions
{
	public class Move : InstructionBase
	{

		#region Constructors

		public Move()
		{

		}

		#endregion

		#region Public Properties

		private ReferenceBase _In;
		public ReferenceBase In
		{
			get
			{
				return this._In;
			}
			set
			{
				this.SetProperty(ref this._In, value);
			}
		}

		public ObservableCollection<ReferenceBase> Out { get; } = new ObservableCollection<ReferenceBase>();

		#endregion

		#region Protected Properties

		protected override string _Name => "Move";

		protected override Dictionary<string, ReferenceBase> _Interface
		{
			get
			{
				var dictionary = new Dictionary<string, ReferenceBase>();
				dictionary.Add("in", this.In);
				for (int i = 0; i < this.Out.Count; i++)
				{
					dictionary.Add($"out{i + 1}", this.Out[i]);
				}
				return dictionary;
			}
		}

		#endregion

	}
}
