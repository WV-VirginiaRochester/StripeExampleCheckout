// helper method for displaying a status message.
const setMessage = (message) => {
    const messageDiv = document.querySelector('#messages');
    messageDiv.innerHTML += "<br>" + message;
}

// Fetch public key and initialize Stripe.
//let stripe
//fetch('/config')
//    .then((resp) => resp.json())
//    .then((resp) => {
//        stripe = Stripe(resp.publishableKey);
//    });
const stripe = Stripe('pk_test_51McVyAJKcS7rKiSnufTfBQU3EUKxoZezQBf7SCa6tJUMbPghmUcl24wSQykYtNq8uZ7HsCJSbNbwl2KCYaCb8e2m00uv9xJL5I');
const clientSecret = window.sessionStorage.getItem('clientSecret');
const addressForBillingDetails = {
    postal_code: 'NG7 1JR',
};
const billingDetailsForDefaultValues = {
    address: addressForBillingDetails,
};
const defaultValuesForOptions = {
    billingDetails: billingDetailsForDefaultValues,
};

const options = {
    clientSecret: clientSecret,
    defaultValues: defaultValuesForOptions,
    layout:'tabs',
    // Fully customizable with appearance API.
    appearance: {/*...*/ },
};

// Set up Stripe.js and Elements to use in checkout form, passing the client secret obtained in step 5
const elements = stripe.elements(options);

// Create and mount the Payment Element
const paymentElement = elements.create('payment');
paymentElement.mount('#payment-element');
paymentElement.update({ business: { name: 'Stripe Shop' } });

// Extract the client secret query string argument. This is
// required to confirm the payment intent from the front-end.
const subscriptionId = window.sessionStorage.getItem('subscriptionId');

// This sample only supports a Subscription with payment
// upfront. If you offer a trial on your subscription, then
// instead of confirming the subscription's latest_invoice's
// payment_intent. You'll use stripe.confirmCardSetup to confirm
// the subscription's pending_setup_intent.
// See https://stripe.com/docs/billing/subscriptions/trials

// Payment info collection and confirmation
// When the submit button is pressed, attempt to confirm the payment intent
// with the information input into the card element form.
// - handle payment errors by displaying an alert. The customer can update
//   the payment information and try again
// - Stripe Elements automatically handles next actions like 3DSecure that are required for SCA
// - Complete the subscription flow when the payment succeeds
const form = document.querySelector('#payment-form');
form.addEventListener('submit', async (e) => {
    e.preventDefault();
    const { error } = await stripe.confirmPayment({
        //`Elements` instance that was used to create the Payment Element
        elements,
        confirmParams: {
            return_url: "http://localhost:4242/PaymentResult.html",
        }
    });
    if (error) {
        // This point will only be reached if there is an immediate error when
        // confirming the payment. Show error to your customer (for example, payment
        // details incomplete)
        //const messageContainer = document.querySelector('#error-message');
        setMessage(error.message);
    } else {
        // Your customer will be redirected to your `return_url`. For some payment
        // methods like iDEAL, your customer will be redirected to an intermediate
        // site first to authorize the payment, then redirected to the `return_url`.
    }
});