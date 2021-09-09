using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KafkaListener
{
    public class Startup
    {
        private const string ConsumerConfigKey = "ConsumerConfig";
        private const string ListenerConfigKey = "ListenerConfig";

        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddOptions<ConsumerConfig>().Bind(_configuration.GetSection(ConsumerConfigKey));
            services.AddOptions<ListenerConfig>().Bind(_configuration.GetSection(ListenerConfigKey));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
        }
    }
}
