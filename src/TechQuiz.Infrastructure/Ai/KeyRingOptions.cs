namespace TechQuiz.Infrastructure.Ai;

/// <summary>
/// Binding for the <c>DataProtection:*</c> section: how the Data Protection key ring that
/// encrypts users' AI keys (ADR-006) is protected at rest. The ring itself is always stored in
/// the database (ADR-028); these settings encrypt it with an X.509 certificate so that a copy of
/// the database alone cannot decrypt the stored keys.
/// </summary>
public sealed class KeyRingOptions
{
    public const string SectionName = "DataProtection";

    /// <summary>
    /// When true, startup fails without a certificate. On in the deployed environments, off in
    /// Development (local work and tests use an unencrypted ring in the local database).
    /// </summary>
    public bool RequireCertificate { get; set; }

    /// <summary>Base64-encoded PKCS#12 (.pfx) certificate. Secret: set as an environment variable.</summary>
    public string? CertificateBase64 { get; set; }

    /// <summary>Password for <see cref="CertificateBase64"/>. Secret: set as an environment variable.</summary>
    public string? CertificatePassword { get; set; }
}
