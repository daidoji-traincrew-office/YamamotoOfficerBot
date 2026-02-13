using System.Net;
using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DiscordConfig = YamamotoOfficerBot.Models.DiscordConfig;

namespace YamamotoOfficerBot.Services;

public class BotHostedService(
    DiscordSocketClient client,
    InteractionService interactionService,
    IOptions<DiscordConfig> discordConfig,
    IServiceProvider serviceProvider,
    ILogger<BotHostedService> logger,
    IHostApplicationLifetime applicationLifetime)
    : IHostedService
{
    private readonly DiscordConfig _discordConfig = discordConfig.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BotHostedService starting...");

        try
        {
            // Readyイベントにハンドラ登録
            client.Ready += OnReadyAsync;

            // インタラクションイベントにハンドラ登録
            client.InteractionCreated += HandleInteractionAsync;

            // ログイン
            await client.LoginAsync(TokenType.Bot, _discordConfig.Token);
            await client.StartAsync();

            logger.LogInformation("Discord client started");
        }
        catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.Unauthorized)
        {
            logger.LogCritical(ex, "Invalid Discord bot token. Please check your configuration.");
            applicationLifetime.StopApplication();
        }
        catch (HttpException ex) when (ex.HttpCode == HttpStatusCode.TooManyRequests)
        {
            logger.LogCritical(ex, "Rate limited by Discord. Please try again later.");
            applicationLifetime.StopApplication();
        }
        catch (TimeoutException ex)
        {
            logger.LogCritical(ex, "Network timeout while connecting to Discord. Please check your network connection.");
            applicationLifetime.StopApplication();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to start Discord bot. Unexpected error occurred.");
            applicationLifetime.StopApplication();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("BotHostedService stopping...");

        await client.StopAsync();
        await client.LogoutAsync();

        logger.LogInformation("Discord client stopped");
    }

    private async Task OnReadyAsync()
    {
        logger.LogInformation("Discord client is ready. Guild ID: {GuildId}", _discordConfig.GuildId);

        try
        {
            // モジュールを登録
            await interactionService.AddModulesAsync(
                assembly: typeof(BotHostedService).Assembly,
                services: serviceProvider);

            // Guildコマンドを登録
            await interactionService.RegisterCommandsToGuildAsync(_discordConfig.GuildId);

            logger.LogInformation("Slash commands registered to guild {GuildId}", _discordConfig.GuildId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register slash commands");
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try
        {
            // インタラクションコンテキストを作成
            var context = new SocketInteractionContext(client, interaction);

            // コマンドを実行
            await interactionService.ExecuteCommandAsync(context, serviceProvider);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling interaction");

            // エラー時のレスポンス
            if (interaction.Type == InteractionType.ApplicationCommand)
            {
                await interaction.RespondAsync("コマンドの実行中にエラーが発生しました", ephemeral: true);
            }
        }
    }
}
