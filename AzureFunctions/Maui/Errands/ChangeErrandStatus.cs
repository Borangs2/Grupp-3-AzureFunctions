using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Dapper;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AzureFunctions.Maui.Errands
{
    public static class ChangeErrandStatus
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [FunctionName("ChangeErrandStatus")]
        public static async Task<IActionResult> Run(
                [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "errands")] HttpRequest req,
                ILogger log)
        {
            string body = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<dynamic>(body);


            if (data.status == null || data.errandId == null)
                return new BadRequestResult();

            if (!Enum.TryParse(data.status.ToString(), out ErrandStatus newStatus))
                return new BadRequestResult();

            if (!Guid.TryParse(data.errandId.ToString(), out Guid errandId))
                return new BadRequestResult();

            try
            {
                using IDbConnection connection = new SqlConnection(DbConnectionString);

                var result = await connection.QueryAsync("UPDATE Errands SET Status = @NewStatus WHERE Id = @ErrandId", new{NewStatus = newStatus.ToString(), ErrandId = errandId});
            }
            catch(Exception ex)
            {
                return new InternalServerErrorResult();
            }
            return new NoContentResult();
        }
    }
}
