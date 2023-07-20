using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Stripe;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace dotnet.Controllers
{
    public class BillingController : Controller
    {
        private readonly IOptions<StripeOptions> options;

        public BillingController(IOptions<StripeOptions> options)
        {
            this.options = options;
            StripeConfiguration.ApiKey = options.Value.SecretKey;
        }

        [HttpGet("config")]
        public ActionResult<ConfigResponse> GetConfig()
        {            
            return new ConfigResponse
            {
                PublishableKey = this.options.Value.PublishableKey               
            };
        }

        [HttpPost("create-customer-transaction")]
        public ActionResult<PaymentIntentCreateResponse> CreateCustomerTransaction([FromBody] CreateCustomerTransactionRequest req)
        {
            //step 1: create customer using email address
            //it is likely we will use more data than just the email but this is the minimum
            var options = new CustomerCreateOptions
            {
                Email = req.Email,
            };
            var service = new CustomerService();
            var customer = service.Create(options);            

            //step 2: check the frequency of the payment. If it is just one-off we create a payment intent:
            if (req.Frequency == "one-off")
            {
                var paymentIntentCreateOptions = new PaymentIntentCreateOptions()
                {
                    Amount = long.Parse(req.Amount),//amount should always have 00 at end, so $14.00 = '1400'
                    Currency = "cad",//we need to think about where the currency info should come from: should it always be canadian dollars?
                    Description = req.ProductDescription,
                    Customer = customer.Id,
                    Metadata = new Dictionary<string, string>()
                    {
                        {"productid", req.Product }//an example of adding information to the metadata of this transaction. Alvin is scoping what data we need to add
                    }
                };
                var paymentIntentService = new PaymentIntentService();

                try
                {
                    var paymentIntent = paymentIntentService.Create(paymentIntentCreateOptions);
                    HttpContext.Response.Cookies.Append("paymentIntentId", paymentIntent.Id);
                    return new PaymentIntentCreateResponse
                    {
                        ClientSecret = paymentIntent.ClientSecret,//this is needed for the Stripe Element to confirm card payment
                    };
                }
                catch (StripeException e)
                {
                    Console.WriteLine($"Failed to create Payment Intent.{e}");
                    return BadRequest();
                }

            }
            else
            {

                //step 2: check the frequency of the payment. If it is monthly we create a price based on the product id, then a subsciption with that price:
                var priceOptions = new PriceCreateOptions
                {
                    Product = req.Product,
                    UnitAmount = long.Parse(req.Amount),
                    Currency = "cad",
                    Recurring = new PriceRecurringOptions()
                    {
                        Interval = "month",
                        IntervalCount = 1
                    },
                    Nickname = req.ProductDescription
                };

                var priceService = new PriceService();
                var price = new Price();
                try
                {
                    price = priceService.Create(priceOptions);
                }
                catch (StripeException e)
                {
                    Console.WriteLine($"Failed to create Price.{e}");
                    return BadRequest();
                }
                // Automatically save the payment method to the subscription
                // when the first payment is successful.
                var paymentSettings = new SubscriptionPaymentSettingsOptions
                {
                    SaveDefaultPaymentMethod = "on_subscription",
                };

                // Create subscription
                var subscriptionOptions = new SubscriptionCreateOptions
                {
                    Customer = customer.Id,
                    Items = new List<SubscriptionItemOptions>
                    {
                        new SubscriptionItemOptions
                        {
                            Price = price.Id
                        },
                    },
                    PaymentSettings = paymentSettings,
                    PaymentBehavior = "default_incomplete",  //On the back end, create the subscription with status incomplete using payment_behavior=default_incomplete.                 
                };
                subscriptionOptions.AddExpand("latest_invoice.payment_intent");//this creates an immediate payment intent to pay now
                var subscriptionService = new SubscriptionService();
                try
                {
                    Subscription subscription = subscriptionService.Create(subscriptionOptions);
                    HttpContext.Response.Cookies.Append("paymentIntentId", subscription.LatestInvoice.PaymentIntent.Id);
                    return new PaymentIntentCreateResponse
                    {
                        ClientSecret = subscription.LatestInvoice.PaymentIntent.ClientSecret,//this is needed for the Stripe Element to confirm card payment
                    };
                }
                catch (StripeException e)
                {
                    Console.WriteLine($"Failed to create subscription.{e}");
                    return BadRequest();
                }
            }

        }

        [HttpGet("payment_result")]
        public ActionResult<PaymentIntentResponse> PaymentResult()
        {
            //check if have returned from redirection at banking validation end (in which case the payment_intent will be in query params: see https://stripe.com/docs/billing/subscriptions/build-subscriptions?ui=elements#complete-payment)
            var paymentIntentId = string.Empty;
            if (!string.IsNullOrEmpty(Request.Query["payment_intent"]))
            { paymentIntentId = Request.Query["payment_intent"]; }
            else //otherwise pick up from the cookie
            { paymentIntentId = HttpContext.Request.Cookies["paymentIntentId"]; }
           
            var service = new PaymentIntentService();
            var paymentIntent = service.Get(paymentIntentId);

            return new PaymentIntentResponse
            {
                PaymentIntent = paymentIntent,
            };
        }

    }
}
