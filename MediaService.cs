using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using System.Net.Http;
using MediaModule.Web.Services;
using MediaModule.Web.Domain.Media;
using MediaModule.Web.Cache;
using MediaModule.Web.Core;
using MediaModule.Web.Domain.Account;
using MediaModule.Web.Services.Interfaces;
using System.Threading;
using RestSharp;

namespace MediaModule.Web.Services.Media
{
    public class MediaService : MemoryCacheDefault, IMediaService
    {
        private IConfigService _configService = null;
        private ICacheService _cacheService = null;
        private IAccountSettingsService _accountSettingSrv = null;
        public MediaService(IConfigService configService, ICacheService cacheService, IAccountSettingsService accountSettingSrv)
        {
            _configService = configService;
            _cacheService = cacheService;
            _accountSettingSrv = accountSettingSrv;
        }
        public List<MediaCharacteristics> GetFeed(string handle)
        {
            var key = "insta_" + handle;
            MediaList media = null;

            if (!_cacheService.Contains(key))
            {
                RestClient resClient = new RestClient(_configService.InstaUrl);
                RestRequest request = new RestRequest("users/self/media/recent/", Method.GET);

                string value = _accountSettingSrv.GetValue(handle) ?? _configService.MediaToken();
                request.AddQueryParameter("access_token", value);

                IRestResponse<MediaList> response = resClient.Execute<MediaList>(request);

                media = response.Data;
                if (media != null && media.Data != null)
                {
                    DateTimeOffset expiration = DateTimeOffset.Now.AddMinutes(20);
                    _cacheService.Add(key, media, expiration);
                }
            }
            else
            {
                media = _cacheService.Get<MediaList>(key);
            }
            return media.Data;
        }
    }
  
}