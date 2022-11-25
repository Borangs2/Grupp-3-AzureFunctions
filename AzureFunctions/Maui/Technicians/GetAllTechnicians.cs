using System;
using System.Collections.Generic;
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
using System.Web.Http;
using AzureFunctions.Models;
using Dapper;
using Microsoft.Azure.Amqp.Serialization;

namespace AzureFunctions.Maui.Technicians
{
    public static class GetAllTechnicians
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";


        [FunctionName("GetAllTechnicians")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "technician/all")] HttpRequest req,
            ILogger log)
        {
            try
            {
                using IDbConnection connection = new SqlConnection(DbConnectionString);
                var result = await connection.QueryAsync<TechnicianModel>("SELECT * FROM Technicians");


                return new OkObjectResult(result);
            }
            catch(Exception ex)
            {
                return new InternalServerErrorResult();
            }

        }
    }
}
