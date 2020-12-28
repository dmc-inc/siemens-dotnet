namespace Dmc.Siemens.Common.Export.Base
{
    public enum WinccExportField
    {
        // Tags
        Name,
        Connection,
        PlcTag,
        DataType,
        Length,
        Address,

        // Alarms
        Id,
        AlarmText,
        TriggerTag,
        TriggerBit,
        InfoText,
        AckTag,
        Group,
        GroupID,
        GroupENU,
        ProcessAreaID,
        ProcessAreaENU,

        //Message Groups
        Parent,
        Layer
    }

    //TODO JAS:  Validate that this shouldn't be included somehow?

    //public enum Wincc7ExportField
    //{
    //    // Tags
    //    Name,
    //    Connection,
    //    PlcTag,
    //    DataType,
    //    Length,
    //    Address,

    //    // Alarms
    //    Id,
    //    AlarmText,
    //    TriggerTag,
    //    TriggerBit,
    //    InfoText
    //}
}
