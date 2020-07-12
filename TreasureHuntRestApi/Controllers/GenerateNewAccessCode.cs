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
using TreasureHuntRestApi.Model;

namespace TreasureHunt
{
  public static class GenerateNewAccessCode
  {

      private static ILogger _logger;

      [FunctionName("GenerateNewAccessCode")]
      public static async Task<IActionResult> Run(
          [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
          [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages,
          ILogger log)
      {
          _logger = log;
          log.LogInformation("Performing GenerateNewAccessCode.");
               
          int nSuppressMessage = await Task.Run(() => {return 99;});

          // TStation.Id
          string GameId = req.Query["GameId"];
            
          if(GameId == null)
          {
            AccessResult arError = new AccessResult();
            arError.Success = false;
            var wrappedError = new Wrapper<AccessResult>(arError);
            wrappedError.ErrorMessage = "Invalid parameters";
            wrappedError.StatusCode = 400;
            return new BadRequestObjectResult(wrappedError);
          }

          DataAccess da = new DataAccess(_logger);
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

          // Test if station is already either occupied or under "Waiting For Authorisation"
          string status = da.GetStationStatus(hubDeviceId);
          if(status != "Online_Ready" && status != "Online_Demo" && status != "Online_Dormant")
          {
            AccessResult arError = new AccessResult();
            arError.Success = false;
            var wrappedError = new Wrapper<AccessResult>(arError);

            wrappedError.ErrorMessage = "Station not in online status, current status is: " + status;

            if(status == "Authenticating")
              wrappedError.ErrorMessage = "Authenticating. Station busy with other client";

            wrappedError.StatusCode = StatusCodes.Status409Conflict;
            return new BadRequestObjectResult(wrappedError);               
          }

          await SignalRClientComms.PublishMessageToSignalRClients(signalRMessages, GameId, hubDeviceId, 
                "SPA2Station", "", "GenerateAccessCode", "");

          string s = "Command performed OK";
          var wrapped = new Wrapper<string>(s);
          wrapped.StatusCode = 200;
          return new OkObjectResult(wrapped); 
      }

  }
}
