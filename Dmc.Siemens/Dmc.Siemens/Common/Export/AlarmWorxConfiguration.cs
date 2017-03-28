using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.IO;
using Dmc.Siemens.Base;
using Dmc.Siemens.Common.PLC;
using Dmc.Siemens.Common.PLC.Interfaces;
using Dmc.Siemens.Portal.Base;

namespace Dmc.Siemens.Common.Export
{
	public static class AlarmWorxConfiguration
	{

		#region Public Methods

		public static void CreateFromBlocks(IEnumerable<IBlock> blocks, string path, string opcServerPrefix, IPortalPlc parentPlc)
		{
			if (blocks == null)
				throw new ArgumentNullException(nameof(blocks));
			IEnumerable<DataBlock> dataBlocks;
			if ((dataBlocks = blocks.OfType<DataBlock>())?.Count() <= 0)
				throw new ArgumentException("Blocks does not contain any valid DataBlocks.", nameof(blocks));

			AlarmWorxConfiguration.CreateFromBlocksInternal(blocks, path, opcServerPrefix, parentPlc);
		}

		#endregion

		#region Private Methods

		private static void CreateFromBlocksInternal(IEnumerable<IBlock> blocks, string path, string opcServerPrefix, IPortalPlc parentPlc)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!FileHelpers.CheckValidFilePath(path, ".csv"))
				throw new ArgumentException(path + " is not a valid path.", nameof(path));

			try
			{
				using (var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
				{
					StreamWriter writer = new StreamWriter(file);

					AlarmWorxConfiguration.WriteHeaders(writer);

					foreach (var block in blocks)
					{
						if (block == null)
							throw new ArgumentNullException(nameof(block));
						//if (block.Children?.Count <= 0)
						//	throw new ArgumentException("Block '" + block.Name + "' contains no data", nameof(block));

						//AlarmWorxConfiguration.ExportDataBlockToFile(block, writer, parentPlc);
					}

					writer.Flush();
				}
			}
			catch (Exception e)
			{
				throw new SiemensException("Could not write Kepware configuration", e);
			}
		}

		private static void WriteHeaders(TextWriter writer)
		{
			writer.WriteLine(@"#AWX_Source;");
			writer.WriteLine("LocationPath,Name,Description,LastModified,Input1,BaseText,Enabled,DefaultDisplay,HelpText,ModifiedSeqNo,LIM_RTNText,LIM_Input2,LIM_Deadband,LIM_HIHI_RequiresAck,LIM_HIHI_Severity,LIM_HIHI_Limit,LIM_HIHI_MsgText,LIM_HI_RequiresAck,LIM_HI_Severity,LIM_HI_Limit,LIM_HI_MsgText,LIM_LO_RequiresAck,LIM_LO_Severity,LIM_LO_Limit,LIM_LO_MsgText,LIM_LOLO_RequiresAck,LIM_LOLO_Severity,LIM_LOLO_Limit,LIM_LOLO_MsgText,DEV_RTNText,DEV_Input2,DEV_Deadband,DEV_HIHI_RequiresAck,DEV_HIHI_Severity,DEV_HIHI_Limit,DEV_HIHI_MsgText,DEV_HI_RequiresAck,DEV_HI_Severity,DEV_HI_Limit,DEV_HI_MsgText,DEV_LO_RequiresAck,DEV_LO_Severity,DEV_LO_Limit,DEV_LO_MsgText,DEV_LOLO_RequiresAck,DEV_LOLO_Severity,DEV_LOLO_Limit,DEV_LOLO_MsgText,DIG_RTNText,DIG_Input2,DIG_Deadband,DIG_RequiresAck,DIG_Severity,DIG_Limit,DIG_MsgText,ROC_RTNText,ROC_Input2,ROC_Deadband,ROC_RequiresAck,ROC_Severity,ROC_Limit,ROC_MsgText,RelatedValue1,RelatedValue2,RelatedValue3,RelatedValue4,RelatedValue5,RelatedValue6,RelatedValue7,RelatedValue8,RelatedValue9,RelatedValue10,Delay,ROC_AckOnRTN,DIG_AckOnRTN,LIM_AckOnRTN,DEV_AckOnRTN,RelatedValue11,RelatedValue12,RelatedValue13,RelatedValue14,RelatedValue15,RelatedValue16,RelatedValue17,RelatedValue18,RelatedValue19,RelatedValue20,RLM_RTNText,RLM_Input2,RLM_Deadband,RLM_HIHI_RequiresAck,RLM_HIHI_Severity,RLM_HIHI_Limit,RLM_HIHI_MsgText,RLM_HI_RequiresAck,RLM_HI_Severity,RLM_HI_Limit,RLM_HI_MsgText,RLM_LO_RequiresAck,RLM_LO_Severity,RLM_LO_Limit,RLM_LO_MsgText,RLM_LOLO_RequiresAck,RLM_LOLO_Severity,RLM_LOLO_Limit,RLM_LOLO_MsgText,RLM_AckOnRTN,TLA_RTNText,TLA_Input2,TLA_Deadband,TLA_RequiresAck,TLA_Severity,TLA_Limit,TLA_MsgText,TLA_AckOnRTN,TemplateId,EnableClear,ExcludeEqualTo,DelayOnAlarmOnly,AlarmTreeWritesEnabled,AlarmTreeEnumType");
		}

		#endregion

	}
}
