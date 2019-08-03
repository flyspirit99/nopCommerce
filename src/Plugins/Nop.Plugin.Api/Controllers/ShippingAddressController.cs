using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Api.Services;
using Nop.Plugin.Api.SimpleDTO;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Api.ModelBinders;
using Nop.Services.Common;

namespace Nop.Plugin.Api.Controllers
{
    public class ShippingAddressController : BaseApiController
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly IAddressService _addressService;

        public ShippingAddressController(
                ICustomerApiService customerApiService,
                IJsonFieldsSerializer jsonFieldsSerializer,
                IAclService aclService,
                ICustomerService customerService,
                IStoreMappingService storeMappingService,
                IStoreService storeService,
                IDiscountService discountService,
                ICustomerActivityService customerActivityService,
                ILocalizationService localizationService,
                IPictureService pictureService,
                IAddressService addressService
            ) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _customerApiService = customerApiService;
            _addressService = addressService;
        }

        [HttpGet]
        [Route("/api/address/{customerId}")]
        [ProducesResponseType(typeof(SimpleAddressDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetAddressesByCustomerId(int customerId, string fields = "")
        {
            if (customerId <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "customerId", "invalid customer id");
            }

            var customer = _customerApiService.GetCustomerById(customerId);

            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            var addressDto = new SimpleAddressDto
            {
                Data = customer.Addresses.Select(x => x.City + x.Address1 + x.Address2).ToList()
            };

            var json = JsonFieldsSerializer.Serialize(addressDto, fields);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/address/setDefault")]
        [ProducesResponseType(typeof(SimpleBaseDto<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult SetShippingAddress(
            //[ModelBinder(typeof(JsonModelBinder<SetDefaultAddressRequest>))]
            [FromBody]CustomerAddressDto requestDto)
        {
            if (requestDto.CustomerId <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "customerId", "invalid customer id");
            }

            if (requestDto.AddressId <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "addressId", "invalid address id");
            }

            var customer = _customerApiService.GetCustomerEntityById(requestDto.CustomerId);
            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            if (customer.ShippingAddress == null || customer.ShippingAddress.Id != requestDto.AddressId)
            {
                // 和旧地址不同才修改
                var address = customer.Addresses.Where(x => x.Id == requestDto.AddressId).SingleOrDefault();
                if(address == null)
                {
                    return Error(HttpStatusCode.NotFound, "address", "not found");
                }

                customer.ShippingAddress = address;

                CustomerService.UpdateCustomer(customer);
            }
            var resultDto = new SimpleBaseDto<string>
            {
                Data = "Success"
            };

            var json = JsonFieldsSerializer.Serialize(resultDto, "");

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/address")]
        [ProducesResponseType(typeof(SimpleBaseDto<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult DeleteAddress(
            //[ModelBinder(typeof(JsonModelBinder<SetDefaultAddressRequest>))]
            [FromBody]CustomerAddressDto request)
        {
            if (request.CustomerId <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "customerId", "invalid customer id");
            }

            if (request.AddressId <= 0)
            {
                return Error(HttpStatusCode.BadRequest, "addressId", "invalid address id");
            }

            var customer = _customerApiService.GetCustomerEntityById(request.CustomerId);
            if (customer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            //try to get an address with the specified id
            var address = customer.Addresses.FirstOrDefault(a => a.Id == request.AddressId);
            if (address == null)
                return Error(HttpStatusCode.BadRequest, "addressId", "invalid address id");

            CustomerService.RemoveCustomerAddress(customer, address);
            CustomerService.UpdateCustomer(customer);

            //now delete the address record
            _addressService.DeleteAddress(address);


            var resultDto = new SimpleBaseDto<string>
            {
                Data = "Success"
            };

            var json = JsonFieldsSerializer.Serialize(resultDto, "");

            return new RawJsonActionResult(json);
        }
    }
}
