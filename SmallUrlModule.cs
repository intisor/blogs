using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Statiq.Common;

namespace blogs
{
    public class SmallUrlModule : ParallelModule
    {
        private readonly string _apiKey;
        private readonly string _apiBase = "https://i.intitech.dev/api/process-blog";

        public SmallUrlModule(string apiKey)
        {
            _apiKey = apiKey;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            // Only process HTML files that look like posts
            if (input.Destination.Extension != ".html")
                return input.Yield();

            // Skip if it's not in the 'posts' or 'blog' folder (adjust as needed)
            if (!input.Source.FullPath.Contains("/posts/"))
                return input.Yield();

            var content = await input.GetContentStringAsync();
            var blogUrl = $"https://blogs.intitech.dev{context.GetLink(input)}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);

            var requestBody = new
            {
                content = content,
                blogUrl = blogUrl
            };

            var json = JsonSerializer.Serialize(requestBody);
            var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(_apiBase, requestContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;

                    var processedContent = root.GetProperty("processedContent").GetString();
                    var shortUrl = root.GetProperty("blogShortUrl").GetString();
                    var shortSlug = root.GetProperty("blogSlug").GetString();

                    return input.Clone(
                        context.GetContentProvider(processedContent, MediaTypes.Html),
                        new MetadataItems
                        {
                            { "ShortUrl", shortUrl },
                            { "ShortSlug", shortSlug }
                        }).Yield();
                }
                else
                {
                    context.LogWarning($"SmallURL API failed for {blogUrl}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                context.LogWarning($"SmallURL processing error: {ex.Message}");
            }

            return input.Yield();
        }
    }
}
