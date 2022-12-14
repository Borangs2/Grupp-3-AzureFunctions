using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using System.Collections.Generic;
using AzureFunctions.Helpers;
using AzureFunctions.Models;

namespace AzureFunctions.Maui.Elevator
{
    public static class GetElevators
    {
        private static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=");

        [FunctionName("GetElevators")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "elevator/all")] HttpRequest req,
            ILogger log)
        {
            var elevatorList = new List<ElevatorDeviceItem>();
            var result = _registryManager.CreateQuery($"SELECT * FROM devices");

            if (result.HasMoreResults)
            {
                foreach (var twin in await result.GetNextAsTwinAsync())
                {
                    elevatorList.Add(await ElevatorHelper.GetElevatorDeviceAsync<ElevatorDeviceItem>(twin.DeviceId));
                }
            }

            return new OkObjectResult(elevatorList);
        }
    }
}
