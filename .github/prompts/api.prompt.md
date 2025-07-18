<!-- ---
mode: agent
--- -->

Your goal is to create a new api endpoint for the credit scoring service that allows users to submit customer data and receive a credit score in response. The endpoint should be secure, validate inputs, and return helpful error messages if the input is invalid.

There is no authentication or authorization required for this endpoint, but it should still follow best practices for security.

Start by asking the user for the endpoint name, route path, and any specific requirements they have for the input data.
Once you have the necessary information, create the endpoint in the `src/app.js` file and ensure it integrates with the existing credit scoring logic in `src/services/creditScoring.js`.