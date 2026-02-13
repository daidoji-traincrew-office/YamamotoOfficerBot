using System.Net;
using Discord;
using Discord.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YamamotoOfficerBot.Exceptions;
using YamamotoOfficerBot.Models;

namespace YamamotoOfficerBot.Services;

public class RoleService(IOptions<RolesConfig> rolesConfig, ILogger<RoleService> logger)
{
    private readonly RolesConfig _rolesConfig = rolesConfig.Value;

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
        try
        {
            await user.AddRoleAsync(dutyConfig.DutyRoleId);
        }
        catch (HttpException ex)
        {
            logger.LogError(ex, "Failed to assign role {RoleId} to user {UserId}. Status: {StatusCode}",
                dutyConfig.DutyRoleId, user.Id, ex.HttpCode);

            var errorType = ex.HttpCode switch
            {
                HttpStatusCode.NotFound => RoleOperationErrorType.RoleNotFound,
                HttpStatusCode.Forbidden => RoleOperationErrorType.MissingPermissions,
                HttpStatusCode.TooManyRequests => RoleOperationErrorType.RateLimited,
                _ => RoleOperationErrorType.Unknown
            };

            throw new RoleOperationException(errorType);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Timeout while assigning role {RoleId} to user {UserId}",
                dutyConfig.DutyRoleId, user.Id);
            throw new RoleOperationException(RoleOperationErrorType.NetworkError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while assigning role {RoleId} to user {UserId}",
                dutyConfig.DutyRoleId, user.Id);
            throw new RoleOperationException(RoleOperationErrorType.Unknown);
        }
    }

    /// <summary>
    /// 担務ロールを解除
    /// </summary>
    public async Task RemoveDutyRole(IGuildUser user, DutyConfig dutyConfig)
    {
        try
        {
            await user.RemoveRoleAsync(dutyConfig.DutyRoleId);
        }
        catch (HttpException ex)
        {
            logger.LogError(ex, "Failed to remove role {RoleId} from user {UserId}. Status: {StatusCode}",
                dutyConfig.DutyRoleId, user.Id, ex.HttpCode);

            var errorType = ex.HttpCode switch
            {
                HttpStatusCode.NotFound => RoleOperationErrorType.RoleNotFound,
                HttpStatusCode.Forbidden => RoleOperationErrorType.MissingPermissions,
                HttpStatusCode.TooManyRequests => RoleOperationErrorType.RateLimited,
                _ => RoleOperationErrorType.Unknown
            };

            throw new RoleOperationException(errorType);
        }
        catch (TimeoutException ex)
        {
            logger.LogError(ex, "Timeout while removing role {RoleId} from user {UserId}",
                dutyConfig.DutyRoleId, user.Id);
            throw new RoleOperationException(RoleOperationErrorType.NetworkError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while removing role {RoleId} from user {UserId}",
                dutyConfig.DutyRoleId, user.Id);
            throw new RoleOperationException(RoleOperationErrorType.Unknown);
        }
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
