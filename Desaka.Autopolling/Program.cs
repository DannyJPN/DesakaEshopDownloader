using Desaka.Autopolling.Application;
using Desaka.Autopolling.Infrastructure;
using Desaka.DataAccess;
using Desaka.EventBus;
using Desaka.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDesakaServiceDefaults(builder.Configuration);
builder.Services.AddDesakaDataAccess(builder.Configuration);
builder.Services.AddDesakaEventBus(builder.Configuration);
builder.Services.AddScoped<IAutopollService, AutopollService>();
builder.Services.AddScoped<AutopollBatchCommitService>();
builder.Services.AddScoped<Desaka.Export.ExportService>();
builder.Services.AddSingleton<Desaka.Comparation.IProductComparator, Desaka.Comparation.ProductComparator>();
builder.Services.AddSingleton<AutopollFilterEngine>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDesakaSwaggerIfDev(app.Environment);
app.UseHttpsRedirection();
app.UseDesakaApiKey();

app.MapControllers();
app.MapDesakaHealth();

app.Run();
