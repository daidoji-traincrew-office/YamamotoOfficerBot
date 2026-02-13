using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using YamamotoOfficerBot.Models;
using YamamotoOfficerBot.Scheduler;
using YamamotoOfficerBot.Services;
using DiscordConfig = YamamotoOfficerBot.Models.DiscordConfig;

var builder = Host.CreateApplicationBuilder(args);

// 設定読み込み
builder.Services.Configure<DiscordConfig>(builder.Configuration.GetSection("Discord"));
builder.Services.Configure<Dictionary<string, DutyConfig>>(builder.Configuration.GetSection("Duty"));
builder.Services.Configure<RolesConfig>(builder.Configuration.GetSection("Roles"));

// Discord.Net - DiscordSocketClient with GatewayIntents
builder.Services.AddSingleton(_ => new DiscordSocketClient(new DiscordSocketConfig
{
    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMembers
}));

// Discord.Net - InteractionService
builder.Services.AddSingleton(sp =>
    new InteractionService(sp.GetRequiredService<DiscordSocketClient>()));

// Services
builder.Services.AddSingleton<RoleService>();
builder.Services.AddSingleton<DutyResetScheduler>();

// HostedService
builder.Services.AddHostedService<BotHostedService>();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new(
                builder.Configuration["OpenTelemetry:OtlpEndpoint"]
                ?? "http://localhost:4317");
        }));

var host = builder.Build();
await host.RunAsync();
