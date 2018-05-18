using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Interfaces;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Base;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Common.Plc.Types;
using Dmc.Wpf.Base;
using Dmc.Wpf.Collections;
using System.Collections.ObjectModel;

namespace Dmc.Siemens.Portal.Plc
{
	public class PortalPlc : NotifyPropertyChanged, IPlc
	{

		#region Public Properties

		public string Name { get; set; }

		public IDictionary<BlockType, ICollection<IBlock>> Blocks { get; } = new ObservableDictionary<BlockType, ICollection<IBlock>>()
		{
			{ BlockType.DataBlock, new ObservableHashSet<IBlock>()},
			{ BlockType.Function, new ObservableHashSet<IBlock>()},
			{ BlockType.FunctionBlock, new ObservableHashSet<IBlock>()},
			{ BlockType.OrganizationBlock, new ObservableHashSet<IBlock>()}
		};

		public ICollection<UserDataType> UserDataTypes { get; } = new ObservableHashSet<UserDataType>();

		public ICollection<PlcTagTable> TagTables { get; } = new ObservableHashSet<PlcTagTable>();
		
		#endregion

		#region Public Methods
		
		public T GetConstantValue<T>(Constant<T> constant)
			where T : struct
		{
			if (constant.HasValue)
				return constant.Value;
			else
			{
				var resolvedConstant = this.TagTables.SelectMany(t => t.Constants).FirstOrDefault(c => c.Name == constant.Name);
				if (resolvedConstant == null)
					throw new SiemensException("Cannot resolve constant '" + constant.Name + "'");
				else
				{
					try
					{
						if (SiemensConverter.TryParse(resolvedConstant.Value.ToString(), resolvedConstant.DataType, out object parsedValue))
						{
							return (T)Convert.ChangeType(parsedValue, typeof(T));
						}
						else
						{
							throw new SiemensException("Constant value for '" + constant.Name + "' is not the correct type: " + resolvedConstant.DataType.ToString());
						}
					}
					catch (Exception e)
					{
						throw new SiemensException("Cannot convert constant value '" + constant.Name + "' to type: " + resolvedConstant.DataType.ToString(), e);
					}
				}
			}
		}

		#endregion
		
	}
}
