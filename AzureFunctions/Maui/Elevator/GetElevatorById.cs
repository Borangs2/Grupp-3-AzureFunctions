using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureFunctions.Helpers;
using AzureFunctions.Models;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;

namespace AzureFunctions.Maui.Elevator
{
    public static class GetElevatorById
    {

        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=");


        [FunctionName("GetElevatorById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "elevator")] HttpRequest req,
            ILogger log)
        {
            var data = await new StreamReader(req.Body).ReadToEndAsync();
            if (data == string.Empty)
            {
                data = req.Query["id"];
            }

            if (!Guid.TryParse(data, out var elevatorId))
                return new BadRequestResult();


            //Gets All Elevator properties
            var elevator = await ElevatorHelper.GetElevatorDeviceAsync<ElevatorDetailedModel>(data);


            elevator.Errands = new List<ErrandModel>();

            using IDbConnection connection = new SqlConnection(DbConnectionString);

            //Gets all errand properties
            elevator.ConnectionString = await connection.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM Elevators WHERE Id = @Id", new { Id = data });
            var errandResult = await connection.QueryAsync("SELECT * FROM Errands WHERE ElevatorModelId = @ElevatorId", new { ElevatorId = data });

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

            return new OkObjectResult(elevator);
        }
    }
}
