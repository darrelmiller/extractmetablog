using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExtractMetablog
{
    public class MetablogDownloader
    {
        private readonly Uri _blogUrl;
        private readonly string _blogId;
        private readonly string _userName;
        private readonly string _password;
        private readonly HttpClient _httpClient;

        public MetablogDownloader(Uri blogUrl, string blogId, string userName, string password, HttpClient httpClient = null)
        {
            this._blogUrl = blogUrl;
            this._blogId = blogId;
            this._userName = userName;
            this._password = password;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<IEnumerable<MetablogPost>> GetRecentPosts(int postCount) {
            
            var content = XmlRpcLib.CreateRPCCall("metaWeblog.getRecentPosts",_blogId, _userName,_password,postCount);
         
            var response = await _httpClient.PostAsync(new Uri(_blogUrl,"XmlRpc"),content);
            response.EnsureSuccessStatusCode();
            Console.WriteLine($"Retrieved document from blog: {_blogUrl}");
            var stream = await response.Content.ReadAsStreamAsync();
            var doc = await XDocument.LoadAsync(stream,LoadOptions.None,new CancellationToken());

            var value = doc.Elements("methodResponse").Elements("params").Elements("param").Elements("value").FirstOrDefault();
            var xPosts = value.Elements("array").Elements("data").Elements("value");
            return xPosts.Select(xp => new MetablogPost(xp));
        }  


    }

}
