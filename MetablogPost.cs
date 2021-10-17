using System;
using System.Xml.Linq;
using System.Collections.Generic;

namespace ExtractMetablog
{
    public class MetablogPost
    {
        Dictionary<string, Action<MetablogPost,string>> _parsers = new Dictionary<string, Action<MetablogPost, string>>()
        {
            {"title", (mb,v) => mb.Title = v },
            {"description", (mb,v) => mb.Description = v },
            {"link", (mb,v) => mb.Link = v },
            {"wp_slug", (mb,v) => mb.Slug = v },
            {"dateCreated", (mb,v) => mb.DateCreated = DateTime.ParseExact(v,"yyyyMMddTHH:mm:ssZ",null) }
        };

        public MetablogPost()
        {
        }

        public MetablogPost(XElement xPost)
        {
            XmlRpcLib.ParseStruct(xPost.Element("struct"),_parsers,this);
        }

        public int PostId {get;set;}
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string Slug { get; set; }
        public DateTime DateCreated { get; set; }

    }    
}