using AzureFunctions.Maui.Elevator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Devices;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using AzureFunctions.Models;
using System.IO;

namespace AzureFunctions.Maui.Errands
{
    public static class GetErrandByTechnicianId
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private static RegistryManager _registryManager = RegistryManager.CreateFromConnectionString("HostName=Grup-3-Devops.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=cXHMcmESQtlUTvhMJ8q5aQvzf9aPcWQ9JAN6fCc2r2Q=");

        [FunctionName("GetErrandByTechnicianId")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "technicianErrand")] HttpRequest req,
            ILogger log)
        {
            var data = await new StreamReader(req.Body).ReadToEndAsync();
            if (data == string.Empty)
            {
                data = req.Query["id"];
            }

            if (!Guid.TryParse(data, out var technicianId))
                return new BadRequestResult();

            var errands = new List<ErrandModel>();
            using IDbConnection connection = new SqlConnection(DbConnectionString);

            var errandResult = await connection.QueryAsync("SELECT * FROM Errands WHERE TechnicianId = @TechnicianId", new { TechnicianId = technicianId });

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
                    "INNER JOIN Errands ON ErrandComments.ErrandModelId = Errands.Id WHERE ErrandComments.ErrandModelId = @ErrandId", new { ErrandId = errand.Id });

                foreach (var comment in comments)
                {
                    var addComment = new ErrandCommentModel(
                        comment.CommentId,
                        comment.Content,
                        comment.Author,
                        comment.PostedAt);
                    addErrand.Comments.Add(addComment);
                }
                errands.Add(addErrand);
            }

            return new OkObjectResult(errands);
        }
    }
}
