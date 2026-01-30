using Discord;
using Discord.Interactions;
using Microsoft.Extensions.Options;
using YamamotoOfficerBot.Models;
using YamamotoOfficerBot.Services;

namespace YamamotoOfficerBot.Handlers;

public class ButtonHandler : InteractionModuleBase<SocketInteractionContext>
{
    private readonly RoleService _roleService;
    private readonly Dictionary<string, DutyConfig> _dutyConfigs;

    public ButtonHandler(RoleService roleService, IOptions<Dictionary<string, DutyConfig>> dutyConfigs)
    {
        _roleService = roleService;
        _dutyConfigs = dutyConfigs.Value;
    }

    [ComponentInteraction("duty_assign_*", ignoreGroupNames: true)]
    public async Task HandleAssignButtonAsync()
    {
        var customId = ((IComponentInteraction)Context.Interaction).Data.CustomId;
        var dutyType = customId.Replace("duty_assign_", "");

        if (!_dutyConfigs.TryGetValue(dutyType, out var dutyConfig))
        {
            await RespondAsync("無効な担務タイプです", ephemeral: true);
            return;
        }

        var user = (IGuildUser)Context.User;

        // 資格チェック
        if (!_roleService.HasRequiredRole(user, dutyConfig))
        {
            var roleNames = _roleService.GetRequiredRoleNames(Context.Guild, dutyConfig);
            await RespondAsync(Messages.RequiredRolesMissing(roleNames), ephemeral: true);
            return;
        }

        // 既に担務を持っているかチェック
        if (_roleService.HasDutyRole(user, dutyConfig))
        {
            await RespondAsync(Messages.AlreadyHasDuty, ephemeral: true);
            return;
        }

        // 担務ロール付与
        await _roleService.AssignDutyRole(user, dutyConfig);
        await RespondAsync(Messages.DutyAssigned, ephemeral: true);
    }

    [ComponentInteraction("duty_remove_*", ignoreGroupNames: true)]
    public async Task HandleRemoveButtonAsync()
    {
        var customId = ((IComponentInteraction)Context.Interaction).Data.CustomId;
        var dutyType = customId.Replace("duty_remove_", "");

        if (!_dutyConfigs.TryGetValue(dutyType, out var dutyConfig))
        {
            await RespondAsync("無効な担務タイプです", ephemeral: true);
            return;
        }

        var user = (IGuildUser)Context.User;

        // 担務を持っているかチェック
        if (!_roleService.HasDutyRole(user, dutyConfig))
        {
            await RespondAsync(Messages.NoDutyToRemove, ephemeral: true);
            return;
        }

        // 担務ロール解除
        await _roleService.RemoveDutyRole(user, dutyConfig);
        await RespondAsync(Messages.DutyRemoved, ephemeral: true);
    }
}
