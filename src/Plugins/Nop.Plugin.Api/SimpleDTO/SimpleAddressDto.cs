using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Nop.Plugin.Api.SimpleDTO
{
    public class SimpleAddressDto : SimpleBaseDto<List<string>>
    {
        public SimpleAddressDto()
        {
            Status = 0;
            Message = "";
            Data = new List<string>();
        }
    }
}
