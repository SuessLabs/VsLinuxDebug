using System;
using System.Reflection;
using System.Security.Cryptography;
using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;
using Renci.SshNet.Security.Cryptography.Ciphers;

namespace VsLinuxDebugger.Core.Security
{
  /// <summary>
  /// Utility class which allows ssh.net to connect to servers using ras-sha2-256.
  /// </summary>
  public static class RsaSha256Util
  {
    /// <summary>RSA SHA2 256.</summary>
    public const string RSA_SHA2_256 = "rsa-sha2-256";

    /// <summary>
    /// Converts key file to rsa key with sha2-256 signature
    /// Due to lack of constructor: https://github.com/sshnet/SSH.NET/blob/bc99ada7da3f05f50d9379f2644941d91d5bf05a/src/Renci.SshNet/PrivateKeyFile.cs#L86
    /// We do that in place
    /// </summary>
    /// <param name="keyFile">Private key file.</param>
    /// <exception cref="ArgumentNullException">Argument Null Exception.</exception>
    public static void ConvertToKeyWithSha256Signature(PrivateKeyFile keyFile)
    {
      var oldKeyHostAlgorithm = keyFile.HostKey as KeyHostAlgorithm;
      if (oldKeyHostAlgorithm == null)
      {
        throw new ArgumentNullException(nameof(oldKeyHostAlgorithm));
      }

      var oldRsaKey = oldKeyHostAlgorithm.Key as RsaKey;
      if (oldRsaKey == null)
      {
        throw new ArgumentNullException(nameof(oldRsaKey));
      }

      var newRsaKey = new RsaWithSha256SignatureKey(oldRsaKey.Modulus, oldRsaKey.Exponent, oldRsaKey.D, oldRsaKey.P, oldRsaKey.Q,
          oldRsaKey.InverseQ);

      UpdatePrivateKeyFile(keyFile, newRsaKey);
    }

    public static void SetupConnection(ConnectionInfo connection)
    {
      connection.HostKeyAlgorithms[RSA_SHA2_256] = data => new KeyHostAlgorithm(RSA_SHA2_256, new RsaKey(), data);
    }

    private static void UpdatePrivateKeyFile(PrivateKeyFile keyFile, RsaWithSha256SignatureKey key)
    {
      var keyHostAlgorithm = new KeyHostAlgorithm(key.ToString(), key);

      var hostKeyProperty = typeof(PrivateKeyFile).GetProperty(nameof(PrivateKeyFile.HostKey));
      hostKeyProperty.SetValue(keyFile, keyHostAlgorithm);

      var keyField = typeof(PrivateKeyFile).GetField("_key", BindingFlags.NonPublic | BindingFlags.Instance);
      keyField.SetValue(keyFile, key);
    }
  }
}
