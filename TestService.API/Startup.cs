using HRBlock.OCAP.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using TestService.Business;

namespace TestService.API
{
    /// <summary>
    /// Startup for configuring the required services and middle wares of Test Web API.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        #endregion Public Constructors

        #region Variables

        public IConfiguration Configuration { get; }

        #endregion Variables

        #region Public Methods

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application.</param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseDefaultMiddlewares();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The services.</param>
        public async Task  ConfigureServices(IServiceCollection services)
        {
            services.AddDefaultServices();
            await services.AddTestManagerServicesAsync(Configuration);
        }

        #endregion Public Methods }
    }
}
