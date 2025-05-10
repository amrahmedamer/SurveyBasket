  using Hangfire;
using HangfireBasicAuthenticationFilter;
using Microsoft.Extensions.Configuration;
using Serilog;
using SurveyBasket.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);
builder.Services.AddDistributedMemoryCache();



builder.Host.UseSerilog( (context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
    app.UseSwagger();
}
app.UseExceptionHandler();
app.UseHttpsRedirection();

// Middleware to log HTTP requests
app.UseSerilogRequestLogging();

app.UseCors("MyPolicy");
app.UseAuthorization();
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization =
   [
            new HangfireCustomBasicAuthenticationFilter
            {
                User = app.Configuration.GetValue<string>("HangfireSetting:Username"),
                Pass = app.Configuration.GetValue<string>("HangfireSetting:Password")
            }

    ],
    DashboardTitle="Survey Basket Dashborad"
});
var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using var scope= scopeFactory.CreateScope();
var notificationSevice = scope.ServiceProvider.GetRequiredService<INotificationService>();
//RecurringJob.AddOrUpdate("SendNewPollNotification",()=> notificationSevice.SendNewPollNotification(null), "20 6 * * *");
app.MapControllers();

app.Run();
