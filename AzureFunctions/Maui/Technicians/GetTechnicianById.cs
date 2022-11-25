using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;
using System.Web.Http;
using AzureFunctions.Models;
using Dapper;

namespace AzureFunctions.Maui.Technicians
{
    public static class GetTechnicianById
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

        [FunctionName("GetTechnicianById")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "technician")] HttpRequest req,
            ILogger log)
        {
            string data;
            try
            {
                data = JsonConvert.DeserializeObject(await new StreamReader(req.Body).ReadToEndAsync()).ToString();
            }
            catch
            {
                data = await new StreamReader(req.Body).ReadToEndAsync();
            }
            if (data == string.Empty)
            {
                data = req.Query["id"];
            }

            if (data == string.Empty)
                return new BadRequestResult();

            using IDbConnection connection = new SqlConnection(DbConnectionString);

            TechnicianModel technician;
            try
            {
                technician = await connection.QueryFirstOrDefaultAsync<TechnicianModel>("SELECT * FROM Technicians WHERE Id = @Id", new { Id = data });
                if (technician == null || technician.Id == Guid.Empty)
                {
                    return new NotFoundResult();
                }
            }
            catch(Exception ex)
            {
                return new InternalServerErrorResult();
            }

            return new OkObjectResult(technician);
        }
    }
}
