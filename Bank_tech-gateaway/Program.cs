using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("LansProxy"))
    .LoadFromConfig(builder.Configuration.GetSection("SavingsProxy"))
    .LoadFromConfig(builder.Configuration.GetSection("CustomersProxy"))
    .LoadFromConfig(builder.Configuration.GetSection("CreditCardsProxy"))
    .LoadFromConfig(builder.Configuration.GetSection("UsersProxy"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

builder.Services.AddAuthentication("Bearer").AddJwtBearer(opt =>
{
    var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
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

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

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
