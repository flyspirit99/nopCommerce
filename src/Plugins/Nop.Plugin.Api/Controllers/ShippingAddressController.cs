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
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Plugin.Api.ModelBinders;
using Nop.Services.Common;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.Delta;
using Nop.Plugin.Api.DTO.Customers;
using Nop.Plugin.Api.MappingExtensions;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Api.Helpers;
using Nop.Services.Directory;

namespace Nop.Plugin.Api.Controllers
{
    public class ShippingAddressController : BaseApiController
    {
        private readonly ICustomerApiService _customerApiService;
        private readonly IAddressService _addressService;
        private readonly IMappingHelper _mappingHelper;
        private readonly ICountryService _countryService;

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
                IAddressService addressService,
                IMappingHelper mappingHelper,
                ICountryService countryService
            ) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _customerApiService = customerApiService;
            _addressService = addressService;
            _mappingHelper = mappingHelper;
            _countryService = countryService;
        }

        [HttpGet]
        [Route("/api/address/{customerId}")]
        [ProducesResponseType(typeof(AddressResponseDto), (int)HttpStatusCode.OK)]
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

            var addressDto = new AddressResponseDto
            {
                Data = customer.Addresses.ToList()
            };

            var json = JsonFieldsSerializer.Serialize(addressDto, fields);

            return new RawJsonActionResult(json);
        }

        [HttpPut]
        [Route("/api/address/setDefault")]
        [ProducesResponseType(typeof(ResponseBaseDto<string>), (int)HttpStatusCode.OK)]
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
            var resultDto = new ResponseBaseDto<string>
            {
                Data = "Success"
            };

            var json = JsonFieldsSerializer.Serialize(resultDto, "");

            return new RawJsonActionResult(json);
        }

        [HttpDelete]
        [Route("/api/address")]
        [ProducesResponseType(typeof(ResponseBaseDto<string>), (int)HttpStatusCode.OK)]
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


            var resultDto = new ResponseBaseDto<string>
            {
                Data = "Success"
            };

            var json = JsonFieldsSerializer.Serialize(resultDto, "");

            return new RawJsonActionResult(json);
        }


        [HttpPut]
        [Route("/api/address/{id}")]
        [ProducesResponseType(typeof(ResponseBaseDto<int?>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), 422)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        public IActionResult UpdateAddress(
            [ModelBinder(typeof(JsonModelBinder<CustomerDto>))]
            Delta<CustomerDto> customerDelta)
        {
            // Here we display the errors if the validation has failed at some point.
            if (!ModelState.IsValid)
            {
                return Error();
            }

            if (customerDelta.Dto.Addresses.Count == 0)
            {
                return Error(HttpStatusCode.BadRequest, "address", "invalid address paramters");
            }

                // Updateting the customer
            var currentCustomer = _customerApiService.GetCustomerEntityById(customerDelta.Dto.Id);

            if (currentCustomer == null)
            {
                return Error(HttpStatusCode.NotFound, "customer", "not found");
            }

            var oldAddressList = currentCustomer.Addresses.Select(x => x.Id).ToList();

            // customerDelta.Merge(currentCustomer);

            //if (customerDelta.Dto.RoleIds.Count > 0)
            //{
            //    AddValidRoles(customerDelta, currentCustomer);
            //}

            //if (customerDelta.Dto.Addresses.Count > 0)
            //{
                var currentCustomerAddresses = currentCustomer.Addresses.ToDictionary(address => address.Id, address => address);

                foreach (var passedAddress in customerDelta.Dto.Addresses)
                {
                    var addressEntity = passedAddress.ToEntity();

                    if (currentCustomerAddresses.ContainsKey(passedAddress.Id))
                    {
                        _mappingHelper.Merge(passedAddress, currentCustomerAddresses[passedAddress.Id]);
                    }
                    else
                    {
                        currentCustomer.Addresses.Add(addressEntity);
                        //some validation
                        if (addressEntity.CountryId == 0)
                            addressEntity.CountryId = null;
                        if (addressEntity.StateProvinceId == 0)
                            addressEntity.StateProvinceId = null;

                        currentCustomer.CustomerAddressMappings.Add(new CustomerAddressMapping { Address = addressEntity });
                    }
                }
            //}

            CustomerService.UpdateCustomer(currentCustomer);

            // TODO: Localization

            // Preparing the result dto of the new customer
            // We do not prepare the shopping cart items because we have a separate endpoint for them.
            var updatedCustomer = currentCustomer.ToDto();

            // This is needed because the entity framework won't populate the navigation properties automatically
            // and the country name will be left empty because the mapping depends on the navigation property
            // so we do it by hand here.
            PopulateAddressCountryNames(updatedCustomer);

            //activity log
            CustomerActivityService.InsertActivity("UpdateCustomer", LocalizationService.GetResource("ActivityLog.UpdateCustomer"), currentCustomer);

            var newAddressList = updatedCustomer.Addresses.Select(x => x.Id).ToList();

            var newId = newAddressList.Except(oldAddressList).FirstOrDefault();

            var response = new ResponseBaseDto<int>()
            {
                Data = newId
            };

            var json = JsonFieldsSerializer.Serialize(response, string.Empty);

            return new RawJsonActionResult(json);
        }

        private void PopulateAddressCountryNames(CustomerDto newCustomerDto)
        {
            foreach (var address in newCustomerDto.Addresses)
            {
                SetCountryName(address);
            }

            if (newCustomerDto.BillingAddress != null)
            {
                SetCountryName(newCustomerDto.BillingAddress);
            }

            if (newCustomerDto.ShippingAddress != null)
            {
                SetCountryName(newCustomerDto.ShippingAddress);
            }
        }

        private void SetCountryName(AddressDto address)
        {
            if (string.IsNullOrEmpty(address.CountryName) && address.CountryId.HasValue)
            {
                var country = _countryService.GetCountryById(address.CountryId.Value);
                address.CountryName = country.Name;
            }
        }
    }
}
