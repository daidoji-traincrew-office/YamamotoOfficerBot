using Discord;
using Discord.Interactions;
using YamamotoOfficerBot.Services;

namespace YamamotoOfficerBot.Modules;

public class DutyModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly RoleService _roleService;

    public DutyModule(RoleService roleService)
    {
        _roleService = roleService;
    }

    [SlashCommand("signal-duty-panel", "信号扱担務パネルを送信します")]
    public async Task SignalDutyPanelAsync()
    {
        await SendDutyPanelAsync("信号扱担務", "SignalHandler");
    }

    [SlashCommand("commander-duty-panel", "司令担務パネルを送信します")]
    public async Task CommanderDutyPanelAsync()
    {
        await SendDutyPanelAsync("司令担務", "Commander");
    }

    [SlashCommand("mood-signal-duty-panel", "気分信号扱担務パネルを送信します")]
    public async Task MoodSignalDutyPanelAsync()
    {
        await SendDutyPanelAsync("気分信号扱担務", "MoodSignalHandler");
    }

    [SlashCommand("instructor-duty-panel", "指導運転士担務パネルを送信します")]
    public async Task InstructorDutyPanelAsync()
    {
        await SendDutyPanelAsync("指導運転士担務", "InstructorDriver");
    }

    private async Task SendDutyPanelAsync(string dutyName, string dutyType)
    {
        var user = Context.User as IGuildUser;
        if (user == null || !_roleService.HasAdministratorRole(user))
        {
            await RespondAsync(Messages.NoPermission, ephemeral: true);
            return;
        }

        var components = new ComponentBuilder()
            .WithButton("付与", $"duty_assign_{dutyType}", ButtonStyle.Success)
            .WithButton("解除", $"duty_remove_{dutyType}", ButtonStyle.Danger)
            .Build();

        await RespondAsync(
            $"{dutyName}\n以下のボタンを押して担務を付与・解除できます。",
            components: components);
    }
}
