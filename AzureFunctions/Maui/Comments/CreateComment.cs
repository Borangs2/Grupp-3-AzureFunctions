using System;
using System.IO;
using System.Threading.Tasks;
using AzureFunctions.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;
using System.Linq.Expressions;
using AzureFunctions.Models;
using Dapper;

namespace AzureFunctions.Maui.Comments
{
    public static class CreateComment
    {
        private static string DbConnectionString = "Server=tcp:kyh-devops.database.windows.net,1433;Initial Catalog=Kyh-Agile Grupp 3;Persist Security Info=False;User ID=CloudSA37b586b4;Password=Andreas1!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";


        [FunctionName("CreateComment")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "comment/create")] HttpRequest req,
            ILogger log)
        {
            CreateErrandCommentModel data;
            try
            {
                data = JsonConvert.DeserializeObject<CreateErrandCommentModel>(
                    await new StreamReader(req.Body).ReadToEndAsync());
            }
            catch
            {
                return new BadRequestResult();
            }


            if (data == null || data.Id == Guid.Empty)
                return new BadRequestResult();


            using IDbConnection connection = new SqlConnection(DbConnectionString);

            var result = await connection.QueryAsync("INSERT INTO ErrandComments VALUES (@Id, @Content, @PostedAt, @ErrandId, @Author)", 
                new {Id = data.Id, Content = data.Content, PostedAt = data.PostedAt, ErrandId = data.ErrandId, Author = data.Author});

            if(result != null )
                return new CreatedResult("", data);
            return new BadRequestResult();
        }
    }
}
