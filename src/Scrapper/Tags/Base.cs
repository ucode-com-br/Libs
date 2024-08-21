using System.Collections.Generic;

namespace UCode.Scrapper.Tags
{
    public record Base : GlobalAttributes
    {
        public Base(int index, string? href, string? target,
            string? accesskey, string? @class, string? contenteditable, IDictionary<string?, string?>? data, string? dir, string? draggable, string? hidden, string? id, string? lang, string? spellcheck, string? style, string? tabindex, string? title, string? translate,
            string? itemprop, string? itemid, string? itemref, string? itemscope, string? itemtype, string? nonce) : base(accesskey, @class, contenteditable, data, dir, draggable, hidden, id, lang, spellcheck, style, tabindex, title, translate, itemprop, itemid, itemref, itemscope, itemtype, nonce)
        {
            this.Index = index;
            this.Href = href;
            this.Target = target;
        }

        public int Index
        {
            get;
        }
        public string? Href
        {
            get;
        }
        public string? Target
        {
            get;
        }
    }
}
