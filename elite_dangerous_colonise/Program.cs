using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.DataProtection;
using Npgsql;
using elite_dangerous_colonise.Classes;
using elite_dangerous_colonise.Models.Database_Types;
using System.Text;
using Ixnas.AltchaNet;

var builder = WebApplication.CreateBuilder(args);

string rootDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), @"..\"));

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
    .AddJsonFile(rootDir + "\\private\\Secrets.json", optional: false, reloadOnChange: true);

bool debugEnabled = builder.Configuration.GetValue<bool>("EnableDebugLauncher", false);

builder.Services.Configure<UpdateTimeOptions>(builder.Configuration.GetSection("UpdateTime"));

// Adds the RazorPages service.
builder.Services.AddRazorPages();

// Adds the Controllers service.
builder.Services.AddControllers();

// Adds the SignalR service.
builder.Services.AddSignalR();

// Adds the MemoryCache service.
builder.Services.AddDistributedMemoryCache();

// Adds the Session service.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configures the data protection service.
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(rootDir + @"\private\keys"))
    .SetApplicationName("EDColonise");

// Configures Serilog.
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.File(rootDir + @"\logs\errorLog.txt",
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj} (RequestId: {RequestId}) {NewLine}{Exception}",
        restrictedToMinimumLevel: LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
    .WriteTo.File(rootDir + @"\logs\infoLog.txt",
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] ({SourceContext}) {Message:lj} {NewLine}{Exception}",
        restrictedToMinimumLevel: LogEventLevel.Information, rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);
builder.Services.AddSingleton<AppLogger>();

// Configures database connection.
string connectionString = builder.Configuration.GetConnectionString("SystemsDatabase")
    ?? throw new InvalidOperationException("Connection string 'SystemsDatabase' not found.");

NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<RingType>("RingType");
dataSourceBuilder.MapEnum<HotspotType>("HotspotType");
dataSourceBuilder.MapEnum<ReserveType>("ReserveType");
dataSourceBuilder.MapEnum<ResultOrderType>("ResultOrderType");
dataSourceBuilder.MapComposite<StarSystemInsertType>("StarSystemInsertType");
dataSourceBuilder.MapComposite<StationInsertType>("StationInsertType");
dataSourceBuilder.MapComposite<RingInsertType>("RingInsertType");
dataSourceBuilder.MapComposite<HotspotInsertType>("HotspotInsertType");
dataSourceBuilder.MapComposite<UncolonisedDetailsInsertType>("UncolonisedDetailsInsertType");
dataSourceBuilder.MapComposite<ColonisableInsertType>("ColonisableInsertType");

NpgsqlDataSource dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

builder.Services.AddScoped<DatabaseBulkWriter>(provider =>
{
    AppLogger logger = provider.GetService<AppLogger>();
    NpgsqlDataSource dataSourceService = provider.GetService<NpgsqlDataSource>();

    return new DatabaseBulkWriter(dataSourceService, logger, debugEnabled);
});

// Configures Altcha service
string altchaKey = builder.Configuration["AltchaKey"];
if (string.IsNullOrEmpty(altchaKey) || Encoding.UTF8.GetByteCount(altchaKey) != 64)
{
    throw new InvalidOperationException("Altcha Key not found or incorrect length.");
}

builder.Services.AddSingleton<AltchaService>(service =>
    Altcha.CreateServiceBuilder()
        .UseSha256(Encoding.UTF8.GetBytes(altchaKey))
        .UseStore(new AltchaMemoryStore())
        .Build()
);

if (!debugEnabled)
{
    // Configures the Spansh download background service.
    builder.Services.AddSingleton<SpanshDataDumpDownloadService>();
    builder.Services.AddHostedService(provider => provider.GetRequiredService<SpanshDataDumpDownloadService>());

    // Configures the staging clearing background service.
    builder.Services.AddHostedService<SystemSummaryStagingClearingService>();

    // Configures the EDDN listening background service.
    builder.Services.AddHostedService<EDDNListeningService>();

    // Configures the self ping background service.
    builder.Services.AddHttpClient();
    builder.Services.AddHostedService<SelfPingService>();

    // Configures the memory reporting background service.
    builder.Services.AddHostedService<MemoryReportingService>();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");

    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.MapControllers();

app.MapHub<UpdateHub>("/updateHub");

if (debugEnabled)
{
    DebugLauncher launcher = new DebugLauncher(app.Services);
    await launcher.Run();
}
else
{
    app.Run();
}