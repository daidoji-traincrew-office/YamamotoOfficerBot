namespace YamamotoOfficerBot;

public static class Messages
{
    public const string NoPermission = "このコマンドを実行する権限がありません";
    public const string AlreadyHasDuty = "既に担務ロールが付与されています";
    public const string NoDutyToRemove = "担務ロールは付与されていません";
    public const string DutyAssigned = "担務ロールを付与しました";
    public const string DutyRemoved = "担務ロールを解除しました";

    // Role operation error messages
    public const string RoleNotFound = "ロールが見つかりませんでした";
    public const string MissingPermissions = "ボットに必要な権限がありません";
    public const string NetworkError = "ネットワークエラーが発生しました";
    public const string RateLimited = "レート制限されています。しばらく待ってから再試行してください";
    public const string Unknown = "予期しないエラーが発生しました";

    public static string RequiredRolesMissing(IEnumerable<string> roleNames)
        => $"{string.Join(" または ", roleNames)} が必要です";
}
