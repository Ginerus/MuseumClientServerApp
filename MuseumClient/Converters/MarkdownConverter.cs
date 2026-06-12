using Markdig;

namespace MuseumClient.Services
{
    public static class MarkdownConverter
    {
        public static string ConvertToHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var html = Markdown.ToHtml(markdown, pipeline);

            return $@"
<html>
<head>
<meta charset='utf-8'>
<style>
body {{
    font-family: Segoe UI;
    padding: 20px;
    line-height: 1.6;
}}
code {{
    background: #f4f4f4;
    padding: 2px 4px;
    border-radius: 4px;
}}
pre {{
    background: #1e1e1e;
    color: white;
    padding: 10px;
    border-radius: 6px;
    overflow-x: auto;
}}
</style>
</head>
<body>
{html}
</body>
</html>";
        }
    }
}