document.addEventListener('DOMContentLoaded', async () => {
    // Fetch the payment result.
    const { paymentIntent } = await fetch('/payment_result').then((r) => r.json());
    const paymentResultDiv = document.querySelector('#paymentResult');
    paymentResultDiv.innerHTML += `
        <hr>
      <h4>
      Result: ${paymentIntent.status}
       
      </h4>

      <p>
        Reference: ${paymentIntent.id}
      </p>

      <p>
        For: ${paymentIntent.description}
      </p>   
      `;
});
