using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SandBox.Util
{

public class ThrottleAttribute : ActionFilterAttribute
    {
        private string name = null;

        private int miliSeconds = -1;

        private readonly CustomLogger _logger = (CustomLogger)LogManager.GetCurrentClassLogger(typeof(CustomLogger));

        /// <summary>
        /// The number of seconds clients must wait before executing this decorated route again.
        /// </summary>
        public int MiliSeconds
        {
            get { return miliSeconds; }
            set{ miliSeconds = value; }
        }

        /// <summary>
        /// A text message that will be sent to the client upon throttling.  You can include the token {n} to
        /// show this.Seconds in the message, e.g. "Wait {n} seconds before trying again".
        /// </summary>
        public string Message { get; set; }

        //public bool IsControllerThrottle { get { return isControllerThrottle; } set { isControllerThrottle = value; } }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {

            ApiRequestBase request = actionContext.ActionArguments["request"] as ApiRequestBase;

            string apiKey = request.sAPIKey;

            SetName(actionContext);

            string key = string.Concat(name, "-", apiKey);

            if (DateTime.Now.Hour == Utils.GetConfigValue("HourBatchIsRunning"))
            {
                Execute(actionContext, request, true);
            }

            miliSeconds = Utils.GetConfigValue(name + "-Interval");

            if (miliSeconds <= 0)
            {
                miliSeconds = Utils.GetConfigValue("Default-Interval");

                    if (miliSeconds <= 0)
                        return;
            }

            ExecuteOrNot(actionContext, request, apiKey, key);
        }

        #region Helper Methods

        private void ExecuteOrNot(HttpActionContext actionContext, ApiRequestBase request, string apiKey, string key, bool allowExecute = false)
        {

            if (HttpRuntime.Cache[key] == null)
            {
                HttpRuntime.Cache.Add(key,
                    true,
                    null, // no dependencies
                    DateTime.Now.AddMilliseconds(miliSeconds), // absolute expiration
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Low,
                    null); // no callback

                allowExecute = true;
            }

            if (!allowExecute)
            {
                Execute(actionContext, request);
            }
        }

        private void Execute(HttpActionContext actionContext, ApiRequestBase request, bool isTotalShutdown = false)
        {

            if (string.IsNullOrEmpty(Message) && !isTotalShutdown)
            {
                Message = string.Format("You may only perform this action every {0} milisecond(s).", miliSeconds);
            }
            string shutDownMessage = string.Format("API is temporarily shutdown, please try again later");

            LogThrottle(request, isTotalShutdown, shutDownMessage);

            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Conflict,
                isTotalShutdown ? shutDownMessage : Message
            );
        }

        private void LogThrottle(ApiRequestBase request, bool isTotalShutdown, string shutDownMessage)
        {
            int auditId = 0;

            auditId = this.InsertAudit(request.sAPIKey, request.iTradingPartnerID, request.sEmailAddress, name, auditId);

            _logger.SetAuditId(auditId);

            _logger.Error(name + " - " + (isTotalShutdown ? shutDownMessage : Message));
        }

        private void SetName(HttpActionContext actionContext)
        {
            string endPoint = actionContext.ControllerContext.RouteData.Values["action"].ToString();

            if ((name == null || name != endPoint))
            {
                name = endPoint;
            }
           
        }

        #endregion
    }
}    
