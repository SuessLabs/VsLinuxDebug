using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using Renci.SshNet.Security;
using Renci.SshNet.Common;
using Renci.SshNet.Security.Cryptography.Ciphers;
using Renci.SshNet.Security.Cryptography;

using System.Security.Cryptography;
using System.Reflection;
namespace VsLinuxDebugger.Core
{
  /// <summary>
  /// Based on https://github.com/sshnet/SSH.NET/blob/1d5d58e17c68a2f319c51e7f938ce6e964498bcc/src/Renci.SshNet/Security/Cryptography/RsaDigitalSignature.cs#L12
  ///
  /// With following changes:
  ///
  /// - OID changed to sha2-256
  /// - hash changed from sha1 to sha2-256
  /// </summary>
  public class RsaSha256DigitalSignature : CipherDigitalSignature, IDisposable
  {
    private HashAlgorithm _hash;

    public RsaSha256DigitalSignature(RsaWithSha256SignatureKey rsaKey)
        // custom OID
        : base(new ObjectIdentifier(2, 16, 840, 1, 101, 3, 4, 2, 1), new RsaCipher(rsaKey))
    {
      // custom
      _hash = SHA256.Create();
    }

    protected override byte[] Hash(byte[] input)
    {
      return _hash.ComputeHash(input);
    }

    private bool _isDisposed;

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (_isDisposed)
        return;

      if (disposing)
      {
        var hash = _hash;
        if (hash != null)
        {
          hash.Dispose();
          _hash = null;
        }

        _isDisposed = true;
      }
    }

    ~RsaSha256DigitalSignature()
    {
      Dispose(false);
    }
  }

  /// <summary>
  /// Utility class which allows ssh.net to connect to servers using ras-sha2-256
  /// </summary>
  public static class RsaSha256Util
  {
    public static void SetupConnection(ConnectionInfo connection)
    {
      connection.HostKeyAlgorithms["rsa-sha2-256"] = data => new KeyHostAlgorithm("rsa-sha2-256", new RsaKey(), data);
    }

    /// <summary>
    /// Converts key file to rsa key with sha2-256 signature
    /// Due to lack of constructor: https://github.com/sshnet/SSH.NET/blob/bc99ada7da3f05f50d9379f2644941d91d5bf05a/src/Renci.SshNet/PrivateKeyFile.cs#L86
    /// We do that in place
    /// </summary>
    /// <param name="keyFile"></param>
    /// <exception cref="ArgumentNullException"></exception>
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

    private static void UpdatePrivateKeyFile(PrivateKeyFile keyFile, RsaWithSha256SignatureKey key)
    {
      var keyHostAlgorithm = new KeyHostAlgorithm(key.ToString(), key);

      var hostKeyProperty = typeof(PrivateKeyFile).GetProperty(nameof(PrivateKeyFile.HostKey));
      hostKeyProperty.SetValue(keyFile, keyHostAlgorithm);

      var keyField = typeof(PrivateKeyFile).GetField("_key", BindingFlags.NonPublic | BindingFlags.Instance);
      keyField.SetValue(keyFile, key);
    }
  }

  public class RsaWithSha256SignatureKey : RsaKey
  {
    public RsaWithSha256SignatureKey(BigInteger modulus, BigInteger exponent, BigInteger d, BigInteger p, BigInteger q,
        BigInteger inverseQ) : base(modulus, exponent, d, p, q, inverseQ)
    {
    }

    private RsaSha256DigitalSignature _digitalSignature;

    protected override DigitalSignature DigitalSignature
    {
      get
      {
        if (_digitalSignature == null)
        {
          _digitalSignature = new RsaSha256DigitalSignature(this);
        }

        return _digitalSignature;
      }
    }

    public override string ToString()
    {
      return "rsa-sha2-256";
    }
  }
}
