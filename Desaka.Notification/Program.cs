using Desaka.DataAccess;
using Desaka.EventBus;
using Desaka.ServiceDefaults;
using Desaka.Notification.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDesakaServiceDefaults(builder.Configuration);
builder.Services.AddDesakaDataAccess(builder.Configuration);
builder.Services.AddDesakaEventBus(builder.Configuration);
builder.Services.AddSingleton<NotificationStream>();
builder.Services.AddHostedService<NotificationEventSubscriber>();

var app = builder.Build();

app.UseDesakaSwaggerIfDev(app.Environment);
app.UseHttpsRedirection();
app.UseDesakaApiKey();

app.MapControllers();
app.MapDesakaHealth();

app.Run();
