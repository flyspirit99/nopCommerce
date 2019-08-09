using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Base;

namespace Nop.Plugin.Api.DTO
{
    public class AddressResponseDto : ResponseBaseDto<List<AddressDto>>
    {
        public AddressResponseDto()
        {
            Status = 0;
            Message = "";
            Data = new List<AddressDto>();
        }
    }
}
