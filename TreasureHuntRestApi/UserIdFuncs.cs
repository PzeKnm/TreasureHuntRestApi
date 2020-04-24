using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TreasureHunt.Data;

namespace TreasureHuntRestApi.Model
{
  public class UserIdFuncs
  {
    public static void UpdateUserId(HttpRequest req, ILogger log)
    {
      if(!req.Headers.ContainsKey("TreasureHuntUserID"))
        return;

      string sUserId = req.Headers["TreasureHuntUserID"].ToString();

      string sUserAgent = "";
      if(req.Headers.ContainsKey("User-Agent"))
        sUserAgent = req.Headers["User-Agent"].ToString();

      string ipAddress = req.HttpContext.Connection.RemoteIpAddress?.ToString();

      DataAccess da = new DataAccess(log);

      da.AddOrUpdateUser(sUserId, sUserAgent, ipAddress);
    }
  }
}
