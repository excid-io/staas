using System.Text.Json.Serialization;

namespace Excid.Sigstore.Rekor.v1.Models
{

	public class LogEntry
	{
        [JsonPropertyName("logID")]
        public string LogID { get; set; } = string.Empty;
        [JsonPropertyName("logIndex")]
        public int LogIndex { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; } = string.Empty;
        [JsonPropertyName("integratedTime")]
        public int IntegratedTime { get; set; }
        [JsonPropertyName("attestation")]
        public Attestation? Attestation { get; set; } = null;
        [JsonPropertyName("verification")]
        public Verification? Verification { get; set; } = null;	
	}

	public class Attestation
	{
        [JsonPropertyName("data")]
        public string Data { get; set; } = string.Empty;
	}

	public class Verification
	{
        [JsonPropertyName("inclusionProof")]
        public InclusionProof InclusionProof { get; set; }	= new InclusionProof();
        [JsonPropertyName("signedEntryTimestamp")]
        public string SignedEntryTimestamp { get; set; } = string.Empty;

	}

	public class InclusionProof
	{
        [JsonPropertyName("logIndex")]
        public int LogIndex { get; set; }
        [JsonPropertyName("rootHash")]
        public string RootHash { get; set; } = string.Empty;
        [JsonPropertyName("treeSize")]
        public int TreeSize { get; set; }
        [JsonPropertyName("hashes")]
        public List<string> Hashes { get; set; } = new List<string>();
        [JsonPropertyName("checkPoint")]
        public string CheckPoint { get; set; } = string.Empty;
	}
}
