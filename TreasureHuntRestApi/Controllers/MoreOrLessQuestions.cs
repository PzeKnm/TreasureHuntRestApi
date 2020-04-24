using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TreasureHunt.Data;
using TreasureHunt.Model;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Collections.Generic;

namespace TreasureHunt
{
    public static class MoreOrLessQuestions
    {

        private static ILogger _logger;

        [FunctionName("MoreOrLessGetQuestion")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages,
            ILogger log)
        {
          _logger = log;
            log.LogInformation("MoreOrLessGetQuestion.");
            int nSuppressMessage = await Task.Run(() => {return 99;});

            
            string HubDeviceId = req.Query["HubDeviceId"];
            string HubDeviceKey = req.Query["HubDeviceKey"];
            string ExcludeKeys = req.Query["ExcludeKeys"];

            // All parms must be present
            if(HubDeviceId == null || HubDeviceKey == null)
            {
              return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid parameters", 400)); 
            }

            DataAccess da = new DataAccess(_logger);

             // Check Station key
            string sDeviceKey = da.GetStationHubDeviceKeyFromStationHubId(HubDeviceId); 
            if(!String.Equals(sDeviceKey, HubDeviceKey, StringComparison.OrdinalIgnoreCase) )
            {
              return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid key", 400)); 
            }

            List<int> lstExclude = new List<int>();
            if(ExcludeKeys != null)
            {
              string[] sArray = ExcludeKeys.Split(',');
              foreach(string s in sArray)
              {
                int n;
                if(Int32.TryParse(s, out n))
                lstExclude.Add(n);

              }
            }

            MoreOrLessQuestion q = da.GetMoreOrLessQuestion(lstExclude);      
      
            var wrappedObject = new Wrapper<MoreOrLessQuestion>(q);
            wrappedObject.StatusCode = 200;
            return new OkObjectResult(wrappedObject);
        }
    }
}
