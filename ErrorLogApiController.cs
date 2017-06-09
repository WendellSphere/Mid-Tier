using AppName.Web.Domain.ErrorLogs;
using AppName.Web.Models.Requests.ErrorLogs;
using AppName.Web.Models.Responses;
using AppName.Web.Services;
using AppName.Web.Services.ErrorLogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppName.Web.Controllers.Api
{
    [RoutePrefix("api/errorLogs")]
    public class ErrorLogApiController : ApiController
    {
        IErrorLogService _errorLogService = null;
        IUserService _userService = null;
        public ErrorLogApiController(IErrorLogService errorLogService,IUserService userService)
        {
            _errorLogService = errorLogService;
            _userService = userService;
        }

       



        [Route, HttpPost]
        public HttpResponseMessage Add(ErrorLogAddRequest model)
        {
            if(!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            ItemResponse<int> response = new ItemResponse<int>();
            
            string userId =  _userService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                userId = "18c5d19c-998a-4c5f-be05-1a3fa116d278";
            }
            response.Item = _errorLogService.Insert(model, userId);

            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        //[Route("edit/{id:int}"), HttpPut]
        //HttpResponseMessage Update(ErrorLogUpdateRequest model, int id)
        //{
        //    if(!ModelState.IsValid)
        //    {
        //        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
        //    }
        //    SuccessResponse response = new SuccessResponse();
        //    _errorLogService.Update(model, id);
        //    return Request.CreateResponse(HttpStatusCode.OK, response);
        //}

        [Route("get/{id:int}"), HttpGet]
        public HttpResponseMessage Get(int id)
        {
            ItemResponse<ErrorLog> response = new ItemResponse<ErrorLog>();
            ErrorLog eLog = _errorLogService.Get(id);
            if(eLog.Id == 0)
            {
                string error = "No ErrorLog exists for supplied id";
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
            }
            response.Item = _errorLogService.Get(id);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route, HttpGet]
        public HttpResponseMessage GetAll()
        {
            ItemsResponse<ErrorLog> response = new ItemsResponse<ErrorLog>();
            response.Items = _errorLogService.Get();
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        [Route("{id:int}"), HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            _errorLogService.Delete(id);
            return Request.CreateResponse(HttpStatusCode.OK, new SuccessResponse());
        }
    }
}
