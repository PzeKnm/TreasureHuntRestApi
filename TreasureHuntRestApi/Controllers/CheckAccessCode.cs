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

namespace TreasureHunt
{
  public static class CheckAccessCode
  {
    private static ILogger _logger;

    [FunctionName("CheckAccessCode")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
      _logger = log;
      log.LogInformation("Checking access code.");
      int nSuppressMessage = await Task.Run(() => {return 99;});

      string GameId = req.Query["GameId"];
      string inputCode = req.Query["InputCode"]; 

      if(GameId == null || inputCode == null)
      {
        AccessResult arError = new AccessResult();
        arError.Success = false;
        var wrappedError = new Wrapper<AccessResult>(arError);
        wrappedError.ErrorMessage = "Invalid parameters";
        wrappedError.StatusCode = 400;
        return new BadRequestObjectResult(wrappedError);
      }  

      DataAccess da = new DataAccess(_logger);


      string currentToken = da.GetStationToken(GameId);
      if(currentToken != "")
      {
        AccessResult arError = new AccessResult();
        arError.Success = false;
        var wrappedError = new Wrapper<AccessResult>(arError);
        wrappedError.ErrorMessage = "Station already in use";
        wrappedError.StatusCode = 200;
        return new OkObjectResult(wrappedError);              
      }

      string realCode = da.GetStationAccessCode(GameId);       

      if(realCode != inputCode)
      {
        AccessResult arError = new AccessResult();
        arError.Success = false;
        var wrappedError = new Wrapper<AccessResult>(arError);
        wrappedError.ErrorMessage = "Incorrect input code";
        wrappedError.StatusCode = 200;
        return new OkObjectResult(wrappedError);
      }

      string token = da.GenerateNewStationToken(GameId);  

      // Attach client to station
      string hubDeviceId = da.GetStationHubDeviceId(GameId); 
      await SignalRClientComms.PublishMessageToSignalRClients(signalRMessages, GameId, hubDeviceId, "SPA2Station", token, "AttachClient", "");

      AccessResult ar = new AccessResult();
      ar.Success = true;
      ar.GameId = GameId;
      ar.Token = token;
      var wrapped = new Wrapper<AccessResult>(ar);
      wrapped.StatusCode = 200;
      return new OkObjectResult(wrapped);    
    }
  }
}
