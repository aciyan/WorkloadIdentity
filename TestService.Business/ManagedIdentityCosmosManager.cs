using Microsoft.Azure.Cosmos;
using TestService.Business.Abstraction;
using TestService.Business.Models;

namespace TestService.Business
{
    public class ManagedIdentityCosmosManager : IManagedIdentityCosmosManager
    {
        #region Private Properties

        // The cosmos client
        private readonly CosmosClient _cosmosClient;
        // Cosmos Storage Database name
        private readonly string _database;
        // Cosmos Container name
        private readonly string _container;

        #endregion

        #region Public Constructor

        public ManagedIdentityCosmosManager(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            _cosmosClient = cosmosClient;
            _database = databaseName;
            _container = containerName;
        }

        #endregion

        #region Public Methods

        public async Task<List<TestDataModel>> CosmosRead(string returnId)
        {
            List<TestDataModel> taxData = new List<TestDataModel>();
            
            // Create the database if it doesn't exist
            var database = _cosmosClient.GetDatabase(_database);

            // Create the container if it doesn't exist
            var container = database.GetContainer(_container);

            string querytext = $"SELECT * FROM c WHERE c.id = '{returnId}'";
            var query = new QueryDefinition(querytext);
            var iterator = container.GetItemQueryIterator<TestDataModel>(query);

            while (iterator.HasMoreResults)
            {
                var responses = await iterator.ReadNextAsync();
                foreach (var response in responses)
                {
                    taxData.Add(response);
                }
            }
            return taxData;
        }

        public async Task<string> CosmosWrite(string taxDocument)
        {
            List<TestDataModel> taxData = new List<TestDataModel>();

            // Create the database if it doesn't exist
            var database = _cosmosClient.GetDatabase(_database);

            // Create the container if it doesn't exist
            var container = database.GetContainer(_container);

            //Console.WriteLine($"Connected to Cosmos DB container {container.Id} using Managed Identity.");
            var key = Guid.NewGuid();
            var partitionKey = new PartitionKey(key.ToString());
            var item = new TestDataModel
            {
                id = key.ToString(),
                TestDocument = taxDocument,
                TestReturnGuid = key
            };

            var response = await container.CreateItemAsync(item, partitionKey);
            return response.StatusCode.ToString();
        }
        #endregion

    }
}
