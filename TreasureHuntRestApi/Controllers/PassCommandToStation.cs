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
using System.Text;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace TreasureHunt
{
    public static class PassCommandToStation
    {

        private static ILogger _logger;

        [FunctionName("PassCommandToStation")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log,
            [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages)
        {
            _logger = log;
            log.LogInformation("Performing PassCommandToStation.");
            int nSuppressMessage = await Task.Run(() => {return 99;});

            string GameId = req.Query["GameId"];
            string AccessToken = req.Query["AccessToken"];
            string cmd = req.Query["Command"]; 
            string parms = req.Query["Parameters"]; 

            // All threee parms must be present
            if(GameId == null || AccessToken == null || cmd == null)
            {
              AccessResult arError = new AccessResult();
              arError.Success = false;
              var wrappedError = new Wrapper<AccessResult>(arError);
              wrappedError.ErrorMessage = "Invalid parameters";
              wrappedError.StatusCode = 400;
              return new BadRequestObjectResult(wrappedError);
            }

            // Check access Token
            DataAccess da = new DataAccess(_logger);
            string sToken = da.GetStationToken(GameId); 
            if(!String.Equals(sToken, AccessToken, StringComparison.OrdinalIgnoreCase) )
            {
              AccessResult arError = new AccessResult();
              arError.Success = false;
              var wrappedError = new Wrapper<AccessResult>(arError);
              wrappedError.ErrorMessage = "Invalid token";
              wrappedError.StatusCode = 400;
              return new BadRequestObjectResult(wrappedError);              
            }

            string hubDeviceId = da.GetStationHubDeviceId(GameId); 
            string hubDeviceKey = da.GetStationHubDeviceKey(GameId);

            if(string.IsNullOrEmpty(hubDeviceId) || string.IsNullOrEmpty(hubDeviceKey))
            {
              AccessResult arError = new AccessResult();
              arError.Success = false;
              var wrappedError = new Wrapper<AccessResult>(arError);
              wrappedError.ErrorMessage = "No hub device for station";
              wrappedError.StatusCode = 400;
              return new BadRequestObjectResult(wrappedError);              
            }

            if(string.IsNullOrEmpty(parms))
              parms = ""; 

            // Cloud2DeviceHelper.SendCommmand(hubDeviceId, hubDeviceKey, cmd, parms);  
            await SignalRClientComms.PublishMessageToSignalRClients(signalRMessages, GameId, hubDeviceId, "SPA2Station", AccessToken, cmd, parms);

            string s = "Command performed OK";
            var wrapped = new Wrapper<string>(s);
            wrapped.StatusCode = 200;
            return new OkObjectResult(wrapped); 
        }


    }
}
