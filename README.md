##Introduction
Managed identities for Azure resources eliminate the need to manage credentials in code. You can use them to get a Microsoft Entra token for your applications. The applications can use the token when accessing resources that support Microsoft Entra authentication. Azure manages the identity so you don't have to.
There are two types of managed identities: **system-assigned** and **user-assigned**. 
- System-assigned managed identities have their lifecycle tied to the resource that created them. This identity is restricted to only one resource, and you can grant permissions to the managed identity by using Azure role-based access control (RBAC).
- User-assigned managed identities can be used on multiple resources

##Implementation
- Create a user assigned managed Identity with the help of DevOPS team.
- The newly created managed identity should have roles assigned to it for each resource so that we can perform various operations on it. This is an important step.
- We need the ClientID and TenantID of the managed identity created in order to connect to various azure resources using the managed identity. 

#####Add the following to deployment.yaml
```
azure.workload.identity/use: "true"
sidecar.istio.io/inject: "false" 
serviceAccountName: workloadidentity
```
##### Creating Cosmos Client with Managed Identity  
```
                var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);
                var cosmosClientOptions = new CosmosClientOptions()
                {
                    RequestTimeout = TimeSpan.FromMinutes(5),
                    ConnectionMode = ConnectionMode.Gateway
                };
                var cosmosClient = new CosmosClient(cosmosDbSettings.ConnectionUri, tokenCredential, cosmosClientOptions);
```
##### Creating Blob Storage Client with Managed Identity  
```
              var blobServiceUri = new Uri(blobSettings.ConnectionUri);
              var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);
              var blobServiceClient = new BlobServiceClient(blobServiceUri, tokenCredential, blobSettings.ClientOptions);
```

##### Creating ServiceBusQueue Client with Managed Identity  
```
                var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);
                var serviceBusClient = new ServiceBusClient(serviceBusSettings.FullyQualifiedNamespace, tokenCredential);
                // Create a sender for the queue
                var sender = serviceBusClient.CreateSender(serviceBusSettings.QueueName);;

```

##### Connecting to Azure APIM with Managed Identity  
```
                var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);
                var tokenContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
                var token = await tokenCredential.GetTokenAsync(tokenContext, cancellationToken);

                // Create an HttpClient instance
                using var httpClient = new HttpClient();
                // Set the authorization header with the access token
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                httpClient.DefaultRequestHeaders.Add("Consumer", "OCAP");

                // Define the URL you want to connect to
                string uriString = "https://blockapi-dev.hrblock.net/edp/eods/client-transactions";
                string host = "blockapi-dev.hrblock.net";
                string request = "{ \"query\": \"{ taxTransactions(ucid: 553562123)}\" }";
                var content = new StringContent(request, Encoding.UTF8, "application/json");
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                content.Headers.ContentLength = request.Length;
                
                // Create the HTTP Request
                var httpRequest = new HttpRequestMessage()
                {
                    Content = content,
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(uriString)
                };
                httpRequest.Headers.Host = host;

                // Send a GET request to the URL
                HttpResponseMessage response = await httpClient.SendAsync(httpRequest);
```

##### Creating ADX connection with Managed Identity  
```
                var tokenCredential = new WorkloadIdentityCredential
                (
                   new WorkloadIdentityCredentialOptions()
                   {
                       TenantId = workloadIdentitySettings.TenantId,
                       ClientId = workloadIdentitySettings.ClientId
                   }
                );
                // Create a connection string using the cluster name and database name
                string connectionString = $"Data Source={adxSettings.Uri};Initial Catalog={adxSettings.Database};AAD Federated Security=True";

                // Create a KustoConnectionStringBuilder with the connection string
                var kustoConnectionStringBuilder = new KustoConnectionStringBuilder(connectionString);

                // Set the KustoConnectionStringBuilder with user-defined managed identity token credentials
                kustoConnectionStringBuilder = kustoConnectionStringBuilder.WithAadAzureTokenCredentialsAuthentication(tokenCredential);
                // Create a KustoIngestClient using the connection string
                var queryProvider = KustoClientFactory.CreateCslQueryProvider(kustoConnectionStringBuilder);

```
 

## References
https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/how-manage-user-assigned-managed-identities?pivots=identity-mi-methods-azp