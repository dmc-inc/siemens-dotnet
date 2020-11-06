namespace Dmc.Siemens.Common.Export.Base
{
    public sealed class AlarmworxSettings
    {

        public string OpcServerTagPrefix { get; set; }

        public AlarmworxExportType ExportType { get; set; }

        public OpcTagStyle ExportStyle { get; set; }

    }
}
