using System.Collections.Generic;
namespace UCode.Scrapper.Tags
{
    public record Meta : GlobalAttributes
    {
        public Meta(int index, string? name, string? charset, string? httpEquiv, string? property, string? content, string? inner,
            string? accesskey, string? @class, string? contenteditable, IDictionary<string?, string?>? data, string? dir, string? draggable, string? hidden, string? id, string? lang, string? spellcheck, string? style, string? tabindex, string? title, string? translate,
            string? itemprop, string? itemid, string? itemref, string? itemscope, string? itemtype, string? nonce) : base(accesskey, @class, contenteditable, data, dir, draggable, hidden, id, lang, spellcheck, style, tabindex, title, translate, itemprop, itemid, itemref, itemscope, itemtype, nonce)
        {
            this.Index = index;
            this.Name = name;
            this.Charset = charset;
            this.HttpEquiv = httpEquiv;
            this.Property = property;
            this.Content = content;
            this.Inner = inner;
        }

        public int Index
        {
            get;
        }
        public string? Name
        {
            get;
        }
        public string? Charset
        {
            get;
        }
        public string? HttpEquiv
        {
            get;
        }
        public string? Property
        {
            get;
        }
        public string? Content
        {
            get;
        }
        public string? Inner
        {
            get;
        }

    }
}
