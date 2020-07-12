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
using TreasureHuntRestApi;

// Functions called by game voa REST api
namespace TreasureHunt
{
  // Called to inform the Server of the current state of the game
  public static class UploadStationStatus
  {
    private static ILogger _logger;

    [FunctionName("UploadStationStatus")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [SignalR(HubName = "broadcast")]IAsyncCollector<SignalRMessage> signalRMessages, 
        ILogger log)
    {
      _logger = log;
      log.LogInformation("Performing UploadStationStatus.");
      int nSuppressMessage = await Task.Run(() => {return 99;});

      string HubDeviceId = req.Query["HubDeviceId"];
      string HubDeviceKey = req.Query["HubDeviceKey"];
      string newState = req.Query["Status"]; 

      // All threee parms must be present
      if(HubDeviceId == null || HubDeviceKey == null || newState == null)
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid parameters", 400)); 
      }

      // Check Device Key
      DataAccess da = new DataAccess(_logger);
      string sDeviceKey = da.GetStationHubDeviceKeyFromStationHubId(HubDeviceId); 
      if(!String.Equals(sDeviceKey, HubDeviceKey, StringComparison.OrdinalIgnoreCase) )
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid key", 400));  
      }

      // Check that another station isn't logged on with same ID
      da.UpdateStationStatusFromLastContactTime();

      if(newState == "Activated")
      {
        string sCurrentStatus = da.GetStationStatus(HubDeviceId);
        if( sCurrentStatus != "Inactive" &&
            sCurrentStatus != "Initialised" &&
            sCurrentStatus != "Deactivated")
        {
            return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Station Already Active", 400));  
        }
      }

