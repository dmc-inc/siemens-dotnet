using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dmc.IO;
using Dmc.Siemens.Common.Base;
using Dmc.Siemens.Common.Export.Base;
using Dmc.Siemens.Common.Plc;
using Dmc.Siemens.Common.Plc.Interfaces;
using Dmc.Siemens.Portal.Base;
using Dmc.Siemens.Portal.Plc;

namespace Dmc.Siemens.Common.Export
{
	public static class AlarmWorxConfiguration
	{

		#region Constants

		private const string ALARM_FOLDER = "ImportConfiguration";

		#endregion

		#region Public Methods

		public static void Create(IEnumerable<IBlock> blocks, string path, string opcServerPrefix, PortalPlc parentPlc)
		{
			if (blocks == null)
				throw new ArgumentNullException(nameof(blocks));
			IEnumerable<DataBlock> dataBlocks;
			if ((dataBlocks = blocks.OfType<DataBlock>())?.Count() <= 0)
				throw new ArgumentException("Blocks does not contain any valid DataBlocks.", nameof(blocks));

			AlarmWorxConfiguration.CreateInternal(dataBlocks, path, opcServerPrefix, parentPlc);
		}

		public static void Create(DataBlock block, string path, string opcServerPrefix, PortalPlc parentPlc)
		{
			AlarmWorxConfiguration.Create(new[] { block }, path, opcServerPrefix, parentPlc);
		}

		#endregion

		#region Private Methods

		private static void CreateInternal(IEnumerable<DataBlock> dataBlocks, string path, string opcServerPrefix, PortalPlc parentPlc)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			if (!FileHelpers.IsValidFilePath(path, ".csv"))
				throw new ArgumentException(path + " is not a valid path.", nameof(path));

			try
			{
				using (var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Read))
				using (StreamWriter writer = new StreamWriter(file))
				{
					AlarmWorxConfiguration.WriteHeaders(writer);

					foreach (var block in dataBlocks)
					{
						if (block == null)
							throw new ArgumentNullException(nameof(block));
						if (block.Children?.Count <= 0)
							throw new ArgumentException("Block '" + block.Name + "' contains no data", nameof(block));
						
						foreach (var child in block)
						{
							WriteAlarmRow(writer, child, block.Name + ".", block.Comment);
						}
					}
                    AlarmWorxConfiguration.WriteFooter(writer);

					writer.Flush();
				}
			}
			catch (Exception e)
			{
				throw new SiemensException("Could not write AlarmWorX configuration", e);
			}

