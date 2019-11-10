using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dmc.Siemens.Common.Plc.Logic.Base;

namespace Dmc.Siemens.Common.Plc.Logic.Instructions
{
	public class Move : InstructionBase
	{

		private ReferenceBase _in;
		public ReferenceBase In
        {
            get => this._in;
            set => this.SetProperty(ref this._in, value);
        }

        public ObservableCollection<ReferenceBase> Out { get; } = new ObservableCollection<ReferenceBase>();

		protected override string _Name => "Move";

		protected override Dictionary<string, ReferenceBase> _Interface
		{
			get
			{
                var dictionary = new Dictionary<string, ReferenceBase>
                {
                    { "in", this.In }
                };
                for (var i = 0; i < this.Out.Count; i++)
				{
					dictionary.Add($"out{i + 1}", this.Out[i]);
				}
				return dictionary;
			}
		}

	}
}
