using Portfolio.Service.Configuration;
using Portfolio.Service.Sync;
using Serilog;
using Serilog.Formatting.Compact;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, cfg) => cfg
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter()))
    .ConfigureServices((ctx, services) =>
    {
        var dbOptions = ctx.Configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("Database configuration is missing.");

        var rabbitOptions = ctx.Configuration
            .GetSection(RabbitMqOptions.SectionName)
            .Get<RabbitMqOptions>()
            ?? throw new InvalidOperationException("RabbitMq configuration is missing.");

        services.AddSingleton(dbOptions);
        services.AddSingleton(rabbitOptions);
        services.AddHostedService<OutboxRelay>();
    })
    .Build();

await host.RunAsync();
