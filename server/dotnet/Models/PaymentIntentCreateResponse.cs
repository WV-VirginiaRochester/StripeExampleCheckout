using Stripe;
using Newtonsoft.Json;


public class PaymentIntentCreateResponse
{
  [JsonProperty("clientSecret")]
  public string ClientSecret { get; set; }
}
