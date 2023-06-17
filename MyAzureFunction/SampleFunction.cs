using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

public static class SampleFunction
{
    [FunctionName("SampleFunction")]
    public static async Task<string> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
    {
        log.LogInformation("Fetching data from database using a stored procedure");

        var config = new ConfigurationBuilder()
            .SetBasePath(context.FunctionAppDirectory)
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
        var connectionString = config.GetConnectionString("str");

        // Connect to the database
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Execute the query
            using (SqlCommand command = new SqlCommand("GetAllItemRecords", connection))
            {
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Log the results
                        log.LogInformation($"ItemCode: {reader["ItemCode"]}, ItemName: {reader["ItemName"]}, BuyingPrice: {reader["BuyingPrice"]}, SellingPrice: {reader["SellingPrice"]}, CreatedDate: {reader["CreatedDate"]}, Terminus: {reader["Terminus"]}");
                    }

                }
            }
        }

        return "Data fetched successfully";
        string responseMessage = string.IsNullOrEmpty("log")
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {"log"}. This HTTP triggered function executed successfully.";
    }
}
