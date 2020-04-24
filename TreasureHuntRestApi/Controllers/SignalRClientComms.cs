using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TreasureHunt.Data;
using TreasureHunt.Model;
using Microsoft.AspNetCore.SignalR.Client;

namespace TreasureHunt
{
    public static class SignalRClientComms
    {
      private static ILogger _logger;


      public static class NegotiateFunction
      {
          [FunctionName("negotiate")]
          public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]HttpRequest req, 
            [SignalRConnectionInfo(HubName = "BroadcastClientMessage")]SignalRConnectionInfo info, 
            ILogger log)
          {
            string sUserId = req.HttpContext.User.Identity.Name;
              log.LogInformation(sUserId);

              return info != null
                  ? (ActionResult)new OkObjectResult(info)
                  : new NotFoundObjectResult("Failed to load SignalR Info.");
          }
      }

      public static class MessageFunction
      {
          [FunctionName("message")]
          public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req, 
                                                      [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages, 
                                                      ILogger log)
          {
            int nSuppressMessage = await Task.Run(() => {return 99;});
              string requestBody = new StreamReader(req.Body).ReadToEnd();

              if (string.IsNullOrEmpty(requestBody))
              {
                  return new BadRequestObjectResult("Please pass a payload to broadcast in the request body.");
              }

              await signalRMessages.AddAsync(new SignalRMessage()
              {
                  Target = "ClientMessage",
                  Arguments = new object[] { requestBody }
              });

              return new OkObjectResult("message called ok");
          }
      }


      public static class TestMessageFunction
      {
          [FunctionName("messagetest")]
          public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequest req, 
                                                      [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages, 
                                                      [SignalRConnectionInfo(HubName = "BroadcastClientMessage")]SignalRConnectionInfo info,
                                                      ILogger log)
          {
            int nSuppressMessage = await Task.Run(() => {return 99;});
            ClientMessage cm = new ClientMessage();
            cm.Sender = "MessageTest";
            cm.StationId = "MyStation";
            cm.Direction = "MyDirection";
            cm.AccessToken = "MyAccessToken";
            cm.Command = "MyCommand";
            cm.Parameters = "MyParameters";

            await SignalRClientComms.PublishMessageToSignalRClients(signalRMessages, cm.Sender, cm.StationId, cm.Direction, 
                cm.AccessToken, cm.Command, cm.Parameters);
 /*
            await signalRMessages.AddAsync(new SignalRMessage()
            {
                Target = "ClientMessage",
                Arguments = new object[] { cm }
            });
*/
            return new OkObjectResult("messagetest called ok");
          }
      }



      public static async Task<bool> PublishMessageToSignalRClients(IAsyncCollector<SignalRMessage> signalRMessages,
        string sender, string stationId, string direction, 
        string accessToken, string cmd, string parms)
      {
        try
        {

          ClientMessage cm = new ClientMessage();
          cm.Sender = sender;
          cm.StationId = stationId;
          cm.Direction = direction;
          cm.AccessToken = accessToken;        
          cm.Command = cmd;
          cm.Parameters = parms;

          await signalRMessages.AddAsync(new SignalRMessage()
          {
              Target = "ClientMessage",
              Arguments = new object[] { cm }
          });
         
          return true;
        }
        catch (Exception ex)
        {
          string s = ex.Message;
          return false;
        } 

      }


      // Sent by a station to be consumed by whatever client might be connected.
      // Station has to supply credentials to guarantee authenticity.
      public static class PublishMessageToClientFunction
      {
          [FunctionName("PublishMessageToClient")]
          public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")]HttpRequest req, 
                                                      [SignalR(HubName = "BroadcastClientMessage")]IAsyncCollector<SignalRMessage> signalRMessages, 
                                                      ILogger log)
          {
            int nSuppressMessage = await Task.Run(() => {return 99;});
            _logger = log;

            // Credentials to assure who is sending and that they are the real device
            string HubDeviceId = req.Query["HubDeviceId"];
            string HubDeviceKey = req.Query["HubDeviceKey"];

            // The command to be published.
            string Command = req.Query["Command"];
            string Parameters = req.Query["Parameters"];

            // All parms must be present
            if(HubDeviceId == null || HubDeviceKey == null || 
               Command == null || Parameters == null)
            {
              var wrappedError = new Wrapper<string>("");
              wrappedError.ErrorMessage = "Invalid parameters";
              wrappedError.StatusCode = 400;
              return new BadRequestObjectResult(wrappedError);
            }

            // Check station credentials
            DataAccess da = new DataAccess(_logger);
            string sDeviceKey = da.GetStationHubDeviceKeyFromStationHubId(HubDeviceId); 
            if(!String.Equals(sDeviceKey, HubDeviceKey, StringComparison.OrdinalIgnoreCase) )
            {
              var wrappedError = new Wrapper<string>("");
              wrappedError.ErrorMessage = "Invalid key";
              wrappedError.StatusCode = 400;
              return new BadRequestObjectResult(wrappedError);              
            }

            // Publish message

            // Get Current AccessToken for Station
            string gameId = da.GetGameIdFromHubDeviceId(HubDeviceId);
            string sToken = da.GetStationToken(gameId);

            ClientMessage cm = new ClientMessage();
            cm.Sender = HubDeviceId;
            cm.StationId = HubDeviceId;
            cm.Direction = "Station2SPA";
            cm.AccessToken = sToken;
            cm.Command = Command;
            cm.Parameters = Parameters;

            await signalRMessages.AddAsync(new SignalRMessage()
            {
                Target = "ClientMessage",
                Arguments = new object[] { cm }
            });

            return new OkObjectResult("PublishMessageToClient called ok");
          }
      }


    }
}

