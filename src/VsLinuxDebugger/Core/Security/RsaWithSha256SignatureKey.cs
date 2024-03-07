using Renci.SshNet.Common;
using Renci.SshNet.Security;
using Renci.SshNet.Security.Cryptography;

namespace VsLinuxDebugger.Core.Security
{
  /*
  /// <summary>RSA With SHA256 Signature Key.</summary>
  public class RsaWithSha256SignatureKey : RsaKey
  {
    private RsaSha256DigitalSignature _digitalSignature;

    public RsaWithSha256SignatureKey(BigInteger modulus, BigInteger exponent, BigInteger d, BigInteger p, BigInteger q, BigInteger inverseQ)
      : base(modulus, exponent, d, p, q, inverseQ)
    {
    }

    protected override DigitalSignature DigitalSignature
    {
      get
      {
        _digitalSignature ??= new RsaSha256DigitalSignature(this);
        return _digitalSignature;
      }
    }

    public override string ToString()
    {
      return RsaSha256Util.RSA_SHA2_256;
    }
  }
  */
}
