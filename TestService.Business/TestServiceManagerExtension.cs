using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Kusto.Data;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.SignalR;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TestService.Business.Abstraction;
using TestService.Business.Configuration;

namespace TestService.Business
{
    /// <summary>
    /// Represents TestManagerExtension.
    /// </summary>
    public static class TestServiceManagerExtension
    {
        #region Public Methods

        /// <summary>
        /// Adds the test manager services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static async Task<IServiceCollection> AddTestManagerServicesAsync(this IServiceCollection services, IConfiguration configuration)
        {
            //AddCosmosStorage(services);
            AddBlobStorage(services);
            AddServiceBusQueue(services);
            AddAzureAPIMAuth(services);
            AddAdxManager(services);
            AddCosmosStorageWithWorkloadIdentity(services);
            //AddBlobStorageWithWorkloadIdentity(services);
            //AddServiceBusQueueWithWorkloadIdentity(services);
            //AddAzureAPIMAuthWithWorkloadIdentity(services);
            //AddAdxManagerWithWorkloadIdentity(services);
            await AddSignalRWithWorkloadIdentity(services, configuration);
            AddAppInsightsWithWorkloadIdentity(services, configuration);
            await AddRedisWithWorkloadIdentity(services, configuration);
            return services;
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// To add the managed Identity provider for cosmos and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddCosmosStorage(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityCosmosManager, ManagedIdentityCosmosManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();

                var cosmosDbSettings = configuration?.GetSection("CosmosDbOptions").Get<CosmosDbOptions>();
                if (cosmosDbSettings == null)
                    throw new ArgumentException("CosmosDbOptions is null.");
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                //var tokenCredential = new DefaultAzureCredential();
                //var tokenCredential = new DefaultAzureCredential(
                //    new DefaultAzureCredentialOptions {
                //        ExcludeAzureCliCredential = true,
                //        ExcludeAzurePowerShellCredential = true,
                //        ExcludeInteractiveBrowserCredential = true,
                //        ExcludeSharedTokenCacheCredential = true,
                //        ExcludeVisualStudioCodeCredential = true,
                //        ExcludeVisualStudioCredential = true,
                //        ExcludeEnvironmentCredential = true,
                //        ExcludeWorkloadIdentityCredential = true,
                //        ManagedIdentityClientId = managedIdentitySettings.ClientId
                //    });
                var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);
                var cosmosClientOptions = new CosmosClientOptions()
                {
                    RequestTimeout = TimeSpan.FromMinutes(5),
                    ConnectionMode = ConnectionMode.Gateway
                };
                var cosmosClient = new CosmosClient(cosmosDbSettings.ConnectionUri, tokenCredential, cosmosClientOptions);

