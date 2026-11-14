using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Portfolio.TradeOrderService.Configuration;
using Portfolio.TradeOrderService.DataAccess;
using Portfolio.TradeOrderService.Database;
using Portfolio.TradeOrderService.ServiceInterfaces;
using Portfolio.TradeOrderService.Services;

const string DashboardCors = "dashboard";

var builder = WebApplication.CreateBuilder(args);

var dashboardOrigin = builder.Configuration["Dashboard:Origin"] ?? "http://localhost:3000";
builder.Services.AddCors(opts => opts.AddPolicy(DashboardCors, p =>
    p.WithOrigins(dashboardOrigin).AllowAnyHeader().AllowAnyMethod()));

var authority = builder.Configuration["Auth:Authority"]
    ?? throw new InvalidOperationException("Auth:Authority configuration is missing.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.TokenValidationParameters.ValidateAudience = false;
        // Allow HTTP in dev; remove for production
        options.RequireHttpsMetadata = false;
    });

builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Trade Order Service", Version = "v1" });

    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a Bearer token obtained from POST http://localhost:5001/connect/token"
    };
    c.AddSecurityDefinition("Bearer", scheme);
    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var dbOptions = builder.Configuration
    .GetSection(DatabaseOptions.SectionName)
    .Get<DatabaseOptions>()
    ?? throw new InvalidOperationException("Database configuration is missing.");

builder.Services.AddSingleton(dbOptions);
builder.Services.AddSingleton<MigrationRunner>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IMetricsRepository, MetricsRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<MigrationRunner>();
    await runner.RunAsync();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trade Order Service v1"));

app.UseHttpsRedirection();
app.UseCors(DashboardCors);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health/live", () => Results.Ok(new { status = "alive" }));
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" }));

app.Run();
