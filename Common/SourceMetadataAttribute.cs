using System;

namespace Dmc.Siemens.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    internal sealed class SourceMetadataAttribute : Attribute
    {

        public string Keyword { get; set; }

        public char Separator { get; set; }

        public string ValuePattern { get; set; }

        public SourceMetadataAttribute(string keyword, char separator = ':', string valuePattern = null)
        {
            this.Keyword = keyword;
            this.Separator = separator;
            this.ValuePattern = valuePattern;
        }

    }
}
