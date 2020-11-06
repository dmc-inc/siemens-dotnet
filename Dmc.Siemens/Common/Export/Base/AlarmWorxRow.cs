using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dmc.Siemens.Common.Export.Base
{
	internal sealed class AlarmWorxRow
	{

		#region Public Properties

		public string LocationPath { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public string LastModified { get; set; } = string.Empty;
		public string Input1 { get; set; } = string.Empty;
		public string BaseText { get; set; } = string.Empty;
		public string Enabled { get; set; } = string.Empty;
		public string DefaultDisplay { get; set; } = string.Empty;
		public string HelpText { get; set; } = string.Empty;
		public string ModifiedSeqNo { get; set; } = string.Empty;
		public string LimRtnText { get; set; } = string.Empty;
		public string LimInput2 { get; set; } = string.Empty;
		public string LimDeadband { get; set; } = string.Empty;
		public string LimHiHiRequiresAck { get; set; } = string.Empty;
		public string LimHiHiSeverity { get; set; } = string.Empty;
		public string LimHiHiLimit { get; set; } = string.Empty;
		public string LimHiHiMessageText { get; set; } = string.Empty;
		public string LimHiRequiresAck { get; set; } = string.Empty;
		public string LimHiSeverity { get; set; } = string.Empty;
		public string LimHiLimit { get; set; } = string.Empty;
		public string LimHiMessageText { get; set; } = string.Empty;
		public string LimLoRequiresAck { get; set; } = string.Empty;
		public string LimLoSeverity { get; set; } = string.Empty;
		public string LimLoLimit { get; set; } = string.Empty;
		public string LimLoMessageText { get; set; } = string.Empty;
		public string LimLoLoRequiresAck { get; set; } = string.Empty;
		public string LimLoLoSeverity { get; set; } = string.Empty;
		public string LimLoLoLimit { get; set; } = string.Empty;
		public string LimLoLoMessageText { get; set; } = string.Empty;
		public string DevRtnText { get; set; } = string.Empty;
		public string DevInput2 { get; set; } = string.Empty;
		public string DevDeadband { get; set; } = string.Empty;
		public string DevHiHiRequiresAck { get; set; } = string.Empty;
		public string DevHiHiSeverity { get; set; } = string.Empty;
		public string DevHiHiLimit { get; set; } = string.Empty;
		public string DevHiHiMessageText { get; set; } = string.Empty;
		public string DevHiRequiresAck { get; set; } = string.Empty;
		public string DevHiSeverity { get; set; } = string.Empty;
		public string DevHiLimit { get; set; } = string.Empty;
		public string DevHiMessageText { get; set; } = string.Empty;
		public string DevLoRequiresAck { get; set; } = string.Empty;
		public string DevLoSeverity { get; set; } = string.Empty;
		public string DevLoLimit { get; set; } = string.Empty;
		public string DevLoMessageText { get; set; } = string.Empty;
		public string DevLoLoRequiresAck { get; set; } = string.Empty;
		public string DevLoLoSeverity { get; set; } = string.Empty;
		public string DevLoLoLimit { get; set; } = string.Empty;
		public string DevLoLoMessageText { get; set; } = string.Empty;
		public string DigRtnText { get; set; } = string.Empty;
		public string DigInput2 { get; set; } = string.Empty;
		public string DigDeadband { get; set; } = string.Empty;
		public string DigRequiresAck { get; set; } = string.Empty;
		public string DigSeverity { get; set; } = string.Empty;
		public string DigLimit { get; set; } = string.Empty;
		public string DigMessageText { get; set; } = string.Empty;
		public string RocRtnText { get; set; } = string.Empty;
		public string RocInput2 { get; set; } = string.Empty;
		public string RocDeadband { get; set; } = string.Empty;
		public string RocRequiresAck { get; set; } = string.Empty;
		public string RocSeverity { get; set; } = string.Empty;
		public string RocLimit { get; set; } = string.Empty;
		public string RocMessageText { get; set; } = string.Empty;
		public string RelatedValue1 { get; set; } = string.Empty;
		public string RelatedValue2 { get; set; } = string.Empty;
		public string RelatedValue3 { get; set; } = string.Empty;
		public string RelatedValue4 { get; set; } = string.Empty;
		public string RelatedValue5 { get; set; } = string.Empty;
		public string RelatedValue6 { get; set; } = string.Empty;
		public string RelatedValue7 { get; set; } = string.Empty;
		public string RelatedValue8 { get; set; } = string.Empty;
		public string RelatedValue9 { get; set; } = string.Empty;
		public string RelatedValue10 { get; set; } = string.Empty;
		public string Delay { get; set; } = string.Empty;
		public string RocAckOnRtn { get; set; } = string.Empty;
		public string DigAckOnRtn { get; set; } = string.Empty;
		public string DevAckOnRtn { get; set; } = string.Empty;
		public string RelatedValue11 { get; set; } = string.Empty;
		public string RelatedValue12 { get; set; } = string.Empty;
		public string RelatedValue13 { get; set; } = string.Empty;
		public string RelatedValue14 { get; set; } = string.Empty;
		public string RelatedValue15 { get; set; } = string.Empty;
		public string RelatedValue16 { get; set; } = string.Empty;
		public string RelatedValue17 { get; set; } = string.Empty;
		public string RelatedValue18 { get; set; } = string.Empty;
		public string RelatedValue19 { get; set; } = string.Empty;
		public string RelatedValue20 { get; set; } = string.Empty;
		public string RlmRtnText { get; set; } = string.Empty;
		public string RlmInput2 { get; set; } = string.Empty;
		public string RlmDeadband { get; set; } = string.Empty;
		public string RlmHiHiRequiresAck { get; set; } = string.Empty;
		public string RlmHiHiSeverity { get; set; } = string.Empty;
		public string RlmHiHiLimit { get; set; } = string.Empty;
		public string RlmHiHiMessageText { get; set; } = string.Empty;
		public string RlmHiRequiresAck { get; set; } = string.Empty;
		public string RlmHiSeverity { get; set; } = string.Empty;
		public string RlmHiLimit { get; set; } = string.Empty;
		public string RlmHiMessageText { get; set; } = string.Empty;
		public string RlmLoRequiresAck { get; set; } = string.Empty;
		public string RlmLoSeverity { get; set; } = string.Empty;
		public string RlmLoLimit { get; set; } = string.Empty;
		public string RlmLoMessageText { get; set; } = string.Empty;
		public string RlmLoLoRequiresAck { get; set; } = string.Empty;
		public string RlmLoLoSeverity { get; set; } = string.Empty;
		public string RlmLoLoLimit { get; set; } = string.Empty;
		public string RlmLoLoMessageText { get; set; } = string.Empty;
		public string RlmAckOnRtn { get; set; } = string.Empty;
		public string TlaRtnText { get; set; } = string.Empty;
		public string TlaInput2 { get; set; } = string.Empty;
		public string TlaDeadband { get; set; } = string.Empty;
		public string TlaRequiresAck { get; set; } = string.Empty;
		public string TlaSeverity { get; set; } = string.Empty;
		public string TlaLimit { get; set; } = string.Empty;
		public string TlaMessageText { get; set; } = string.Empty;
		public string TlaAckOnRtn { get; set; } = string.Empty;
		public string TemplateId { get; set; } = string.Empty;
		public string EnableClear { get; set; } = string.Empty;
		public string ExcludeEqualTo { get; set; } = string.Empty;
		public string DelayOnAlarmOnly { get; set; } = string.Empty;
		public string AlarmTreeWritesEnabled { get; set; } = string.Empty;
		public string AlarmTreeEnumType { get; set; } = string.Empty;

		#endregion

		#region Public Methods

		// Fuck alarmworx.  Seriously... worst CSV format ever
		public override string ToString()
		{
			return string.Join(",",
				this.LocationPath,
				this.Name,
				this.Description,
				this.LastModified,
				this.Input1,
				this.BaseText,
				this.Enabled,
				this.DefaultDisplay,
				this.HelpText,
				this.ModifiedSeqNo,
				this.LimRtnText,
				this.LimInput2,
				this.LimDeadband,
				this.LimHiHiRequiresAck,
				this.LimHiHiSeverity,
				this.LimHiHiLimit,
				this.LimHiHiMessageText,
				this.LimHiRequiresAck,
				this.LimHiSeverity,
				this.LimHiLimit,
				this.LimHiMessageText,
				this.LimLoRequiresAck,
				this.LimLoSeverity,
				this.LimLoLimit,
				this.LimLoMessageText,
				this.LimLoLoRequiresAck,
				this.LimLoLoSeverity,
				this.LimLoLoLimit,
				this.LimLoLoMessageText,
				this.DevRtnText,
				this.DevInput2,
				this.DevDeadband,
				this.DevHiHiRequiresAck,
				this.DevHiHiSeverity,
				this.DevHiHiLimit,
				this.DevHiHiMessageText,
				this.DevHiRequiresAck,
				this.DevHiSeverity,
				this.DevHiLimit,
				this.DevHiMessageText,
				this.DevLoRequiresAck,
				this.DevLoSeverity,
				this.DevLoLimit,
				this.DevLoMessageText,
				this.DevLoLoRequiresAck,
				this.DevLoLoSeverity,
				this.DevLoLoLimit,
				this.DevLoLoMessageText,
				this.DigRtnText,
				this.DigInput2,
				this.DigDeadband,
				this.DigRequiresAck,
				this.DigSeverity,
				this.DigLimit,
				this.DigMessageText,
				this.RocRtnText,
				this.RocInput2,
				this.RocDeadband,
				this.RocRequiresAck,
				this.RocSeverity,
				this.RocLimit,
				this.RocMessageText,
				this.RelatedValue1,
				this.RelatedValue2,
				this.RelatedValue3,
				this.RelatedValue4,
				this.RelatedValue5,
				this.RelatedValue6,
				this.RelatedValue7,
				this.RelatedValue8,
				this.RelatedValue9,
				this.RelatedValue10,
				this.Delay,
				this.RocAckOnRtn,
				this.DigAckOnRtn,
				this.DevAckOnRtn,
				this.RelatedValue11,
				this.RelatedValue12,
				this.RelatedValue13,
				this.RelatedValue14,
				this.RelatedValue15,
				this.RelatedValue16,
				this.RelatedValue17,
				this.RelatedValue18,
				this.RelatedValue19,
				this.RelatedValue20,
				this.RlmRtnText,
				this.RlmInput2,
				this.RlmDeadband,
				this.RlmHiHiRequiresAck,
				this.RlmHiHiSeverity,
				this.RlmHiHiLimit,
				this.RlmHiHiMessageText,
				this.RlmHiRequiresAck,
				this.RlmHiSeverity,
				this.RlmHiLimit,
				this.RlmHiMessageText,
				this.RlmLoRequiresAck,
				this.RlmLoSeverity,
				this.RlmLoLimit,
				this.RlmLoMessageText,
				this.RlmLoLoRequiresAck,
				this.RlmLoLoSeverity,
				this.RlmLoLoLimit,
				this.RlmLoLoMessageText,
				this.RlmAckOnRtn,
				this.TlaRtnText,
				this.TlaInput2,
				this.TlaDeadband,
				this.TlaRequiresAck,
				this.TlaSeverity,
				this.TlaLimit,
				this.TlaMessageText,
				this.TlaAckOnRtn,
				this.TemplateId,
				this.EnableClear,
				this.ExcludeEqualTo,
				this.DelayOnAlarmOnly,
				this.AlarmTreeWritesEnabled,
				this.AlarmTreeEnumType
				);
		}

		#endregion

	}
}
