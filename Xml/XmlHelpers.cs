using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace Dmc.Siemens.Xml
{
	public static class XmlHelpers
	{

		#region Internal Methods

		internal static string FormatXmlString(string xml, bool isPath = false)
		{
			if (isPath)
			{
				return Regex.Replace(xml, @"[\\/:*?""<>|]", string.Empty);
			}
			else
			{
				if (xml == null)
					return string.Empty;

				return Regex.Replace(xml, "([&])(?!amp;|gt;|apos;|lt;)", "&amp;").Replace("\"", "&quot;").Replace("'", "&apos;").Replace("<", "&lt;").Replace(">", "&gt;");
			}
		}
		
		internal static void WriteAttributeString(this XmlWriter writer, string name, string value, XmlStringType type)
		{
			writer.WriteAttributeString(name, XmlHelpers.FormatXmlString(value, type == XmlStringType.FilePath));
		}

		internal static void WriteElementString(this XmlWriter writer, string name, string value, XmlStringType type)
		{
			switch (type)
			{
				case XmlStringType.Default:
				case XmlStringType.FilePath:
					writer.WriteElementString(name, XmlHelpers.FormatXmlString(value, type == XmlStringType.FilePath));
					break;
				case XmlStringType.MultiLanguageText:
					writer.WriteStartElement(name);
					writer.WriteStartElement("MultiLanguageText");
					writer.WriteAttributeString("Lang", CultureInfo.CurrentCulture.Name);
					writer.WriteValue(value, XmlStringType.Default);
					writer.WriteEndElement();
					writer.WriteEndElement();
					break;
				default:
					break;
			}
			
		}

		internal static void WriteValue(this XmlWriter writer, object value, XmlStringType type)
		{
			writer.WriteString(XmlHelpers.FormatXmlString(value.ToString(), type == XmlStringType.FilePath));
		}

		#endregion

	}
}
