using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using Microsoft.VisualStudio.Shell;

namespace Xeno.VsLinuxDebug.OptionsPages
{
  /// <summary>Remote SSH authentication. Shows either the password field or the private-key
  /// fields, depending on <see cref="UserPrivateKeyEnabled"/> (enforced in the WPF UI via
  /// visibility bindings, not by hiding properties from a PropertyGrid).</summary>
  public class RemoteCredentialsOptionsPage : UIElementDialogPage, INotifyPropertyChanged
  {
    private string _userName = "pi";
    private bool _userPrivateKeyEnabled = false;
    private string _userPass = "raspberry";
    private string _userPrivateKeyPath = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
      ".ssh\\id_rsa");
    private string _userPrivateKeyPassword = "";
    private string _userCertificatePath = "";

    public event PropertyChangedEventHandler PropertyChanged;

    [Category("Remote Credentials")]
    [DisplayName("User Name")]
    [Description("SSH User name on remote machine.")]
    public string UserName
    {
      get => _userName;
      set { _userName = value; OnPropertyChanged(nameof(UserName)); }
    }

    [Category("Remote Credentials")]
    [DisplayName("SSH Key File Enabled")]
    [Description(
      "Use an SSH private key instead of a password. PLINK only accepts '.ppk' keys, not " +
      "this OpenSSH-format one, so the debugger connection needs 'Use SSH.exe' enabled " +
      "(Options > Local) to use it.")]
    public bool UserPrivateKeyEnabled
    {
      get => _userPrivateKeyEnabled;
      set { _userPrivateKeyEnabled = value; OnPropertyChanged(nameof(UserPrivateKeyEnabled)); }
    }

    [Category("Remote Credentials")]
    [DisplayName("User Password")]
    [Description("SSH Password on remote machine.")]
    public string UserPass
    {
      get => _userPass;
      set { _userPass = value; OnPropertyChanged(nameof(UserPass)); }
    }

    [Category("Remote Credentials")]
    [DisplayName("SSH Private Key File (optional)")]
    [Description("Path to an OpenSSH-format private key file.")]
    public string UserPrivateKeyPath
    {
      get => _userPrivateKeyPath;
      set { _userPrivateKeyPath = value; OnPropertyChanged(nameof(UserPrivateKeyPath)); }
    }

    [Category("Remote Credentials")]
    [DisplayName("SSH Private Key Password (optional)")]
    [Description("Private key password (only if it was set).")]
    public string UserPrivateKeyPassword
    {
      get => _userPrivateKeyPassword;
      set { _userPrivateKeyPassword = value; OnPropertyChanged(nameof(UserPrivateKeyPassword)); }
    }

    [Category("Remote Credentials")]
    [DisplayName("SSH Certificate File (optional)")]
    [Description(
      "Path to a CA-signed '<key>-cert.pub' certificate. Leave blank to auto-detect it " +
      "next to the private key.")]
    public string UserCertificatePath
    {
      get => _userCertificatePath;
      set { _userCertificatePath = value; OnPropertyChanged(nameof(UserCertificatePath)); }
    }

    /*[Category(Credientials)]
    [DisplayName("PLINK PPK Key File Enabled")]
    [Description("Use SSH Key for connecting to remote machine.")]
    public bool UserPlinkPrivateKeyEnabled { get; set; } = false;

    [Category(Credientials)]
    [DisplayName("SSH Private Key File (optional)")]
    [Description("Private key file.")]
    public string UserPlinkPrivateKeyPath { get; set; } = Path.Combine(
      Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
      ".ssh\\id_rsa");
    */

    protected override UIElement Child => new RemoteCredentialsOptionsControl(this);

    private void OnPropertyChanged(string propertyName)
      => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }
}
