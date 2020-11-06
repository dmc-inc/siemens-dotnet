using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Logic.Base;

namespace Dmc.Siemens.Common.Plc.Logic.Instructions
{
	public class Or : InstructionBase
	{

        #region Public Properties

        public ObservableCollection<ReferenceBase> In { get; } = new ObservableCollection<ReferenceBase>();

        #endregion

        #region Protected Properties

        protected override string _Name => "O";

		protected override Dictionary<string, ReferenceBase> _Interface
		{
			get
			{
				var dictionary = new Dictionary<string, ReferenceBase>();
				for (var i = 0; i < this.In.Count; i++)
				{
					dictionary.Add($"in{i + 1}", this.In[i]);
				}
				return dictionary;
			}
		}

		#endregion

	}
}
