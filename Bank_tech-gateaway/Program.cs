using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Bank_tech_gateaway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProxyConfig(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder =>
        {
            builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
        });
});

builder.Services.AddAuthConfig(builder.Configuration);

/*Rate limiting*/
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("limitPolicy", opt =>
    {
        opt.PermitLimit = 4;
        opt.Window = TimeSpan.FromSeconds(12);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
    });
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapReverseProxy();

/*Just for admins*/
app.MapGet("/", () => "Hello")
        .RequireAuthorization(x => x.RequireClaim("Scope", "Admin"));

/*Shows the user in the token*/
app.MapGet("/user", (ClaimsPrincipal user) => user.Identity?.Name)
    .RequireAuthorization();

app.MapGet("/auth/{user}/{pass}", (string user, string pass) =>
{
    var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
    var key = jwtSettingsSection["Key"];

    var tokenHandle = new JwtSecurityTokenHandler();
    var byteKey = Encoding.UTF8.GetBytes(key!);
    var tokenDes = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.Name, user),
            new Claim("Scope", "Admin")
        }),
        Expires = DateTime.UtcNow.AddHours(2),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(byteKey), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandle.CreateToken(tokenDes);
    return tokenHandle.WriteToken(token);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
