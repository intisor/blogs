using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Statiq.Common;

namespace blogs
{
    public class SmallUrlModule : ParallelModule
    {
        private readonly string _apiKey;
        private readonly string _apiBase;

        public SmallUrlModule(string apiKey, string? apiBase = null)
        {
            _apiKey = apiKey;
            _apiBase = apiBase ?? "https://i.intitech.dev/api/process-blog";
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            // Only process HTML files
            if (input.Destination.Extension != ".html")
                return input.Yield();

            // Only process posts
            bool isPost = input.GetBool("IsPost") || 
                          input.Destination.FullPath.StartsWith("posts/", StringComparison.OrdinalIgnoreCase);

            if (!isPost)
                return input.Yield();

            var content = await input.GetContentStringAsync();
            var blogUrl = $"https://blogs.intitech.dev{context.GetLink(input)}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);

            var requestBody = new
            {
                content,
                blogUrl
            };

            var json = JsonSerializer.Serialize(requestBody);
            var requestContent = new System.Net.Http.StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync(_apiBase, requestContent);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("processedContent", out var processedContentProp))
                    {
                        var processedContent = processedContentProp.GetString();
                        var shortUrl = root.TryGetProperty("blogShortUrl", out var urlProp) ? urlProp.GetString() : null;
                        var shortSlug = root.TryGetProperty("blogSlug", out var slugProp) ? slugProp.GetString() : null;

                        if (!string.IsNullOrEmpty(shortSlug) && !string.IsNullOrEmpty(processedContent))
                        {
                            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                            htmlDoc.LoadHtml(processedContent);
                            var head = htmlDoc.DocumentNode.SelectSingleNode("//head");
                            if (head != null)
                            {
                                // Check if og:image already exists (it shouldn't, but safety first)
                                if (htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='og:image']") == null)
                                {
                                    var ogImage = htmlDoc.CreateElement("meta");
                                    ogImage.SetAttributeValue("property", "og:image");
                                    ogImage.SetAttributeValue("content", $"https://i.intitech.dev/og/{shortSlug}");
                                    head.AppendChild(ogImage);
                                }

                                if (htmlDoc.DocumentNode.SelectSingleNode("//meta[@name='twitter:image']") == null)
                                {
                                    var twitterImage = htmlDoc.CreateElement("meta");
                                    twitterImage.SetAttributeValue("name", "twitter:image");
                                    twitterImage.SetAttributeValue("content", $"https://i.intitech.dev/og/{shortSlug}");
                                    head.AppendChild(twitterImage);
                                }
                                processedContent = htmlDoc.DocumentNode.OuterHtml;
                            }
                        }

                        return input
                            .Clone(new MetadataItems
                            {
                                { "ShortUrl", shortUrl },
                                { "ShortSlug", shortSlug }
                            })
                            .Clone(context.GetContentProvider(processedContent, MediaTypes.Html))
                            .Yield();
                    }
                }
                else
                {
                    context.LogWarning(input, $"SmallURL API failed for {blogUrl}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                context.LogWarning(input, $"SmallURL processing error: {ex.Message}");
            }

            return input.Yield();
        }
    }
}
