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
using AzureFunctions.Models;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using Dapper;
using System.Reflection.Metadata;
using AzureFunctions.Helpers;
using AzureFunctions.Models.Interfaces;

namespace AzureFunctions.Maui.Elevator
{
    public static class GetElevatorsDetailed
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=");

        [FunctionName("GetElevatorsDetailed")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "elevator/all/detailed")] HttpRequest req,
            ILogger log)
        {
            var elevatorList = new List<ElevatorDetailedModel>();
            var result = _registryManager.CreateQuery($"SELECT * FROM devices");

            if (result.HasMoreResults)
            {
                foreach (var twin in await result.GetNextAsTwinAsync())
                {
                    var elevator = await ElevatorHelper.GetElevatorDeviceAsync<ElevatorDetailedModel>(twin.DeviceId);

                    using IDbConnection connection = new SqlConnection(DbConnectionString);
                    elevator.ConnectionString =
                        await connection.QueryFirstOrDefaultAsync<string>(
                            "SELECT ConnectionString FROM Elevators");

                    elevator.Errands = await ErrandHelper.GetElevatorErrands(elevator.Id.ToString(), connection);

                    elevatorList.Add(elevator);
                }
            }

            return new OkObjectResult(elevatorList);
        }
    }
}
