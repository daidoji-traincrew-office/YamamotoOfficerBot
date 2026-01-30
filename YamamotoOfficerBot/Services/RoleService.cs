using Discord;
using Microsoft.Extensions.Options;
using YamamotoOfficerBot.Models;

namespace YamamotoOfficerBot.Services;

public class RoleService
{
    private readonly RolesConfig _rolesConfig;

    public RoleService(IOptions<RolesConfig> rolesConfig)
    {
        _rolesConfig = rolesConfig.Value;
    }

    /// <summary>
    /// AdministratorRoleIdsのいずれかを持つか確認
    /// </summary>
    public bool HasAdministratorRole(IGuildUser user)
    {
        return _rolesConfig.AdministratorRoleIds.Any(roleId => user.RoleIds.Contains(roleId));
    }

    /// <summary>
    /// RequiredRoleIdsのいずれかを持つか確認
    /// </summary>
    public bool HasRequiredRole(IGuildUser user, DutyConfig dutyConfig)
    {
        // RequiredRoleIdsが空の場合は誰でも実行可能
        if (dutyConfig.RequiredRoleIds.Count == 0)
        {
            return true;
        }

        return dutyConfig.RequiredRoleIds.Any(roleId => user.RoleIds.Contains(roleId));
    }

    /// <summary>
    /// 担務ロールを持っているか確認
    /// </summary>
    public bool HasDutyRole(IGuildUser user, DutyConfig dutyConfig)
    {
        return user.RoleIds.Contains(dutyConfig.DutyRoleId);
    }

    /// <summary>
    /// 担務ロールを付与
    /// </summary>
    public async Task AssignDutyRole(IGuildUser user, DutyConfig dutyConfig)
    {
        await user.AddRoleAsync(dutyConfig.DutyRoleId);
    }

    /// <summary>
    /// 担務ロールを解除
    /// </summary>
    public async Task RemoveDutyRole(IGuildUser user, DutyConfig dutyConfig)
    {
        await user.RemoveRoleAsync(dutyConfig.DutyRoleId);
    }

    /// <summary>
    /// RequiredRoleIdsのロール名一覧を取得（エラーメッセージ用）
    /// </summary>
    public IEnumerable<string> GetRequiredRoleNames(IGuild guild, DutyConfig dutyConfig)
    {
        foreach (var roleId in dutyConfig.RequiredRoleIds)
        {
            var role = guild.GetRole(roleId);
            if (role != null)
            {
                yield return role.Name;
            }
        }
    }
}