      if(!GameStateFuncs.IsValidGameState(newState))
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("State not valid: " + newState, 400));
      }

      if (da.UpdateStationStatus(HubDeviceId, newState))
      {
        if(newState == "Online_Ready")
        {
          string gameId = da.GetGameIdFromHubDeviceId(HubDeviceId);
          da.ResetStationToken(gameId);
        }

        UploadStationEventResult arSuccess = new UploadStationEventResult();
        arSuccess.Success = true;
        var wrapped = new Wrapper<UploadStationEventResult>(arSuccess);
        wrapped.StatusCode = 200;
        return new OkObjectResult(wrapped);      
      }   

      return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Status not recognised", 400));   
    }
  }


  /*
  public static class GetStationStatus
  {
    private static ILogger _logger;

    [FunctionName("GetStationStatus")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        [SignalR(HubName = "broadcast")]IAsyncCollector<SignalRMessage> signalRMessages, 
        ILogger log)
    {
      _logger = log;
      log.LogInformation("Performing GetStationStatus.");
      int nSuppressMessage = await Task.Run(() => {return 99;});

      string HubDeviceId = req.Query["HubDeviceId"];
      string HubDeviceKey = req.Query["HubDeviceKey"];
 
      // All threee parms must be present
      if(HubDeviceId == null || HubDeviceKey == null )
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid parameters", 400)); 
      }

      // Check DeviceKey
      DataAccess da = new DataAccess(_logger);
      string sDeviceKey = da.GetStationHubDeviceKeyFromStationHubId(HubDeviceId); 
      if(!String.Equals(sDeviceKey, HubDeviceKey, StringComparison.OrdinalIgnoreCase) )
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid key", 400)); 
      }

      // Check that another station isn't logged on with same ID
      da.UpdateStationStatusFromLastContactTime();
      string sStatus = da.GetStationStatus(HubDeviceId);

      var wrappedObject = new Wrapper<string>(sStatus);
      wrappedObject.StatusCode = 200;
      return new OkObjectResult(wrappedObject);
    }
  }*/



  public static class UploadStationEvent
  {

    private static ILogger _logger;

    [FunctionName("UploadStationEvent")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [SignalR(HubName = "broadcast")]IAsyncCollector<SignalRMessage> signalRMessages, 
        ILogger log)
    {
      _logger = log;
      log.LogInformation("Performing UploadStationEvent.");
      int nSuppressMessage = await Task.Run(() => {return 99;});

      string HubDeviceId = req.Query["HubDeviceId"];
      string HubDeviceKey = req.Query["HubDeviceKey"];
      string Event = req.Query["Event"]; 

      // All threee parms must be present
      if(HubDeviceId == null || HubDeviceKey == null || Event == null)
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid parameters", 400)); 
      }

      // Check access Token
      DataAccess da = new DataAccess(_logger);
      string sDeviceKey = da.GetStationHubDeviceKeyFromStationHubId(HubDeviceId); 
      if(!String.Equals(sDeviceKey, HubDeviceKey, StringComparison.OrdinalIgnoreCase) )
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid key", 400)); 
      }

      if(String.Equals(Event, "Heartbeat"))
      {
        if(da.UpdateStationHeartbeat(HubDeviceId))
        {
          return new OkObjectResult(Wrapper<ApiResult>.GetWrappedSuccess());     
        }              
      }            

      if(String.Equals(Event, "Command"))
      {
        string Command = req.Query["Command"];
        string Parameters = req.Query["Parameters"];

        log.LogInformation("Command: " + Command + "Parms: " + Parameters);
              
        if(Command == null || Parameters == null)
        {
          return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid parameters", 400)); 
        }

        // Get Current AccessToken for Station
        string gameId = da.GetGameIdFromHubDeviceId(HubDeviceId);
        string sToken = da.GetStationToken(gameId);
/* 
        ClientMessage cm = new ClientMessage();
        cm.AccessToken = sToken;
        cm.Command = Command;
        cm.Parameters = Parameters;

        await signalRMessages.AddAsync(new SignalRMessage()
        {
            Target = "notify",
            Arguments = new object[] { cm }
        });*/

        log.LogInformation("SignalRClientComms.PublishMessageToSignalRClients" );
        await SignalRClientComms.PublishMessageToSignalRClients(signalRMessages, 
                gameId, HubDeviceId, "Station2SPA", sToken, Command, Parameters);

        return new OkObjectResult(Wrapper<ApiResult>.GetWrappedSuccess());  
      }



      // ... other event types
      // ...

      return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Event not recognised", 400));
    }


  }

  public static class UploadStationAccessCode
  {

    private static ILogger _logger;

    [FunctionName("UploadStationAccessCode")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [SignalR(HubName = "broadcast")]IAsyncCollector<SignalRMessage> signalRMessages, 
        ILogger log)
    {
      _logger = log;
      log.LogInformation("Performing UploadStationAccessCode.");
      int nSuppressMessage = await Task.Run(() => {return 99;});

      string HubDeviceId = req.Query["HubDeviceId"];
      string HubDeviceKey = req.Query["HubDeviceKey"];
      string AccessCode = req.Query["AccessCode"]; 
      string Timeout = req.Query["Timeout"]; 

      // All three parms must be present
      if(HubDeviceId == null || HubDeviceKey == null || AccessCode == null)
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid parameters", 400)); 
      }

      // Check Station key
      DataAccess da = new DataAccess(_logger);
      string sDeviceKey = da.GetStationHubDeviceKeyFromStationHubId(HubDeviceId); 
      if(!String.Equals(sDeviceKey, HubDeviceKey, StringComparison.OrdinalIgnoreCase) )
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Invalid key", 400)); 
      }

      // Update the code in the DB
      if(da.UpdateStationAccessCode(HubDeviceId, AccessCode))
      {
        string gameId = da.GetGameIdFromHubDeviceId(HubDeviceId);
        string sToken = da.GetStationToken(gameId);
      /* todo does this function work???
        // Notify SPA of timeout
        await SignalRClientComms.PublishMessageToSignalRClients(signalRMessages, 
                gameId, HubDeviceId, "Station2SPA", sToken, "AuthenticationTimeout", Timeout);
                */
        return new OkObjectResult(Wrapper<ApiResult>.GetWrappedSuccess());       
      }   
      else
      {
        return new BadRequestObjectResult(Wrapper<ApiResult>.GetWrappedError("Event not recognised", 400));          
      }     
    }      

  }



}
