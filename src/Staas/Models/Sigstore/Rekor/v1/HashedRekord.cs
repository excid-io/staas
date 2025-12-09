using System.Text.Json.Serialization;


namespace Excid.Sigstore.Rekor.v1.Models
{
	public class HashedRekord
	{
        public Spec Spec { get; set; } = new Spec();
        public string Kind { get; set; } = "hashedrekord";
        public string ApiVersion { get; set; } = "0.0.1";

	}

    public class Spec
    {
		public Signature Signature { get; set; } = new Signature();
		public Data Data { get; set; } = new Data();
	}

	public class Signature
	{
        public string Content { get; set; } = string.Empty;
        public PublicKey PublicKey { get; set; } = new PublicKey();
    }

    public class PublicKey
    {
        public string Content { get; set; } = string.Empty;

    }

    public class Data
    {
        public Hash Hash { get; set; } = new Hash();

    }

    public class Hash
    {
        public string Algorithm { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}

