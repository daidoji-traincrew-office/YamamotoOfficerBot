using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamamotoOfficerBot.Models;

namespace YamamotoOfficerBot.Scheduler;

public class DutyResetScheduler : Scheduler
{
    private static readonly TimeZoneInfo JstTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Tokyo");
    private const int ResetHour = 4;

    private readonly DiscordSocketClient _client;
    private readonly Dictionary<string, DutyConfig> _dutyConfigs;
    private readonly ulong _guildId;

    protected override int Interval => CalculateIntervalUntilNextReset();

    public DutyResetScheduler(
        IServiceScopeFactory serviceScopeFactory,
        DiscordSocketClient client,
        IOptions<Dictionary<string, DutyConfig>> dutyConfigs,
        IOptions<DiscordConfig> discordConfig)
        : base(serviceScopeFactory)
    {
        _client = client;
        _dutyConfigs = dutyConfigs.Value;
        _guildId = discordConfig.Value.GuildId;
    }

    private int CalculateIntervalUntilNextReset()
    {
        var nowUtc = DateTime.UtcNow;
        var nowJst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, JstTimeZone);

        var nextResetJst = nowJst.Date.AddHours(ResetHour);
        if (nowJst >= nextResetJst)
        {
            nextResetJst = nextResetJst.AddDays(1);
        }

        var nextResetUtc = TimeZoneInfo.ConvertTimeToUtc(nextResetJst, JstTimeZone);
        var interval = (int)(nextResetUtc - nowUtc).TotalMilliseconds;

        return Math.Max(interval, 1);
    }

    protected override async Task ExecuteTaskAsync(IServiceScope scope)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<DutyResetScheduler>();

        var guild = _client.GetGuild(_guildId);
        if (guild == null)
        {
            logger.LogError("Guild with ID {GuildId} not found.", _guildId);
            return;
        }

        var dutyRoleIds = _dutyConfigs.Values.Select(c => c.DutyRoleId).ToHashSet();

        foreach (var roleId in dutyRoleIds)
        {
            var role = guild.GetRole(roleId);
            if (role == null)
            {
                logger.LogWarning("Role with ID {RoleId} not found.", roleId);
                continue;
            }

            var membersWithRole = guild.Users.Where(u => u.Roles.Any(r => r.Id == roleId)).ToList();

            foreach (var member in membersWithRole)
            {
                try
                {
                    await member.RemoveRoleAsync(role);
                    logger.LogInformation("Removed role {RoleName} from user {UserName}.", role.Name, member.DisplayName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to remove role {RoleName} from user {UserName}.", role.Name, member.DisplayName);
                }
            }
        }

        logger.LogInformation("Duty reset completed at {Time} JST.", TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, JstTimeZone));
    }
}
