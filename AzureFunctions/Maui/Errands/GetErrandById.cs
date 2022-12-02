using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using AzureFunctions.Models;
using Microsoft.Azure.Amqp.Framing;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using Dapper;

namespace AzureFunctions.Maui.Errands
{
    public static class GetErrandById
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [FunctionName("GetErrandById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "errand")] HttpRequest req,
            ILogger log)
        {

            var data = await new StreamReader(req.Body).ReadToEndAsync();
            if (data == string.Empty)
            {
                data = req.Query["id"];
            }

            if (data == string.Empty)
                return new BadRequestResult();

            using IDbConnection connection = new SqlConnection(DbConnectionString);

            //Gets all errand properties
            var errandResult = await connection.QueryFirstOrDefaultAsync<ErrandModel>("SELECT * FROM Errands WHERE Id = @Id", new { Id = data });

            
            //Gets the technician
            var technicians = await connection.QueryAsync(
                "SELECT Technicians.Id AS 'TechnicianId', Technicians.Name, Errands.Id AS 'ErrandId' FROM Technicians " +
                "INNER JOIN Errands ON Technicians.Id = Errands.TechnicianId");

            var technicianResult = technicians.FirstOrDefault(technician => technician.ErrandId.ToString() == errandResult.Id.ToString());
            var technician = new TechnicianModel();
            if (technicianResult == null)
                technician = null;
            else
                technician = new TechnicianModel(technicianResult.TechnicianId, technicianResult.Name);

            errandResult.Technician = technician;



            //Gets all comments
            var comments = await connection.QueryAsync(
                "SELECT ErrandComments.Id AS 'CommentId', ErrandComments.Content,ErrandComments.PostedAt,ErrandComments.Author,Errands.Id AS 'ErrandId' FROM ErrandComments " +
                "INNER JOIN Errands ON ErrandComments.ErrandModelId = Errands.Id WHERE ErrandComments.ErrandModelId = @ErrandId", new { ErrandId = errandResult.Id });

            errandResult.Comments = new List<ErrandCommentModel>();
            foreach (var comment in comments)
            {
                var addComment = new ErrandCommentModel(
                    comment.CommentId,
                    comment.Content,
                    comment.Author,
                    comment.PostedAt);
                errandResult.Comments.Add(addComment);
                errandResult.Comments = errandResult.Comments.OrderByDescending(c => c.PostedAt).ToList();
            }


            return new OkObjectResult(errandResult);
        }
    }
}
