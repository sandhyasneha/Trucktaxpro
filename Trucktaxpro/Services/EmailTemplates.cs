namespace Trucktaxpro.Services;

public static class EmailTemplates
{
    public static string Welcome(string firstNameOrEmail) => $@"
<!DOCTYPE html>
<html>
<body style=""margin:0;padding:0;background:#F6F3EC;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F6F3EC;padding:40px 0;"">
    <tr><td align=""center"">
      <table width=""480"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:8px;overflow:hidden;"">
        <tr><td style=""background:#12283D;padding:28px 32px;"">
          <span style=""font-family:Georgia,serif;font-weight:bold;font-size:22px;color:#ffffff;letter-spacing:0.03em;"">
            TRUCK<span style=""color:#F2A93B;"">TAX</span>PRO
          </span>
        </td></tr>
        <tr><td style=""padding:36px 32px 24px;"">
          <h1 style=""font-size:20px;color:#12283D;margin:0 0 16px;"">Welcome aboard, {firstNameOrEmail}!</h1>
          <p style=""font-size:15px;color:#4C5C6B;line-height:1.6;margin:0 0 20px;"">
            Your TruckTaxPro account is ready. You can now file your Form 2290, submit amendments, or correct a VIN — all backed by IRS-authorized e-filing.
          </p>
          <table cellpadding=""0"" cellspacing=""0"">
            <tr><td style=""background:#F2A93B;border-radius:4px;"">
              <a href=""https://trucktaxpro.com"" style=""display:inline-block;padding:13px 28px;color:#8A5A0F;font-weight:bold;font-size:14px;text-decoration:none;text-transform:uppercase;letter-spacing:0.04em;"">
                File Your 2290
              </a>
            </td></tr>
          </table>
        </td></tr>
        <tr><td style=""background:#1B2126;padding:24px 32px;"">
          <p style=""color:#9AA6AE;font-size:12px;margin:0;"">
            © 2026 TruckTaxPro. IRS-authorized e-file provider. · US-based support
          </p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";


 public static string ResetPassword(string resetLink) => $@"
<!DOCTYPE html>
<html>
<body style=""margin:0;padding:0;background:#F6F3EC;font-family:'Segoe UI',Arial,sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#F6F3EC;padding:40px 0;"">
    <tr><td align=""center"">
      <table width=""480"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff;border-radius:8px;overflow:hidden;"">
        <tr><td style=""background:#12283D;padding:28px 32px;"">
          <span style=""font-family:Georgia,serif;font-weight:bold;font-size:22px;color:#ffffff;letter-spacing:0.03em;"">
            TRUCK<span style=""color:#F2A93B;"">TAX</span>PRO
          </span>
        </td></tr>
        <tr><td style=""padding:36px 32px 24px;"">
          <h1 style=""font-size:20px;color:#12283D;margin:0 0 16px;"">Reset your password</h1>
          <p style=""font-size:15px;color:#4C5C6B;line-height:1.6;margin:0 0 20px;"">
            We received a request to reset your TruckTaxPro password. Click below to choose a new one. This link expires shortly for your security — if you didn't request this, you can safely ignore this email.
          </p>
          <table cellpadding=""0"" cellspacing=""0"">
            <tr><td style=""background:#F2A93B;border-radius:4px;"">
              <a href=""{resetLink}"" style=""display:inline-block;padding:13px 28px;color:#8A5A0F;font-weight:bold;font-size:14px;text-decoration:none;text-transform:uppercase;letter-spacing:0.04em;"">
                Reset Password
              </a>
            </td></tr>
          </table>
        </td></tr>
        <tr><td style=""background:#1B2126;padding:24px 32px;"">
          <p style=""color:#9AA6AE;font-size:12px;margin:0;"">
            © 2026 TruckTaxPro. IRS-authorized e-file provider. · US-based support
          </p>
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
}
