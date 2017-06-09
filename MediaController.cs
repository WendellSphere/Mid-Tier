using InstaSharp;
using MediaModule.Models.Requests.Account;
using MediaModule.Models.Services;
using MediaModule.Services.Interfaces;
using MediaModule.Services.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MediaMoudle.Web.Controllers
{
    [RoutePrefix("Media")]
    public class MediaController : SiteController
    {
        private IUserService _userService = null;
        private IAccountSettingsService _accountSettingsSrv = null;
        private IConfigService _configService = null;
        private IMediaService _media = null;

        public MediaController(IMediaService media, IConfigService configService, IAccountSettingsService accountSettingsSrv, IUserService userService)
        {
            _media = media;
            _configService = configService;
            _accountSettingsSrv = accountSettingsSrv;
            _userService = userService;
        }

        [Route("login")]
        public ActionResult Login()
        {
            var scopes = new List<OAuth.Scope>();
            scopes.Add(InstaSharp.OAuth.Scope.Likes);
            scopes.Add(InstaSharp.OAuth.Scope.Comments);
            scopes.Add(InstaSharp.OAuth.Scope.Public_Content);
            scopes.Add(InstaSharp.OAuth.Scope.Likes);

            var link = InstaSharp.OAuth.AuthLink(_configService.GetMediaConfig.OAuthUri + "authorize", _configService.GetMediaConfig.ClientId, _configService.GetMediaConfig.RedirectUri, scopes, InstaSharp.OAuth.ResponseType.Code);

            return Redirect(link);
        }

        [Route("oauth")]
        public async Task<ActionResult> OAuth(string code)
        {
            var auth = new OAuth(_configService.GetMediaConfig);

            var oauthResponse = await auth.RequestToken(code);

            AccountSettingRequest asr = new AccountSettingRequest();

            asr.UserId = _userService.GetCurrentUserId();
            asr.Value = oauthResponse.AccessToken;
            asr.SettingId = Enums.AccountSettings.InstagramToken;

            _accountSettingsSrv.Insert(asr);

            return Redirect("/account#!/settings");
        }
    }
}
