using Azure.Core;
using Microsoft.Extensions.Logging;
using TestService.Business.Abstraction;
using Kusto.Data;
using Kusto.Data.Net.Client;

namespace TestService.Business
{
    public class ManagedIdentityAdxManager : IManagedIdentityAdxManager
    {
        #region Private Properties

        // The token credential
        private readonly TokenCredential _tokenCredential;
        private readonly KustoConnectionStringBuilder _kustoConnectionStringBuilder;
        private readonly ILogger _logger = Logger.CreateLogger(nameof(ManagedIdentityAdxManager));

        #endregion

        #region Public Constructor

        public ManagedIdentityAdxManager(KustoConnectionStringBuilder kustoConnectionStringBuilder)
        {
            _kustoConnectionStringBuilder = kustoConnectionStringBuilder;
        }

        #endregion

        #region Public Methods

        public async Task<List<string[]>> TestAdxConnection()
        {
            List<string[]> queryResults = new List<string[]>();
            // Create a KustoIngestClient using the connection string
            using (var queryProvider = KustoClientFactory.CreateCslQueryProvider(_kustoConnectionStringBuilder))
            {
                try
                {
                    // Open the connection
                   var resultReader = queryProvider.ExecuteQuery("traces | take 10");
                    while (resultReader.Read())
                    {
                        string[] rowData = new string[resultReader.FieldCount];
                        for (int i = 0; i < resultReader.FieldCount; i++)
                        {
                            rowData[i] = resultReader[i]?.ToString();
                        }
                        queryResults.Add(rowData);
                        
                    }
                    if (queryResults.Count == 0)
                    {
                        _logger.LogInformation("TestService-ManageIdentity-Adx: Table is empty. No data to show");
                    }
                    _logger.LogInformation("TestService-ManageIdentity-Adx:ADX connectivity successful. Query executed successfully.");
                    return queryResults;

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"TestService-ManageIdentity-Adx: An error occurred while connecting to Azure Data Explorer: {ex.Message}");
                    return null;
                }

            }
        }
        #endregion
    }
}
