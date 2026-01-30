namespace YamamotoOfficerBot;

public static class Messages
{
    public const string NoPermission = "このコマンドを実行する権限がありません";
    public const string AlreadyHasDuty = "既に担務ロールが付与されています";
    public const string NoDutyToRemove = "担務ロールは付与されていません";
    public const string DutyAssigned = "担務ロールを付与しました";
    public const string DutyRemoved = "担務ロールを解除しました";

    public static string RequiredRolesMissing(IEnumerable<string> roleNames)
        => $"{string.Join(" または ", roleNames)} が必要です";
}
