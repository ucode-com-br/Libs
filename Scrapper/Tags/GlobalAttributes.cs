using System.Collections.Generic;

namespace UCode.Scrapper.Tags
{
    public record GlobalAttributes
    {
        public GlobalAttributes(string? accesskey, string? @class, string? contenteditable, IDictionary<string?, string?>? data, string? dir,
            string? draggable, string? hidden, string? id, string? lang, string? spellcheck, string? style, string? tabindex, string? title, string? translate, string? itemprop, string? itemid, string? itemref, string? itemscope, string? itemtype, string? nonce)
        {
            this.Accesskey = accesskey;
            this.Class = @class;
            this.Contenteditable = contenteditable;
            this.Data = data;
            this.Dir = dir;
            this.Draggable = draggable;
            this.Hidden = hidden;
            this.Id = id;
            this.Lang = lang;
            this.Spellcheck = spellcheck;
            this.Style = style;
            this.Tabindex = tabindex;
            this.Title = title;
            this.Translate = translate;
            this.Itemprop = itemprop;
            this.Itemid = itemid;
            this.Itemref = itemref;
            this.Itemscope = itemscope;
            this.Itemtype = itemtype;
            this.Nonce = nonce;
        }

        public string? Accesskey
        {
            get;
        }
        public string? Class
        {
            get;
        }
        public string? Contenteditable
        {
            get;
        }
        public IDictionary<string?, string?>? Data
        {
            get;
        }
        public string? Dir
        {
            get;
        }
        public string? Draggable
        {
            get;
        }
        public string? Hidden
        {
            get;
        }
        public string? Id
        {
            get;
        }
        public string? Lang
        {
            get;
        }
        public string? Spellcheck
        {
            get;
        }
        public string? Style
        {
            get;
        }
        public string? Tabindex
        {
            get;
        }
        public string? Title
        {
            get;
        }
        public string? Translate
        {
            get;
        }
        public string? Itemprop
        {
            get;
        }
        public string? Itemid
        {
            get;
        }
        public string? Itemref
        {
            get;
        }
        public string? Itemscope
        {
            get;
        }
        public string? Itemtype
        {
            get;
        }
        public string? Nonce
        {
            get;
        }
    }
}
