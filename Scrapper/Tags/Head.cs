using System.Collections.Generic;
using HtmlAgilityPack;

namespace UCode.Scrapper.Tags
{
    public record Head
    {
        private readonly HtmlDocument _HeadDocument;


        public Head(HtmlDocument headDocument) => this._HeadDocument = headDocument;


        private string? _title;
        public string Title
        {
            get
            {
                if (this._title == null)
                {
                    var title = this._HeadDocument.DocumentNode.SelectSingleNode("//title");

                    if (title != null)
                    {
                        this._title = title.InnerText;
                    }
                    else
                    {
                        this._title = "";
                    }
                }


                return this._title;
            }
        }

        public List<Meta> Metas { get; } = new List<Meta>();

        public List<Base> Bases { get; } = new List<Base>();

        public List<Link> Links { get; } = new List<Link>();

        public override string ToString() => this._HeadDocument.DocumentNode.InnerHtml;

        public static implicit operator string(Head head) => head.ToString();
        public static implicit operator HtmlDocument(Head head) => head._HeadDocument;
        public static implicit operator HtmlNode(Head head) => head._HeadDocument.DocumentNode;
    }
}
