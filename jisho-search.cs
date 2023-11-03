using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace Chanjma.Function
{
    public static class jisho_search
    {
        
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("jisho_search")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string keyword = req.Query["keyword"];

            log.LogInformation("Jisho Search Request started. Keyword {keyword}", keyword);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            keyword = keyword ?? data?.keyword;

            if(string.IsNullOrEmpty(keyword))
            {
                return new OkObjectResult("This HTTP triggered function executed successfully. Pass a keyword (&keyword=love) in the query string or in the request body for a personalized response.");
            }

            var apiUrl = string.Format("https://jisho.org/api/v1/search/words?keyword={0}", keyword);
            try
            {
                // Perform the GET request
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                // Check if the request was successful
                if (response.IsSuccessStatusCode)
                {
                    // Read the response content
                    string responseData = await response.Content.ReadAsStringAsync();
                    return new OkObjectResult(responseData);
                }
                else
                {
                    // If the request fails, return an error status code and message
                    return new StatusCodeResult((int)response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                log.LogError(ex, "Error making HTTP GET request: {url}", apiUrl);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
