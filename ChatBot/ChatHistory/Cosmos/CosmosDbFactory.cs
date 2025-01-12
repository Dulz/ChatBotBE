using Microsoft.Azure.Cosmos;

namespace ChatBot.ChatHistory.Cosmos;

public static class CosmosDbFactory
{
    public static async Task<Database> GetDatabase(IConfiguration configuration)
    {
        var cosmosClient =
            new CosmosClient(configuration["CosmosDb:EndPointUri"], configuration["CosmosDb:PrimaryKey"]);
        await cosmosClient.CreateDatabaseIfNotExistsAsync("ChatDb");
        var database = cosmosClient.GetDatabase("ChatDb");

        await CreateContainers(database);

        return database;
    }

    private static async Task CreateContainers(Database database)
    {
        await database.CreateContainerIfNotExistsAsync(CosmosChatHistory.ContainerId, "/conversationId");
        await database.CreateContainerIfNotExistsAsync(CosmosChatHistory.UserContainerId, "/userId");
    }
}