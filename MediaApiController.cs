using MediaModule.Web.Models.Responses;
using MediaModule.Web.Services.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Threading.Tasks;
using MediaModule.Web.Models.Requests;
using MediaModule.Web.Domain.Media;

using InstaSharp;
using MediaModule.Web.Services;
using MediaModule.Web.Services.Interfaces;
using MediaModule.Web.Models.Requests.Account;

using System.Text;
using InstaSharp.Endpoints;

namespace MediaModule.Web.Controllers.Api.Media

{
    [RoutePrefix("Api/Media")]
    [AllowAnonymous]
    public class MediaApiController : ApiController

    {
        private IMediaService _media = null;
        public MediaApiController(IMediaService media)
        {
            _media = media;
        }

        [Route("get/{handle}"), HttpGet]
        public HttpResponseMessage Get(string handle)
        {
            ItemsResponse<MediaCharacteristics> r = new ItemsResponse<MediaCharacteristics>();

            r.Items = _media.GetFeed(handle);

            return Request.CreateResponse(HttpStatusCode.OK, r);
        }

        #region Legacy
        //******************Go to MediaService file 
        //[Route("comments/{mediaId}"), HttpGet]
        //public async Task<HttpResponseMessage> GetComments(string mediaId)
        //{
        //    ItemsResponse<IComment> response = new ItemsResponse<IComment>();

        //    response.Items = await _media.GetComments(mediaId);

        //    return Request.CreateResponse(HttpStatusCode.OK, response);
        //}


        //[Route("{mediaId}"), HttpGet]
        //public async Task<HttpResponseMessage> GetMedia(string mediaId) // gets one picture item by id
        //{

        //    ItemResponse<IMediaCharacteristics> r = new ItemResponse<IMediaCharacteristics>();
        //    r.Item = await _media.GetMedia(mediaId);
        //    return Request.CreateResponse(HttpStatusCode.OK, r);
        //}
        #endregion

    }
}
