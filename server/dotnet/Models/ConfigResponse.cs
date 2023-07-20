using Newtonsoft.Json;
using Stripe;
using System.Collections.Generic;

public class ConfigResponse
{
    [JsonProperty("publishableKey")]
    public string PublishableKey { get; set; }
  
}
