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
            await RespondAsync("無効な担務タイプです", ephemeral: true);
            return;
        }

        // 資格チェック
        if (!roleService.HasRequiredRole(user, dutyConfig))
        {
            var roleNames = roleService.GetRequiredRoleNames(Context.Guild, dutyConfig);
            await RespondAsync(Messages.RequiredRolesMissing(roleNames), ephemeral: true);
            return;
        }

        // 既に担務を持っているかチェック
        if (roleService.HasDutyRole(user, dutyConfig))
        {
            await RespondAsync(Messages.AlreadyHasDuty, ephemeral: true);
            return;
        }

        // 担務ロール付与
        try
        {
            await roleService.AssignDutyRole(user, dutyConfig);
            await RespondAsync(Messages.DutyAssigned, ephemeral: true);
        }
        catch (RoleOperationException ex)
        {
            logger.LogError(ex, "Failed to assign duty {DutyType} to user {UserId}. ErrorType: {ErrorType}",
                dutyType, user.Id, ex.ErrorType);
            await RespondAsync(ex.UserMessage, ephemeral: true);
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
            await RespondAsync("無効な担務タイプです", ephemeral: true);
            return;
        }

        // 担務を持っているかチェック
        if (!roleService.HasDutyRole(user, dutyConfig))
        {
            await RespondAsync(Messages.NoDutyToRemove, ephemeral: true);
            return;
        }

        // 担務ロール解除
        try
        {
            await roleService.RemoveDutyRole(user, dutyConfig);
            await RespondAsync(Messages.DutyRemoved, ephemeral: true);
        }
        catch (RoleOperationException ex)
        {
            logger.LogError(ex, "Failed to remove duty {DutyType} from user {UserId}. ErrorType: {ErrorType}",
                dutyType, user.Id, ex.ErrorType);
            await RespondAsync(ex.UserMessage, ephemeral: true);
        }
    }
}
