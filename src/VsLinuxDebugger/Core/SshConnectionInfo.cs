namespace VsLinuxDebugger.Core
{
  public struct SshConnectionInfo
  {
    public string Host { get; set; }

    public int Port { get; set; }

    /// <summary>User's Group (if applicable).</summary>
    public string UserGroup { get; set; }

    public string UserName { get; set; }

    public string UserPass { get; set; }

    public bool PrivateKeyEnabled { get; set; }

    public string PrivateKeyPath { get; set; }

    public string PrivateKeyPassword { get; set; }

    /// <summary>Explicit path to an OpenSSH certificate file (i.e. a CA-signed
    /// '&lt;key&gt;-cert.pub'). When blank, the certificate is looked up next to
    /// <see cref="PrivateKeyPath"/> using the OpenSSH naming convention.</summary>
    public string CertificatePath { get; set; }
  }
}
