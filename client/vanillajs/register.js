document.addEventListener('DOMContentLoaded', async () => {
    const signupForm = document.querySelector('#signup-form');
    if (signupForm) {
        signupForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            // Grab reference to the emailInput. The email address
            // entered will be passed to the server and used to create
            // a customer. Email addresses do NOT uniquely identify
            // customers in Stripe.
            const emailInput = document.querySelector('#email');
            const pledgeAmountEntered = document.querySelector('#pledgeAmount');
            const frequenceSelection = document.querySelector("#frequency");
            const productSelection = document.querySelector("#product");
                       

            // Create a customer. This will also set a cookie on the server
            // to simulate having a logged in user.
            const { customer } = await fetch('/create-customer-transaction', {
                method: 'post',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    email: emailInput.value,
                    frequency: frequenceSelection.value,
                    amount: pledgeAmountEntered.value,
                    product: productSelection.value,
                    product_description: productSelection.selectedOptions[0].text,
                }),
            }).then((response) => response.json())
              .then((data) => {
                    window.sessionStorage.setItem('subscriptionId', data.subscriptionId);
                    window.sessionStorage.setItem('clientSecret', data.clientSecret);
                    window.location.href = '/subscribe.html';
                })
              .catch((error) => {
                    console.error('Error:', error);
                });
        });
    } else {
        alert("No sign up form with ID `signup-form` found on the page.");
    }
});
