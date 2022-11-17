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

        private static string DbConnectionString = "HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=";
        private static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=");


        [FunctionName("GetElevatorById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "elevator")] HttpRequest req,
            ILogger log)
        {
            dynamic data = JsonConvert.DeserializeObject(await new StreamReader(req.Body).ReadToEndAsync());
            
            if (data == null || data.GetType() != typeof(int))
                return new BadRequestResult();

            //Gets All Elevator properties
            var twin = _registryManager.GetTwinAsync(data);

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



            using IDbConnection connection = new SqlConnection(DbConnectionString);

            //Gets all errand properties
            elevator.ConnectionString = await connection.QueryFirstOrDefaultAsync<string>("SELECT ConnectionString FROM Elevators WHERE Id = @Id", new {Id = data});
            elevator.Errands = (List<ErrandModel>) await connection.QueryAsync("SELECT * FROM Errands WHERE ElevatorId = @ElevatorId", new {ElevatorId = data});

            //Gets the technician
            var technicians = await connection.QueryAsync(
                "SELECT Technicians.Id AS 'TechnicianId', Technicians.Name, Errands.Id AS 'ErrandId' FROM Technicians " +
                "INNER JOIN Errands ON Technicians.Id = Errands.TechnicianId");

            foreach (var errand in elevator.Errands)
            {
                var result = technicians.FirstOrDefault(technician => technician.ErrandId = errand.Id.ToString());
                var technician = new TechnicianModel();
                if (result == null)
                    technician = null;
                else
                    technician = new TechnicianModel(result.TechnicianId, result.Name);

                errand.Technician = technician;
            }

            //Gets all comments
            var comments = await connection.QueryAsync(
                "ErrandComments.Id AS 'CommentId', ErrandComments.Content, ErrandComments.PostedAt, ErrandComments.Author, Errands.Id AS 'ErrandId'" +
                "FROM ErrandComments INNER JOIN Errands ON ErrandComments.ErrandModelId = Errands.Id");

            foreach (var errand in elevator.Errands)
            {
                var result = comments.FirstOrDefault(technician => technician.ErrandId = errand.Id.ToString());

                foreach (var comment in comments)
                {
                    var addComment = new ErrandCommentModel()
                    {
                        Id = comment.CommentId,
                        Content = comment.Content,
                        PostedAt = comment.PostedAt,
                        Author = comment.PostedAt
                    };
                    errand.Comments.Add(addComment);
                }

            }


            return new OkObjectResult(elevator);
        }
    }
}
