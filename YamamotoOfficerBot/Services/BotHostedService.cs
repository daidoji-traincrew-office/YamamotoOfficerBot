using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DiscordConfig = YamamotoOfficerBot.Models.DiscordConfig;

namespace YamamotoOfficerBot.Services;

public class BotHostedService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly DiscordConfig _discordConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotHostedService> _logger;

    public BotHostedService(
        DiscordSocketClient client,
        InteractionService interactionService,
        IOptions<DiscordConfig> discordConfig,
        IServiceProvider serviceProvider,
        ILogger<BotHostedService> logger)
    {
        _client = client;
        _interactionService = interactionService;
        _discordConfig = discordConfig.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BotHostedService starting...");

        // Readyイベントにハンドラ登録
        _client.Ready += OnReadyAsync;

        // ログイン
        await _client.LoginAsync(TokenType.Bot, _discordConfig.Token);
        await _client.StartAsync();

        _logger.LogInformation("Discord client started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("BotHostedService stopping...");

        await _client.StopAsync();
        await _client.LogoutAsync();

        _logger.LogInformation("Discord client stopped");
    }

    private async Task OnReadyAsync()
    {
        _logger.LogInformation("Discord client is ready. Guild ID: {GuildId}", _discordConfig.GuildId);

        try
        {
            // モジュールを登録
            await _interactionService.AddModulesAsync(
                assembly: typeof(BotHostedService).Assembly,
                services: _serviceProvider);

            // Guildコマンドを登録
            await _interactionService.RegisterCommandsToGuildAsync(_discordConfig.GuildId);

            _logger.LogInformation("Slash commands registered to guild {GuildId}", _discordConfig.GuildId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register slash commands");
        }
    }
}
