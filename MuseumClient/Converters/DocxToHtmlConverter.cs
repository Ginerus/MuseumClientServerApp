using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Text;

namespace MuseumClient.Services
{
    public static class DocxToHtmlConverter
    {
        public static string Convert(string filePath)
        {
            using var doc = WordprocessingDocument.Open(filePath, false);

            var body = doc.MainDocumentPart?.Document.Body;
            if (body == null)
                return "<p>Пустой документ</p>";

            var sb = new StringBuilder();

            sb.Append("<html><body style='font-family:Segoe UI; padding:20px;'>");

            foreach (var element in body.Elements())
            {
                if (element is Paragraph p)
                {
                    sb.Append("<p style='margin-bottom:10px;'>");

                    foreach (var run in p.Elements<Run>())
                    {
                        foreach (var text in run.Elements<Text>())
                        {
                            sb.Append(System.Net.WebUtility.HtmlEncode(text.Text));
                        }
                    }

                    sb.Append("</p>");
                }
            }

            sb.Append("</body></html>");

            return sb.ToString();
        }
    }
}