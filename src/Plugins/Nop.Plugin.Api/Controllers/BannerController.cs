using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Api.Attributes;
using Nop.Plugin.Api.DTO;
using Nop.Plugin.Api.DTO.Base;
using Nop.Plugin.Api.DTO.Errors;
using Nop.Plugin.Api.JSON.ActionResults;
using Nop.Plugin.Api.JSON.Serializers;
using Nop.Plugin.Widgets.NivoSlider;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace Nop.Plugin.Api.Controllers
{
    public class BannerController : BaseApiController
    {
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        public BannerController(IStoreContext storeContext,
                IStaticCacheManager cacheManager,
                ISettingService settingService,
                IPictureService pictureService,
                IWebHelper webHelper,
                IJsonFieldsSerializer jsonFieldsSerializer,
                IAclService aclService,
                ICustomerService customerService,
                IStoreMappingService storeMappingService,
                IStoreService storeService,
                IDiscountService discountService,
                ICustomerActivityService customerActivityService,
                ILocalizationService localizationService
            ) :
            base(jsonFieldsSerializer, aclService, customerService, storeMappingService, storeService, discountService, customerActivityService,
                 localizationService, pictureService)
        {
            _storeContext = storeContext;
            _cacheManager = cacheManager;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        [HttpGet]
        [Route("/api/banner")]
        [ProducesResponseType(typeof(ResponseBaseDto<IList<BannerDto>>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorsRootObject), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.Unauthorized)]
        [GetRequestsErrorInterceptorActionFilter]
        public IActionResult GetBanners()
        {
            var nivoSliderSettings = _settingService.LoadSetting<NivoSliderSettings>(_storeContext.CurrentStore.Id);

            var response = new ResponseBaseDto<IList<BannerDto>>();

            response.Data = new List<BannerDto>();

            
            response.Data.Add(new BannerDto()
            {
                PictureUrl = PictureService.GetPictureUrl(nivoSliderSettings.Picture1Id, showDefaultPicture: false) ?? "",
                Text = nivoSliderSettings.Text1,
                Link = nivoSliderSettings.Link1,
                AltText = nivoSliderSettings.AltText1,
            });

            response.Data.Add(new BannerDto()
            {
                PictureUrl = PictureService.GetPictureUrl(nivoSliderSettings.Picture2Id, showDefaultPicture: false) ?? "",
                Text = nivoSliderSettings.Text2,
                Link = nivoSliderSettings.Link2,
                AltText = nivoSliderSettings.AltText2,
            });

            response.Data.Add(new BannerDto()
            {
                PictureUrl = PictureService.GetPictureUrl(nivoSliderSettings.Picture3Id, showDefaultPicture: false) ?? "",
                Text = nivoSliderSettings.Text3,
                Link = nivoSliderSettings.Link3,
                AltText = nivoSliderSettings.AltText3,
            });

            response.Data.Add(new BannerDto()
            {
                PictureUrl = PictureService.GetPictureUrl(nivoSliderSettings.Picture4Id, showDefaultPicture: false) ?? "",
                Text = nivoSliderSettings.Text4,
                Link = nivoSliderSettings.Link4,
                AltText = nivoSliderSettings.AltText4,
            });

            response.Data.Add(new BannerDto()
            {
                PictureUrl = PictureService.GetPictureUrl(nivoSliderSettings.Picture5Id, showDefaultPicture: false) ?? "",
                Text = nivoSliderSettings.Text5,
                Link = nivoSliderSettings.Link5,
                AltText = nivoSliderSettings.AltText5,
            });

            response.Data = response.Data.Where(x => x.PictureUrl != null && x.PictureUrl != "").ToList();

            var json = JsonFieldsSerializer.Serialize(response, "");

            return new RawJsonActionResult(json);
        }

    }
}
