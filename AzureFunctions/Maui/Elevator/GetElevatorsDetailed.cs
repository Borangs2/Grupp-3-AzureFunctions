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

                    //Gets all errand properties
                    elevator.ConnectionString =
                        await connection.QueryFirstOrDefaultAsync<string>(
                            "SELECT ConnectionString FROM Elevators");
                    var errandResult = await connection.QueryAsync(
                        "SELECT * FROM Errands WHERE ElevatorModelId = @ElevatorId", new {ElevatorId = elevator.Id.ToString()});

                    foreach (var errand in errandResult.ToList())
                    {
                        var addErrand = new ErrandModel()
                        {
                            Id = errand.Id,
                            Title = errand.Title,
                            Description = errand.Description,
                            CreatedAt = errand.CreatedAt,
                            CreatedBy = errand.CreatedBy,
                            LastEdited = errand.LastEdited,
                            Status = Enum.Parse<ErrandStatus>(errand.Status),
                            Comments = new List<ErrandCommentModel>()
                        };

                        //Gets the technician
                        addErrand.Technician = await TechnicianHelper.GetTechnicianAsync(errand.Id.ToString(), connection);

                        //Gets all comments
                        errand.Comments = await CommentHelper.GetErrandCommentsAsync(errand.Id.ToString(), connection);

                        elevator.Errands.Add(addErrand);
                    }
                    elevatorList.Add(elevator);
                }
            }

            return new OkObjectResult(elevatorList);
        }
    }
}
