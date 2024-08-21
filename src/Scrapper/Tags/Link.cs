using System.Collections.Generic;

namespace UCode.Scrapper.Tags
{
    public record Link : GlobalAttributes
    {
        public Link(int index, string? crossorigin, string? href, string? hreflang, string? media, string? referrerpolicy, string? rel, string? sizes, string? title, string? type, string? inner,
            string? accesskey, string? @class, string? contenteditable, IDictionary<string?, string?>? data, string? dir,
            string? draggable, string? hidden, string? id, string? lang, string? spellcheck, string? style, string? tabindex, string? translate,
            string? itemprop, string? itemid, string? itemref, string? itemscope, string? itemtype, string? nonce) : base(accesskey, @class, contenteditable, data, dir, draggable, hidden, id, lang, spellcheck, style, tabindex, title, translate, itemprop, itemid, itemref, itemscope, itemtype, nonce)
        {
            this.Index = index;
            this.Crossorigin = crossorigin;
            this.Href = href;
            this.Hreflang = hreflang;
            this.Media = media;
            this.Referrerpolicy = referrerpolicy;
            this.Rel = rel;
            this.Sizes = sizes;
            this.Type = type;
            this.Inner = inner;
        }

        public int Index
        {
            get;
        }
        public string? Crossorigin
        {
            get;
        }
        public string? Href
        {
            get;
        }
        public string? Hreflang
        {
            get;
        }
        public string? Media
        {
            get;
        }
        public string? Referrerpolicy
        {
            get;
        }
        public string? Rel
        {
            get;
        }
        public string? Sizes
        {
            get;
        }

        public string? Type
        {
            get;
        }
        public string? Inner
        {
            get;
        }
    }
}