                return new ManagedIdentityCosmosManager(cosmosClient, cosmosDbSettings.Database, cosmosDbSettings.Container);
            });
            return services;
        }

        /// <summary>
        /// To add the Managed Identity provider for Blob and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddBlobStorage(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityBlobManager, ManagedIdentityBlobManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();

                var blobSettings = configuration?.GetSection("BlobOptions").Get<BlobOptions>();
                if (blobSettings == null)
                    throw new ArgumentException("BlobOptions is null.");
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var blobServiceUri = new Uri(blobSettings.ConnectionUri);
                var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);
                var blobServiceClient = new BlobServiceClient(blobServiceUri, tokenCredential, blobSettings.ClientOptions);

                return new ManagedIdentityBlobManager(blobServiceClient);
            });
            return services;
        }

        /// <summary>
        /// To add the Managed Identity provider for Blob and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddServiceBusQueue(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityQueueManager, ManagedIdentityQueueManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();

                var serviceBusSettings = configuration?.GetSection("ServiceBusOptions").Get<ServiceBusOptions>();
                if (serviceBusSettings == null)
                    throw new ArgumentException("ServiceBusOptions is null.");
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);
                var serviceBusClient = new ServiceBusClient(serviceBusSettings.FullyQualifiedNamespace, tokenCredential);
                // Create a sender for the queue
                var sender = serviceBusClient.CreateSender(serviceBusSettings.QueueName);

                return new ManagedIdentityQueueManager(sender);
            });
            return services;
        }

        /// <summary>
        /// To add the Managed Identity provider for APIM and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddAzureAPIMAuth(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityApimManager, ManagedIdentityApimManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var tokenCredential = new ManagedIdentityCredential(managedIdentitySettings.ClientId);

                return new ManagedIdentityApimManager(tokenCredential);
            });
            return services;
        }

        /// <summary>
        /// To add the Managed Identity provider for ADX and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddAdxManager(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityAdxManager, ManagedIdentityAdxManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var adxSettings = configuration?.GetSection("AdxConnectionSettings").Get<AdxConnectionSettings>();
                if (adxSettings == null)
                    throw new ArgumentException("AdxConnectionSettings is null.");

                // Create a connection string using the cluster name and database name
                string connectionString = $"Data Source={adxSettings.Uri};Initial Catalog={adxSettings.Database};AAD Federated Security=True";

                // Create a KustoConnectionStringBuilder with the connection string
                var kustoConnectionStringBuilder = new KustoConnectionStringBuilder(connectionString);

                // Set the KustoConnectionStringBuilder with user-defined managed identity token credentials
                kustoConnectionStringBuilder = kustoConnectionStringBuilder.WithAadUserManagedIdentity(managedIdentitySettings.ClientId);

                return new ManagedIdentityAdxManager(kustoConnectionStringBuilder);
            });
            return services;
        }

        /// <summary>
        /// To add the Workload Identity provider for cosmos and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddCosmosStorageWithWorkloadIdentity(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityCosmosManager, ManagedIdentityCosmosManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();

                var cosmosDbSettings = configuration?.GetSection("CosmosDbOptions").Get<CosmosDbOptions>();
                if (cosmosDbSettings == null)
                    throw new ArgumentException("CosmosDbOptions is null.");
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");
                
                var tokenCredential = new WorkloadIdentityCredential
                (
                    new WorkloadIdentityCredentialOptions()
                    {
                        TenantId = managedIdentitySettings.TenantId,
                        ClientId = managedIdentitySettings.ClientId
                    }
                );
                var cosmosClientOptions = new CosmosClientOptions()
                {
                    RequestTimeout = TimeSpan.FromMinutes(5),
                    ConnectionMode = ConnectionMode.Gateway
                };
                var cosmosClient = new CosmosClient(cosmosDbSettings.ConnectionUri, tokenCredential, cosmosClientOptions);

                return new ManagedIdentityCosmosManager(cosmosClient, cosmosDbSettings.Database, cosmosDbSettings.Container);
            });
            return services;
        }
        /// <summary>
        /// To add the Workload Identity provider for Blob and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddBlobStorageWithWorkloadIdentity(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityBlobManager, ManagedIdentityBlobManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();

                var blobSettings = configuration?.GetSection("BlobOptions").Get<BlobOptions>();
                if (blobSettings == null)
                    throw new ArgumentException("BlobOptions is null.");
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var blobServiceUri = new Uri(blobSettings.ConnectionUri);
                var tokenCredential = new WorkloadIdentityCredential
                (
                    new WorkloadIdentityCredentialOptions()
                    {
                        TenantId = managedIdentitySettings.TenantId,
                        ClientId = managedIdentitySettings.ClientId
                    }
                );
                var blobServiceClient = new BlobServiceClient(blobServiceUri, tokenCredential, blobSettings.ClientOptions);

                return new ManagedIdentityBlobManager(blobServiceClient);
            });
            return services;
        }

        /// <summary>
        /// To add the Workload Identity provider for Blob and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddServiceBusQueueWithWorkloadIdentity(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityQueueManager, ManagedIdentityQueueManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();

                var serviceBusSettings = configuration?.GetSection("ServiceBusOptions").Get<ServiceBusOptions>();
                if (serviceBusSettings == null)
                    throw new ArgumentException("ServiceBusOptions is null.");
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var tokenCredential = new WorkloadIdentityCredential
                (
                    new WorkloadIdentityCredentialOptions()
                    {
                        TenantId = managedIdentitySettings.TenantId,
                        ClientId = managedIdentitySettings.ClientId
                    }
                ); 
                var serviceBusClient = new ServiceBusClient(serviceBusSettings.FullyQualifiedNamespace, tokenCredential);
                // Create a sender for the queue
                var sender = serviceBusClient.CreateSender(serviceBusSettings.QueueName);

                return new ManagedIdentityQueueManager(sender);
            });
            return services;
        }

        /// <summary>
        /// To add the WorkloadIdentity provider for APIM and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddAzureAPIMAuthWithWorkloadIdentity(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityApimManager, ManagedIdentityApimManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var managedIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (managedIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var tokenCredential = new WorkloadIdentityCredential
               (
                   new WorkloadIdentityCredentialOptions()
                   {
                       TenantId = managedIdentitySettings.TenantId,
                       ClientId = managedIdentitySettings.ClientId
                   }
               );
                return new ManagedIdentityApimManager(tokenCredential);
            });
            return services;
        }

        /// <summary>
        /// To add the WorkloadIdentity provider for ADX and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddAdxManagerWithWorkloadIdentity(this IServiceCollection services)
        {
            services.AddTransient<IManagedIdentityAdxManager, ManagedIdentityAdxManager>(x =>
            {
                var configuration = x.GetService<IConfiguration>();
                var workloadIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
                if (workloadIdentitySettings == null)
                    throw new ArgumentException("MangedIdentitySettings is null.");

                var adxSettings = configuration?.GetSection("AdxConnectionSettings").Get<AdxConnectionSettings>();
                if (adxSettings == null)
                    throw new ArgumentException("AdxConnectionSettings is null.");

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

                return new ManagedIdentityAdxManager(kustoConnectionStringBuilder);
            });
            return services;
        }

        /// <summary>
        /// To add the WorkloadIdentity provider for SignalR and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static async Task<IServiceCollection> AddSignalRWithWorkloadIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var signalRUrl = configuration?.GetSection("SignalRSettings").Get<string>();
            if (signalRUrl == null)
                throw new ArgumentException("SignalRSettings is null.");

            var workloadIdentitySettings = configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>();
            if (workloadIdentitySettings == null)
                throw new ArgumentException("WorkloadIdentitySettings is null.");

            var credential = new ManagedIdentityCredential(workloadIdentitySettings.ClientId);
            var serviceManager = new ServiceManagerBuilder()
                    .WithOptions(option =>
                    {
                        option.ConnectionString = $"Endpoint={signalRUrl};AuthType=azure.msi;ClientId={workloadIdentitySettings.ClientId};";
                    })
                    .BuildServiceManager();
            services.AddSingleton(serviceManager);
            var hubcontext = await serviceManager.CreateHubContextAsync("TestHub", new CancellationToken());
            services.AddSingleton(hubcontext);
            //services.AddSignalR().AddAzureSignalR(option =>
            //{
            //    option.Endpoints = new ServiceEndpoint[]
            //    {
            //        new ServiceEndpoint(new Uri(signalRUrl), credential),
            //    };
            //});

            services.AddTransient<IWorkloadIdentitySignalRManager, WorkloadIdentitySignalRManager>(x =>
            {
                return new WorkloadIdentitySignalRManager(hubcontext);
            });
            return services;
        }

        /// <summary>
        /// To add the WorkloadIdentity provider for Redis and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static async Task<IServiceCollection> AddRedisWithWorkloadIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var workloadIdentitySettings = configuration.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>() ?? throw new ArgumentException("MangedIdentitySettings is null.");
            var redisConnectionString = (configuration?.GetSection("RedisEndpoint").Get<string>()) ?? throw new ArgumentException("Redis Configuration is null.");

            //var tokenCredential = new WorkloadIdentityCredential
            //(
            //   new WorkloadIdentityCredentialOptions()
            //   {
            //       TenantId = workloadIdentitySettings.TenantId,
            //       ClientId = workloadIdentitySettings.ClientId
            //   }
            //);
            //StringWriter connectionLog = new();
            var tokenCredential = new ManagedIdentityCredential(workloadIdentitySettings.ClientId);

            //var configurationOptions = await ConfigurationOptions.Parse(redisConnectionString).ConfigureForAzureWithTokenCredentialAsync(tokenCredential);
            //configurationOptions.AbortOnConnectFail = false; // Fail fast for the purposes of this sample. In production code, this should remain false to retry connections on startup
            //configurationOptions.Ssl = true;
            //var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions, connectionLog);

            services.AddTransient<IWorkloadIdentityRedisManager, WorkloadIdentityRedisManager>(x =>
            {
                return new WorkloadIdentityRedisManager(workloadIdentitySettings, tokenCredential, redisConnectionString);
            });
            //Console.WriteLine(connectionLog);
            return services;
        }

        /// <summary>
        /// To add the WorkloadIdentity provider for AppInsights and connection will be read from configuration.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IServiceCollection AddAppInsightsWithWorkloadIdentity(this IServiceCollection services, IConfiguration configuration)
        {
            var workloadIdentitySettings = (configuration?.GetSection("ManagedIdentitySettings").Get<WorkloadIdentitySettings>()) ?? throw new ArgumentException("MangedIdentitySettings is null.");
            var appInsightsConnectionString = (configuration?.GetSection("AppInsightsConnectionString").Get<string>()) ?? throw new ArgumentException("Application Insights is null.");
            services.Configure<TelemetryConfiguration>(config =>
            {
                //var credential = new WorkloadIdentityCredential
                //(
                //   new WorkloadIdentityCredentialOptions()
                //   {
                //       TenantId = workloadIdentitySettings.TenantId,
                //       ClientId = workloadIdentitySettings.ClientId
                //   }
                //);
                var credential = new ManagedIdentityCredential(workloadIdentitySettings.ClientId);
                config.SetAzureTokenCredential(credential);
            });
            services.AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
            {
                ConnectionString = appInsightsConnectionString
            });
            services.AddSingleton<TelemetryClient>(sp =>
            {
                var telemetryConfiguration = sp.GetRequiredService<TelemetryConfiguration>();
                return new TelemetryClient(telemetryConfiguration);
            });
            // Configure logging
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddApplicationInsights();
                loggingBuilder.AddConsole();
            });
            services.AddTransient<IApplicationInsightsWIManager, ApplicationInsightsWIManager>();
            return services;
        }


        #endregion Private Methods
    }
}
