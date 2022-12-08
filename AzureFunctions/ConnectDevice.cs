using System;
using System.IO;
using System.Threading.Tasks;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions
{
    public static class ConnectDevice
    {
        private static readonly string iothub = "HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=";
        private static readonly RegistryManager _registryManager = RegistryManager.CreateFromConnectionString(iothub);

        [FunctionName("ConnectDevice")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "devices/connect")] HttpRequest req,
            ILogger log)
        {
            try
            {
                var body = JsonConvert.DeserializeObject<DeviceRequest>(await new StreamReader(req.Body).ReadToEndAsync());
                var device = await _registryManager.GetDeviceAsync(body.DeviceId) ?? await _registryManager.AddDeviceAsync(new Device(body.DeviceId));

                var connectionString = $"{iothub.Split(";")[0]};DeviceId={device.Id};SharedAccessKey={device.Authentication.SymmetricKey.PrimaryKey}";

                return new OkObjectResult(connectionString);
            }
            catch
            {
                return new BadRequestResult();
            }
        }
    }
}
