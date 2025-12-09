using System.Text.Json.Serialization;

namespace Excid.Sigstore.Fulcio.v2.Models
{
	public class SigningCertificateRequest
	{
		public Credentials Credentials { get; set; }= new Credentials();

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public PublicKeyRequest? PublicKeyRequest { get; set; } = null;

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public string? CertificateSigningRequest { get; set; } = null;
	}

	public class Credentials
	{
		public string OidcIdentityToken { get; set; } = string.Empty;
	}

	public class PublicKeyRequest
	{
		public PublicKey PublicKey { get; set; } =  new PublicKey();
		public string ProofOfPossession { get; set; } = string.Empty;
	}

	public class PublicKey
	{
		
		public PublicKeyAlgorithm Algorithm { get; set; } = PublicKeyAlgorithm.PUBLIC_KEY_ALGORITHM_UNSPECIFIED;
		public string Content { get; set; } = string.Empty;

	}

	public enum PublicKeyAlgorithm
	{
		PUBLIC_KEY_ALGORITHM_UNSPECIFIED = 0,
		RSA_PSS = 1,
		ECDSA = 2,
		ED25519 = 3
	}

	public class SigningCertificateResponse
	{
		public SignedCertificateDetachedSct SignedCertificateDetachedSct { get; set; }	=	new SignedCertificateDetachedSct();
	}

	public class SignedCertificateDetachedSct
	{
		public Chain Chain { get; set; } = new Chain();  
		public string SignedCertificateTimestamp { get; set; } = string.Empty;
	}

	public class Chain
	{
		public List<string> Certificates { get; set; } = new List<string>();
	}


}
