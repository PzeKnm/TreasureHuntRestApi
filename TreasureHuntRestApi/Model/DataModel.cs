


using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace TreasureHunt.Model
{

  
  

  public class AccessResult
  {
    public bool Success;
    public string GameId;
    public string Token;
  }



  public class UploadStationEventResult
  {
    public bool Success;
    public string Information;

  }

  public class ApiResult
  {
    public bool Success;
    public string Information;

  }

  // Message broadcast to all SignalR clients
  public class ClientMessage
  {
    public string Sender;

    public string StationId;

    public string Direction;
    // Token so that the clients know if the message is for them
    public string AccessToken;
    // The Command
    public string Command;
    // The parameters
    public string Parameters;
  }

}