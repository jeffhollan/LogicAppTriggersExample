using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace LogicAppTriggers.Controllers
{
    public class WebhookTriggerController : ApiController
    {
        public List<string> subscriptions = new List<string>();
        /// <summary>
        /// Recieve a subscription to a webhook.  In this case I'm just going to wait for 2 minutes and then fire a trigger to the callback URL.
        /// You could also have a dictionary of callback URLs and call all when certain events happen.
        /// </summary>
        /// <param name="callbackUrl">URL to get from Logic Apps - @listCallbackUrl()</param>
        /// <returns></returns>
        [HttpPost, Route("api/webhooktrigger/subscribe")]
        public HttpResponseMessage Subscribe([FromBody] string callbackUrl)
        {
            subscriptions.Add(callbackUrl);
            new Thread(() => doWork()).Start();   //Start the thread of work, but continue on before it completes
            return Request.CreateResponse();
        }

        /// <summary>
        /// Some background thread monitoring when alerts should occur.  
        /// </summary>
        /// <param name="id"></param>
        private async void doWork()
        {
            Task.Delay(12000).Wait(); //Do work will work for 120 seconds)
            using (HttpClient client = new HttpClient())
            {
                foreach(string callbackUrl in subscriptions)
                await client.PostAsync<string>(callbackUrl, @"{""Trigger"": ""Fired""}", new JsonMediaTypeFormatter(), "application/json");
            }
        }

        /// <summary>
        /// Unsubscribe
        /// </summary>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        [HttpPost, Route("api/webhooktrigger/unsubscribe")]
        public HttpResponseMessage Unsubscribe([FromBody] string callbackUrl)
        {
            subscriptions.Remove(callbackUrl);
            return Request.CreateResponse();
        }
    }
}
