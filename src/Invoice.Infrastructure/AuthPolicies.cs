namespace Invoice.Infrastructure;

public static class AuthPolicies
{
    /// <summary>
    /// Regular business endpoints (customers/invoices/reports/chat): any
    /// authenticated user except the admin account, which only manages users.
    /// </summary>
    public const string NotAdmin = "NotAdmin";
}
