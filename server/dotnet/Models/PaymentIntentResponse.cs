using Stripe;
using Newtonsoft.Json;


public class PaymentIntentResponse
{
    [JsonProperty("paymentIntent")]
    public PaymentIntent PaymentIntent { get; set; }
}
