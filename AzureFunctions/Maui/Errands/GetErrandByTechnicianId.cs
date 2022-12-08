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
using AzureFunctions.Helpers;

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
                    Comments = await CommentHelper.GetErrandCommentsAsync(errand.Id.ToString(), connection),
                    Technician = await TechnicianHelper.GetErrandTechnicianAsync(errand.Id.ToString(), connection),
                };
                errands.Add(addErrand);
            }

            return new OkObjectResult(errands);
        }
    }
}
