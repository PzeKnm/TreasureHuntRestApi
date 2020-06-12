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
  public static class Settings
  {
    private static ILogger _logger;

    [FunctionName("Settings")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages,
        ILogger log)
    {
      _logger = log;
      log.LogInformation("Get Setting.");
      int nSuppressMessage = await Task.Run(() => {return 99;});

      string SettingName = req.Query["SettingName"];


      if(SettingName == null)
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid parameters", 400)); 
      }  

      DataAccess da = new DataAccess(_logger);

      string value = da.GetSetting(SettingName);
      if(value == "")
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("No setting with this name: " + SettingName, 400)); 
      }

      /*
      AccessResult ar = new AccessResult();
      ar.Success = true;
      ar.GameId = GameId;
      ar.Token = token;
      var wrapped = new Wrapper<AccessResult>(ar);
      wrapped.StatusCode = 200;
      return new OkObjectResult(wrapped);    
      */
      var wrappedObject = new Wrapper<string>(value);
      wrappedObject.StatusCode = 200;
      return new OkObjectResult(wrappedObject);
    }
  }
}
