using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamamotoOfficerBot.Exceptions;
using YamamotoOfficerBot.Models;
using YamamotoOfficerBot.Services;

namespace YamamotoOfficerBot.Handlers;

public class ButtonHandler(
    RoleService roleService,
    IOptions<Dictionary<string, DutyConfig>> dutyConfigs,
    ILogger<ButtonHandler> logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    private readonly Dictionary<string, DutyConfig> _dutyConfigs = dutyConfigs.Value;

    private async Task RespondEphemeralAsync(string message)
    {
        await RespondAsync(message, ephemeral: true);
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await DeleteOriginalResponseAsync();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to delete ephemeral message");
            }
        });
    }

    [ComponentInteraction("duty_assign_*", ignoreGroupNames: true)]
    public async Task HandleAssignButtonAsync()
    {
        if (Context.Interaction is not IComponentInteraction interaction  
           || Context.User is not IGuildUser user)
        {
            return;
        }

        var customId = interaction.Data.CustomId;
        var dutyType = customId.Replace("duty_assign_", "");

        if (!_dutyConfigs.TryGetValue(dutyType, out var dutyConfig))
        {
            await RespondEphemeralAsync("無効な担務タイプです");
            return;
        }

        // 資格チェック
        if (!roleService.HasRequiredRole(user, dutyConfig))
        {
            var roleNames = roleService.GetRequiredRoleNames(Context.Guild, dutyConfig);
            await RespondEphemeralAsync(Messages.RequiredRolesMissing(roleNames));
            return;
        }

        // 既に担務を持っているかチェック
        if (roleService.HasDutyRole(user, dutyConfig))
        {
            await RespondEphemeralAsync(Messages.AlreadyHasDuty);
            return;
        }

        // 担務ロール付与
        try
        {
            await roleService.AssignDutyRole(user, dutyConfig);
            await RespondEphemeralAsync(Messages.DutyAssigned);
        }
        catch (RoleOperationException ex)
        {
            logger.LogError(ex, "Failed to assign duty {DutyType} to user {UserId}. ErrorType: {ErrorType}",
                dutyType, user.Id, ex.ErrorType);
            await RespondEphemeralAsync(ex.UserMessage);
        }
    }

    [ComponentInteraction("beginner_assign", ignoreGroupNames: true)]
    public async Task HandleBeginnerAssignAsync()
    {
        if (Context.User is not IGuildUser user)
        {
            return;
        }

        if (roleService.HasBeginnerRole(user))
        {
            await RespondEphemeralAsync(Messages.BeginnerAlreadyAssigned);
            return;
        }

        try
        {
            await roleService.AssignBeginnerRole(user);
            await RespondAsync(Messages.BeginnerAssigned, ephemeral: true);
        }
        catch (RoleOperationException ex)
        {
            logger.LogError(ex, "Failed to assign beginner role to user {UserId}. ErrorType: {ErrorType}",
                user.Id, ex.ErrorType);
            await RespondEphemeralAsync(ex.UserMessage);
        }
    }

    [ComponentInteraction("duty_remove_*", ignoreGroupNames: true)]
    public async Task HandleRemoveButtonAsync()
    {
        if (Context.Interaction is not IComponentInteraction interaction
            || Context.User is not IGuildUser user)
        {
            return;
        }

        var customId = interaction.Data.CustomId;
        var dutyType = customId.Replace("duty_remove_", "");

        if (!_dutyConfigs.TryGetValue(dutyType, out var dutyConfig))
        {
            await RespondEphemeralAsync("無効な担務タイプです");
            return;
        }

        // 担務を持っているかチェック
        if (!roleService.HasDutyRole(user, dutyConfig))
        {
            await RespondEphemeralAsync(Messages.NoDutyToRemove);
            return;
        }

        // 担務ロール解除
        try
        {
            await roleService.RemoveDutyRole(user, dutyConfig);
            await RespondEphemeralAsync(Messages.DutyRemoved);
        }
        catch (RoleOperationException ex)
        {
            logger.LogError(ex, "Failed to remove duty {DutyType} from user {UserId}. ErrorType: {ErrorType}",
                dutyType, user.Id, ex.ErrorType);
            await RespondEphemeralAsync(ex.UserMessage);
        }
    }
}
