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
  }
}
