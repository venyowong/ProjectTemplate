using System;
using System.ComponentModel;

namespace ProjectTemplate.Models
{
    public class Feed
    {
        [Description("id")]
        public string Id{get;set;}

        [Description("url")]
        public string Url{get;set;}

        [Description("title")]
        public string Title{get;set;}

        [Description("create_time")]
        public DateTime CreateTime{get;set;}

        [Description("update_time")]
        public DateTime UpdateTime{get;set;}
    }
}