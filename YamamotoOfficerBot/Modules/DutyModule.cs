using Discord;
using Discord.Interactions;
using YamamotoOfficerBot.Services;

namespace YamamotoOfficerBot.Modules;

public class DutyModule(RoleService roleService) : InteractionModuleBase<SocketInteractionContext>
{
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
        // Defer the response to prevent timeout
        await DeferAsync(ephemeral: true);
        if (Context.User is not IGuildUser user || !roleService.HasAdministratorRole(user))
        {
            await RespondAsync(Messages.NoPermission, ephemeral: true);
            return;
        }

        var components = new ComponentBuilder()
            .WithButton("出勤", $"duty_assign_{dutyType}", ButtonStyle.Success)
            .WithButton("退勤", $"duty_remove_{dutyType}", ButtonStyle.Danger)
            .Build();

        // Delete the deferred response
        await DeleteOriginalResponseAsync();

        // Send components in a separate message to the same channel
        await Context.Channel.SendMessageAsync(
            $"{dutyName}\n以下のボタンを押して出勤・退勤できます。",
            components: components);
    }
}
