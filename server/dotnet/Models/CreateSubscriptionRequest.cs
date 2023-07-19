using Newtonsoft.Json;

public class CreateSubscriptionRequest
{
    [JsonProperty("amount")]
    public long Amount { get; set; }
    [JsonProperty("subscription")]
    public bool Subscription{ get; set; }
}
