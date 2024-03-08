var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("LansProxy"))
    .LoadFromConfig(builder.Configuration.GetSection("SavingsProxy"))
    .LoadFromConfig(builder.Configuration.GetSection("CustomersProxy"))
    .LoadFromConfig(builder.Configuration.GetSection("CreditCardsProxy"));
    //.AddConfigFilter<CustomConfigFilter>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.MapReverseProxy();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
