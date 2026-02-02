using Desaka.DataAccess;
using Desaka.EventBus;
using Desaka.ServiceDefaults;
using Desaka.Unifier.Application;
using Desaka.Unifier.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDesakaServiceDefaults(builder.Configuration);
builder.Services.AddDesakaDataAccess(builder.Configuration);
builder.Services.AddDesakaEventBus(builder.Configuration);
builder.Services.AddScoped<IUnifierService, UnifierService>();
builder.Services.AddScoped<Desaka.Export.ExportService>();
builder.Services.AddScoped<MemoryLookupService>();
builder.Services.AddScoped<CodeGenerator>();
builder.Services.AddScoped<UnifierProcessor>();

var app = builder.Build();

app.UseDesakaSwaggerIfDev(app.Environment);
app.UseHttpsRedirection();
app.UseDesakaApiKey();

app.MapControllers();
app.MapDesakaHealth();

app.Run();
