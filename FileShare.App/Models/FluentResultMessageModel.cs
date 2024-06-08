using Newtonsoft.Json;

namespace FileShare.App.Models;

public class FluentResultMessageModel
{
    [JsonProperty("Message")]
    public string Message { get; set; }

    [JsonProperty("Metadata")]
    public Dictionary<string, object> Metadata { get; set; }

    [JsonProperty("Reasons")]
    public List<object> Reasons { get; set; }
}