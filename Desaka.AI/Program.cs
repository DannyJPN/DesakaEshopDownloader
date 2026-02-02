using Desaka.AI.Application;
using Desaka.AI.Infrastructure;
using Desaka.DataAccess;
using Desaka.EventBus;
using Desaka.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDesakaServiceDefaults(builder.Configuration);
builder.Services.AddDesakaDataAccess(builder.Configuration);
builder.Services.AddDesakaEventBus(builder.Configuration);
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddHostedService<PricingUpdater>();

var app = builder.Build();

app.UseDesakaSwaggerIfDev(app.Environment);
app.UseHttpsRedirection();
app.UseDesakaApiKey();

app.MapControllers();
app.MapDesakaHealth();

app.Run();
