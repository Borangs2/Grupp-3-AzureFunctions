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
using System.Data.SqlClient;
using System.Data;
using System.Web.Http;
using Dapper;

namespace AzureFunctions.Maui.Errands
{
    public static class UpdateErrandLastEdited
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [FunctionName("UpdateErrandLastEdited")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "errand/update")] HttpRequest req,
            ILogger log)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();

            if (body == null || body == string.Empty)
                return new BadRequestResult();

            if (!Guid.TryParse(body, out Guid errandId))
                return new BadRequestResult();

            try
            {
                using IDbConnection connection = new SqlConnection(DbConnectionString);

                var result = await connection.QueryAsync("UPDATE Errands SET LastEdited = @CurrentTime WHERE Id = @ErrandId", new { CurrentTime = DateTime.Now.ToString("G").ToString(), ErrandId = errandId });
            }
            catch (Exception ex)
            {
                return new InternalServerErrorResult();
            }
            return new NoContentResult();
        }
    }
}
