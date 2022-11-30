using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureFunctions.Maui
{
    public static class UpdateLastEdited
    {
        public static async Task Update(string errandId)
        {
            var url = $"https://grupp3azurefunctions.azurewebsites.net/api/errand/update?";

            var client = new HttpClient();
            var content = new StringContent($"{errandId}", Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
        }
    }
}
