namespace YamamotoOfficerBot.Models;

public class DutyConfig
{
    public ulong DutyRoleId { get; set; }
    public List<ulong> RequiredRoleIds { get; set; } = new();
}
