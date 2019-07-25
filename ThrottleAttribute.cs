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

        private int seconds = -1;

        /// <summary>
        /// The number of seconds clients must wait before executing this decorated route again.
        /// </summary>
        public int Seconds
        {
            get { return seconds; }
            set{ seconds = value; }
        }

        /// <summary>
        /// A text message that will be sent to the client upon throttling.  You can include the token {n} to
        /// show this.Seconds in the message, e.g. "Wait {n} seconds before trying again".
        /// </summary>
        public string Message { get; set; }

        //public bool IsControllerThrottle { get { return isControllerThrottle; } set { isControllerThrottle = value; } }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if(DateTime.Now.Hour == Utils.GetConfigValue("HourBatchIsRunning"))
            {
                Execute(actionContext, true);
            }

            SetName(actionContext);

            seconds = Utils.GetConfigValue(name + "-SecondInterval");

            if (seconds <= 0)
            {
                    seconds = Utils.GetConfigValue("Default-SecondInterval");

                    if (seconds <= 0)
                        return;
            }

            ApiRequestBase request = actionContext.ActionArguments["request"] as ApiRequestBase;

            string apiKey = request.sAPIKey;

            string key = string.Concat(name, "-", apiKey);

            ExecuteOrNot(actionContext, apiKey, key);
        }

        private void ExecuteOrNot(HttpActionContext actionContext, string apiKey, string key, bool allowExecute = false)
        {

            if (HttpRuntime.Cache[key] == null)
            {
                HttpRuntime.Cache.Add(key,
                    true,
                    null, // no dependencies
                    DateTime.Now.AddSeconds(Seconds), // absolute expiration
                    Cache.NoSlidingExpiration,
                    CacheItemPriority.Low,
                    null); // no callback

                allowExecute = true;
            }

            if (!allowExecute)
            {
                Execute(actionContext);
            }
        }

        private void Execute(HttpActionContext actionContext, bool isTotalShutdown = false)
        {
            
            if (string.IsNullOrEmpty(Message) && !isTotalShutdown)
            {
                Message = string.Format("You may only perform this action every {0} second(s).", seconds);
            }
            string shutDownMessage = string.Format("API is temporarily shutdown, please try again later");

            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Conflict,
                isTotalShutdown ? shutDownMessage : Message
            );
        }

        private void SetName(HttpActionContext actionContext)
        {
            string endPoint = actionContext.ControllerContext.RouteData.Values["action"].ToString();

            if ((name == null || name != endPoint))
            {
                name = endPoint;
            }
           
        }
    }
}    
