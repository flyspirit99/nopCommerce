using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.DTO
{
    public class BannerDto
    {
        [JsonProperty("attach")]
        public string PictureUrl { get; set; }

        [JsonProperty("title")]
        public string Text { get; set; }

        [JsonProperty("content")]
        public string Link { get; set; }

        [JsonIgnore]
        public string AltText { get; set; }
    }
}
