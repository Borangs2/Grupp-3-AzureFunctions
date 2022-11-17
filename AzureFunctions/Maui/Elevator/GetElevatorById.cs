using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Devices;
using Grupp3_Elevator.Models;
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
            var data = JsonConvert.DeserializeObject<string?>(await new StreamReader(req.Body).ReadToEndAsync());


            if (!Guid.TryParse(data, out var elevatorId))
                return new BadRequestResult();




            //Gets All Elevator properties
            var twin = await _registryManager.GetTwinAsync(elevatorId.ToString());

            var elevator = new ElevatorDetailedModel();
            elevator.Id = Guid.Parse(twin.DeviceId);

            try { elevator.Name = twin.Properties.Reported["deviceName"]; }
            catch { elevator.Name = "Name unknown"; }

            try { elevator.Status = twin.Properties.Reported["status"]; }
            catch { elevator.Status = ElevatorDetailedModel.ElevatorStatus.Disabled; }

            try { elevator.DoorStatus = twin.Properties.Reported["doorStatus"]; }
            catch { elevator.DoorStatus = false; }

            try { elevator.CurrentLevel = twin.Properties.Reported["currentLevel"]; }
            catch { elevator.CurrentLevel = 0; }

            try { elevator.TargetLevel = twin.Properties.Reported["targetLevel"]; }
            catch { elevator.TargetLevel = 0; }

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
                var technicians = await connection.QueryAsync(
                    "SELECT Technicians.Id AS 'TechnicianId', Technicians.Name, Errands.Id AS 'ErrandId' FROM Technicians " +
                    "INNER JOIN Errands ON Technicians.Id = Errands.TechnicianId");

                var technicianResult = technicians.FirstOrDefault(technician => technician.ErrandId.ToString() == errand.Id.ToString());
                var technician = new TechnicianModel();
                if (technicianResult == null)
                    technician = null;
                else
                    technician = new TechnicianModel(technicianResult.TechnicianId, technicianResult.Name);

                addErrand.Technician = technician;



                //Gets all comments
                var comments = await connection.QueryAsync(
                    "SELECT ErrandComments.Id AS 'CommentId', ErrandComments.Content,ErrandComments.PostedAt,ErrandComments.Author,Errands.Id AS 'ErrandId' FROM ErrandComments " +
                    "INNER JOIN Errands ON ErrandComments.ErrandModelId = Errands.Id WHERE ErrandComments.ErrandModelId = @ErrandId", new {ErrandId = errand.Id});

                foreach (var comment in comments)
                {
                    var addComment = new ErrandCommentModel(
                        comment.CommentId,
                        comment.Content,
                        comment.Author,
                        comment.PostedAt);
                    addErrand.Comments.Add(addComment);
                }
                elevator.Errands.Add(addErrand);
            }

            return new OkObjectResult(elevator);
        }
    }
}
