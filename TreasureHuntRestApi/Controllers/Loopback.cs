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

namespace TreasureHunt
{
    public static class Loopback
    {

      private static ILogger _logger;


        [FunctionName("Loopback")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            _logger = log;
            log.LogInformation("Loopback.");
            int nSuppressMessage = await Task.Run(() => {return 99;});

            String sReply = "Loopback " + DateTime.Now.ToLongTimeString();              

            var wrappedObject = new Wrapper<string>(sReply);
            wrappedObject.StatusCode = 200;
            return new OkObjectResult(wrappedObject);
        }
    }
}
