using Desaka.DataAccess;
using Desaka.EventBus;
using Desaka.Scraping.Services;
using Desaka.ServiceDefaults;
using Desaka.WebScrape.Application;
using Desaka.WebScrape.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDesakaServiceDefaults(builder.Configuration);
builder.Services.AddDesakaDataAccess(builder.Configuration);
builder.Services.AddDesakaEventBus(builder.Configuration);
builder.Services.AddScoped<IWebScrapeService, WebScrapeService>();
builder.Services.AddSingleton<EshopScraperRegistry>();
builder.Services.AddScoped<ISiteScraper, GenericSitemapScraper>();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDesakaSwaggerIfDev(app.Environment);
app.UseHttpsRedirection();
app.UseDesakaApiKey();

app.MapControllers();
app.MapDesakaHealth();

app.Run();
