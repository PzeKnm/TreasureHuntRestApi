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
using System.Collections.Generic;
using TreasureHunt.Model;
using TreasureHuntRestApi.Model;

namespace TreasureHunt
{
  public static class Users
  {

    [FunctionName("Users")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("Get Users.");
      await Task.Run(() => {UserIdFuncs.UpdateUserId(req, log);});
          
    //   int nSuppressMessage = await Task.Run(() => {return 99;});

      DataAccess da = new DataAccess(log);          
      List<UserDto> lst = da.GetUsers();   

      var wrappedObject = new Wrapper<IEnumerable<UserDto>>(lst);
      wrappedObject.StatusCode = 200;
      return new OkObjectResult(wrappedObject);
    }
  }
}
