using System.Text.Json.Serialization;

namespace Excid.Oidc.Models
{
    public class OpenIDConfiguration
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; } = string.Empty;

		[JsonPropertyName("jwks_uri")]
		public string JwksUri { get; set; } = string.Empty;

		[JsonPropertyName("scopes_supported")]
        public List<string> ScopesSupported { get; set; } = new List<string> { "openid"};

        [JsonPropertyName("response_types_supported")]
        public List<string> ReponseTypesSupported { get; set; } = new List<string> { "id_token" };

        [JsonPropertyName("grant_types_supported")]
        public List<string> GtantTypesSupported { get; set; } = new List<string> { "authorization_code" };

		[JsonPropertyName("subject_types_supported")]
		public List<string> SubjectTypesSupported { get; set; } = new List<string> { "public" };

		[JsonPropertyName("id_token_signing_alg_values_supported")]
		public List<string> IdTokenSigningAlgValuesSupported { get; set; } = new List<string> { "ES256" };
	}
}
