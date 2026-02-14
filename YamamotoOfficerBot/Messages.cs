namespace YamamotoOfficerBot;

public static class Messages
{
    public const string NoPermission = "このコマンドを実行する権限がありません";
    public const string AlreadyHasDuty = "既に出勤済みです(担務ロールが付与されています)";
    public const string NoDutyToRemove = "既に退勤済み・出勤していません(担務ロールが付与されていません)";
    public const string DutyAssigned = "出勤しました！今日も一日ご安全に！";
    public const string DutyRemoved = "退勤しました！今日も一日お疲れ様でした！";

    // Role operation error messages
    public const string RoleNotFound = "ロールが見つかりませんでした";
    public const string MissingPermissions = "ボットに必要な権限がありません";
    public const string NetworkError = "ネットワークエラーが発生しました";
    public const string RateLimited = "レート制限されています。しばらく待ってから再試行してください";
    public const string Unknown = "予期しないエラーが発生しました";

    public static string RequiredRolesMissing(IEnumerable<string> roleNames)
        => $"{string.Join(" または ", roleNames)} が必要です";
}
