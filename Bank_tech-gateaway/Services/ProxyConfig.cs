using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

namespace Bank_tech_gateaway.Services
{
    public static class ProxyConfig
    {
        public static IServiceCollection AddProxyConfig(this IServiceCollection services, IConfiguration _config)
        {
            services.AddReverseProxy()
                .LoadFromConfig(_config.GetSection("LansProxy"))
                .LoadFromConfig(_config.GetSection("SavingsProxy"))
                .LoadFromConfig(_config.GetSection("CustomersProxy"))
                .LoadFromConfig(_config.GetSection("CreditCardsProxy"))
                .LoadFromConfig(_config.GetSection("UsersProxy"))
                .AddTransforms(builder =>
                {
                    // TODO: Set token in headers on routes users, header: Authorization: {token}
                });

            return services;
        }
    }
}
