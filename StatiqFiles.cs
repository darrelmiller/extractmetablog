using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace ExtractMetablog
{
    public class StatiqFiles {
        private readonly string _basePath;
        private readonly Uri sourceUrl;
        private HttpClient _httpClient = new HttpClient();

        public StatiqFiles(string basePath, Uri sourceUrl)
        {
            this._basePath = basePath;
            this.sourceUrl = sourceUrl;
        }

        public async Task WritePost(MetablogPost post) {
            var targetPath= Path.Combine(_basePath, post.Slug + ".html");
            var dir = Path.GetDirectoryName(targetPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await DownloadAssets(post);

            using (var writer = File.CreateText(Path.Combine(_basePath, post.Slug + ".md")))
            {
                // Write Front Matter
                writer.WriteLine($"Title: \"{post.Title}\"");
                writer.WriteLine($"Published: {post.DateCreated.ToString("yyyy-MM-dd")}");
                writer.WriteLine("---");
                // Content
                
                writer.WriteLine(post.Description);
                await writer.FlushAsync();
            }
            
        }
        

        private async Task DownloadAssets(MetablogPost post)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(post.Description);
            var htmlNodes = doc.DocumentNode.SelectNodes("//img");
            if (htmlNodes != null)
            {
                foreach (var imgNode in htmlNodes)
                {
                    var link = imgNode.GetAttributeValue("src", "");
                    if (link != "")
                    {
                        Console.WriteLine("Downloading image: " + link);
                        if (link.Contains(this.sourceUrl.OriginalString))
                        {
                            await DownloadImage(link, post.Slug);
                            Uri newUrl = RewriteAssetUrl(this.sourceUrl, post.Slug, link);
                            imgNode.SetAttributeValue("src", newUrl.OriginalString);

                        }
                    }
                }
            }
            post.Description = doc.DocumentNode.InnerHtml.Replace("@", "@@");

            Uri RewriteAssetUrl(Uri sourceUrl, string slug, string originalLink)
            {
                var url = new Uri(originalLink);
                var newUrl = new Uri(sourceUrl, slug + "/" + url.Segments.Last()); // host
                return newUrl;
            }
        }

        private async Task DownloadImage(string link, string slug)
        {
            var url = new Uri(link);
            var response = await _httpClient.GetAsync(link);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var filename = url.Segments.Last();
                var dir = Path.Combine(_basePath, slug);
                if (!Directory.Exists(dir)) {
                      Directory.CreateDirectory(dir);
                }
                using (var fileStream = File.Create(Path.Combine(dir, filename)))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
        }
    }
}