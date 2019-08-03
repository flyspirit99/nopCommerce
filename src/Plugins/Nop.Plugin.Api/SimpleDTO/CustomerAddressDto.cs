using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.SimpleDTO
{
    public class CustomerAddressDto
    {
        [JsonProperty("customerId")]
        public int CustomerId { get; set; }

        [JsonProperty("addressId")]
        public int AddressId { get; set; }
    }
}