			void WriteAlarmRow(StreamWriter writer, DataEntry entry, string prependNameText, string prependCommentText)
			{
				string stackedComment;
				if (string.IsNullOrWhiteSpace(prependCommentText))
					stackedComment = entry.Comment.Replace(",","-");
				else if (prependCommentText.EndsWith(" - "))
					stackedComment = prependCommentText + entry.Comment;
				else
					stackedComment = prependCommentText + " - " + entry.Comment;

				switch (entry.DataType)
				{
					case DataType.ARRAY:
						TagHelper.ResolveArrayChildren(entry, parentPlc);
						foreach (var child in entry.Children)
						{
							WriteAlarmRow(writer, child, prependNameText, prependNameText);
						}
						break;
					case DataType.STRUCT:
						foreach (var child in entry.Children)
						{
							WriteAlarmRow(writer, child, prependNameText + entry.Name + ".", stackedComment);
						}
						break;
					case DataType.UDT:
						foreach (var child in parentPlc.UserDataTypes.FirstOrDefault(u => u.Name == entry.DataTypeName))
						{
							WriteAlarmRow(writer, child, prependNameText + entry.Name + ".", stackedComment);
						}
						break;
					case DataType.BOOL:
                        AlarmWorxRow row = new AlarmWorxRow
                        {
                            LocationPath = @"\\Alarm Configurations\" + ALARM_FOLDER,
                            Name = ALARM_FOLDER + "." + prependNameText + entry.Name,
                            Description = stackedComment,
                            LastModified = DateTime.Now.ToString(),
                            Input1 = opcServerPrefix + "\\" + prependNameText + entry.Name,
                            BaseText = stackedComment,  // Message text 
                            DigMessageText = " ",   // Prevents 'Digital Alarm' text at the end of each message
                            DigLimit = "1",     // Alarm state value needs to be 1 for a digital
                            DigSeverity = "500", // Default severity is 500
                            DigRequiresAck = "1" // Require an acknowledge by default
                        };

                        writer.WriteLine(row.ToString());
						break;
					default:
						throw new ArgumentException("Cannot export datatype: " + entry.DataType.ToString() + " to AlarmWorX configuration");
				}
			}

		}

		private static void WriteHeaders(TextWriter writer)
		{
			writer.WriteLine(@"#Ico.AwxServer.BusinessEntities.AwxSvrSource,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,");
			writer.WriteLine("{LocationPath},Name,Description,LastModified,Input1,BaseText,Enabled,DefaultDisplay,HelpText,ModifiedSeqNo,LIM_RTNText,LIM_Input2,LIM_Deadband,LIM_HIHI_RequiresAck,LIM_HIHI_Severity,LIM_HIHI_Limit,LIM_HIHI_MsgText,LIM_HI_RequiresAck,LIM_HI_Severity,LIM_HI_Limit,LIM_HI_MsgText,LIM_LO_RequiresAck,LIM_LO_Severity,LIM_LO_Limit,LIM_LO_MsgText,LIM_LOLO_RequiresAck,LIM_LOLO_Severity,LIM_LOLO_Limit,LIM_LOLO_MsgText,DEV_RTNText,DEV_Input2,DEV_Deadband,DEV_HIHI_RequiresAck,DEV_HIHI_Severity,DEV_HIHI_Limit,DEV_HIHI_MsgText,DEV_HI_RequiresAck,DEV_HI_Severity,DEV_HI_Limit,DEV_HI_MsgText,DEV_LO_RequiresAck,DEV_LO_Severity,DEV_LO_Limit,DEV_LO_MsgText,DEV_LOLO_RequiresAck,DEV_LOLO_Severity,DEV_LOLO_Limit,DEV_LOLO_MsgText,DIG_RTNText,DIG_Input2,DIG_Deadband,DIG_RequiresAck,DIG_Severity,DIG_Limit,DIG_MsgText,ROC_RTNText,ROC_Input2,ROC_Deadband,ROC_RequiresAck,ROC_Severity,ROC_Limit,ROC_MsgText,RelatedValue1,RelatedValue2,RelatedValue3,RelatedValue4,RelatedValue5,RelatedValue6,RelatedValue7,RelatedValue8,RelatedValue9,RelatedValue10,Delay,ROC_AckOnRTN,DIG_AckOnRTN,LIM_AckOnRTN,DEV_AckOnRTN,RelatedValue11,RelatedValue12,RelatedValue13,RelatedValue14,RelatedValue15,RelatedValue16,RelatedValue17,RelatedValue18,RelatedValue19,RelatedValue20,RLM_RTNText,RLM_Input2,RLM_Deadband,RLM_HIHI_RequiresAck,RLM_HIHI_Severity,RLM_HIHI_Limit,RLM_HIHI_MsgText,RLM_HI_RequiresAck,RLM_HI_Severity,RLM_HI_Limit,RLM_HI_MsgText,RLM_LO_RequiresAck,RLM_LO_Severity,RLM_LO_Limit,RLM_LO_MsgText,RLM_LOLO_RequiresAck,RLM_LOLO_Severity,RLM_LOLO_Limit,RLM_LOLO_MsgText,RLM_AckOnRTN,TLA_RTNText,TLA_Input2,TLA_Deadband,TLA_RequiresAck,TLA_Severity,TLA_Limit,TLA_MsgText,TLA_AckOnRTN,TemplateId,EnableClear,ExcludeEqualTo,DelayOnAlarmOnly,AlarmTreeWritesEnabled,AlarmTreeEnumType");
		}

        private static void WriteFooter(TextWriter writer)
        {
            writer.WriteLine("");
            writer.WriteLine("#Version,,");
            writer.WriteLine("ExportTime, IEVersion, SupportedVersion");
            writer.Write(DateTime.Now.ToString("M/d/yyyy HH:MM:ss tt") + ", 10.90.169.00, 8.20");
        }

        #endregion

    }
}
