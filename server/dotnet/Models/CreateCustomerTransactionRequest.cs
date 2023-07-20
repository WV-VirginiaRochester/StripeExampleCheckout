using Newtonsoft.Json;

public class CreateCustomerTransactionRequest
{
    [JsonProperty("email")]
    public string Email { get; set; }
    [JsonProperty("frequency")]
    public string Frequency { get; set; }
    [JsonProperty("amount")]
    public string Amount { get; set; }
    [JsonProperty("product")]
    public string Product { get; set; }
    [JsonProperty("product_description")]
    public string ProductDescription { get; set; }
}