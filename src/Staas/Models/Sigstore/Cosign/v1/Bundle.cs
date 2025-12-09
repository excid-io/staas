using System.Text.Json.Serialization;

namespace Excid.Sigstore.Cosign.v1.Models
{
    public class Bundle
    {
        [JsonPropertyName("base64Signature")]
        public string Base64Signature { get; set; } = string.Empty;
        [JsonPropertyName("cert")]
        public string Cert { get; set; } = string.Empty;
        [JsonPropertyName("rekorBundle")]
        public RekorBundle RekorBundle { get;set;} = new RekorBundle();

    }

    public class RekorBundle
    {
        [JsonPropertyName("SignedEntryTimestamp")]
        public string SignedEntryTimestamp { get; set; } = string.Empty;
        [JsonPropertyName("Payload")]
        public Payload Payload { get; set; } = new Payload();
    }

    public class Payload
    {
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
        [JsonPropertyName("integratedTime")]
        public int IntegratedTime { get; set; }
        [JsonPropertyName("logIndex")]
        public int LogIndex { get; set; }
        [JsonPropertyName("logID")]
        public string LogId { get; set; } = string.Empty;
    }
}
