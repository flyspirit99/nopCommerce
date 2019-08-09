using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;

namespace Nop.Plugin.Api.DTO.Base
{
    public class ResponseBaseDto : ISerializableObject
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }


        public string GetPrimaryPropertyName()
        {
            return "";
        }

        public Type GetPrimaryPropertyType()
        {
            return null;
        }
    }

    public class ResponseBaseDto<T> : ISerializableObject
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }

        public string GetPrimaryPropertyName()
        {
            return "data";
        }

        public Type GetPrimaryPropertyType()
        {
            return typeof(T);
        }
    }
}
