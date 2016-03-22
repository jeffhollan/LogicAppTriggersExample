using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace LogicAppTriggers.Controllers
{
    public class PollTriggerController : ApiController
    {
        /// <summary>
        /// Sample polling trigger that will fire whenever the last time it fired was more than 2 minutes ago.
        /// </summary>
        /// <param name="triggerState"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(string triggerState = "")
        {
            //If there is no triggerState - meaning this is the first poll.
            if(String.IsNullOrEmpty(triggerState))
            {
                ///
                /// Generate the triggerState.  This will be the identifier you will use to know which items you have triggered off of.
                /// This is commonly a "timestamp", and you would check on each poll if there are any items available after the timestamp.
                ///
                triggerState = DateTime.UtcNow.ToString();
                return GenerateAsyncResponse(HttpStatusCode.Accepted, triggerState, "15");
            }
            //If there is a triggerState - meaning we have polled before and returned a location header in the if branch above.
            else
            {
                //Do some work to check if a trigger is available
                if(DateTime.Parse(triggerState) < DateTime.UtcNow)
                {
                    //If available, return a 200 - can update the triggerState as well.
                    //In this case I'm going to update the triggerState so that it will fire again in 2 minutes based on this fake logic.
                    triggerState = DateTime.UtcNow.AddMinutes(2).ToString();
                    return GenerateAsyncResponse(HttpStatusCode.OK, triggerState, "15");
                }
                else
                {
                    return GenerateAsyncResponse(HttpStatusCode.Accepted, triggerState, "15");
                }
            }

        }

        private HttpResponseMessage GenerateAsyncResponse(HttpStatusCode code, string triggerState, string retryAfter)
        {
            HttpResponseMessage responseMessage = Request.CreateResponse(code); //Return a 200 to tell it to fire.
            responseMessage.Headers.Add("location", String.Format("{0}://{1}/api/polltrigger?triggerState={2}", Request.RequestUri.Scheme, Request.RequestUri.Host, HttpUtility.UrlEncode(triggerState)));  //Where the engine will poll to check status
            responseMessage.Headers.Add("retry-after", retryAfter);   //How many seconds it should wait.  If multiple files are available you can return a 0 here and the engine will immediately come back and grab other triggers.
            return responseMessage;
        }
    }
}
