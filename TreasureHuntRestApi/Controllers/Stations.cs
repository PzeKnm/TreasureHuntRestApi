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
    public static class Stations
    {

      private static ILogger _logger;


        [FunctionName("Stations")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
          UserIdFuncs.UpdateUserId(req, log);

          _logger = log;
          log.LogInformation("Get Stations.");
          int nSuppressMessage = await Task.Run(() => {return 99;});

          DataAccess da = new DataAccess(_logger);

          da.UpdateStationStatusFromLastContactTime();

          List<GameStationDto> lst = da.GetGameStations();   

          var wrappedObject = new Wrapper<IEnumerable<GameStationDto>>(lst);
          wrappedObject.StatusCode = 200;
          return new OkObjectResult(wrappedObject);

            /* 
            var jsonToReturn = JsonConvert.SerializeObject(lst);

            if(jsonToReturn != null)
              return new OkObjectResult(jsonToReturn);

            return new BadRequestObjectResult("Invalid Input parameter");
*/
        }
    }
}
