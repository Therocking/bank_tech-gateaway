using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Bank_tech_gateaway.Services
{
    public static class AuthConfig
    {
        public static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration _config)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuthenticatedUser", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddAuthentication("Bearer").AddJwtBearer(opt =>
            {
                var jwtSettingsSection = _config.GetSection("JwtSettings");
                var signKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettingsSection["Key"]!));
                var singCredencial = new SigningCredentials(signKey, SecurityAlgorithms.HmacSha256Signature);

                opt.RequireHttpsMetadata = false;

                opt.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey = signKey
                };
            });

            return services;
        }
    }
}
