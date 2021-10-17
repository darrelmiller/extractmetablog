using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace ExtractMetablog
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand();
            rootCommand.AddOption(new Option("--sourceUrl", argumentType: typeof(Uri)));
            rootCommand.AddOption(new Option("--blogId", argumentType: typeof(string)));
            rootCommand.AddOption(new Option("--userName", argumentType: typeof(string)));
            rootCommand.AddOption(new Option("--password", argumentType: typeof(string)));
            rootCommand.AddOption(new Option("--topPosts",argumentType: typeof(int)));
            rootCommand.AddOption(new Option("--output", argumentType: typeof(string)));

            rootCommand.Handler= CommandHandler.Create<Uri,string,string,string,int,string>(GetPostsAsync);
            return await rootCommand.InvokeAsync(args);
        }

        static private async Task GetPostsAsync(Uri sourceUrl, string blogId, string userName, string password, int topPosts, string output)
        {
            var mbd = new MetablogDownloader(sourceUrl, blogId, userName, password);
            var posts = await mbd.GetRecentPosts(topPosts);
            var files = new StatiqFiles(output, sourceUrl);

            foreach (var post in posts)
            {
                await files.WritePost(post);
                Console.WriteLine($"Title: {post.Title}");
                Console.WriteLine($"Link: {post.Link}");
                Console.WriteLine($"Slug: {post.Slug}");
                Console.WriteLine($"Date Created: {post.DateCreated}");
                //   Console.WriteLine($"Description: {post.Description}");
            }
        }
    }
}
