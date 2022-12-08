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
using AzureFunctions.Helpers;
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

            errandResult.Technician = await TechnicianHelper.GetErrandTechnicianAsync(data, connection);
            errandResult.Comments = await CommentHelper.GetErrandCommentsAsync(errandResult.Id.ToString(), connection);

            return new OkObjectResult(errandResult);
        }
    }
}
