

# **Whitepaper: Integrating MoonPay's Whitelabel Financial Infrastructure into a.NET Ecosystem**

## **Executive Summary**

The proliferation of digital assets has created a significant opportunity for businesses to integrate cryptocurrency services. However, the development of proprietary fiat-crypto-fiat financial rails presents formidable challenges. These include navigating a complex web of global payment integrations, managing liquidity for a diverse range of assets, mitigating sophisticated financial fraud, and ensuring continuous compliance with multi-jurisdictional regulatory frameworks like Know Your Customer (KYC) and Anti-Money Laundering (AML).1 The technical, legal, and financial overhead associated with these tasks can divert critical resources from core product innovation.

MoonPay emerges not merely as a payment processor but as a comprehensive Financial Infrastructure-as-a-Service (FaaS) platform designed to abstract this complexity.3 It provides a robust suite of APIs and SDKs that empower businesses to seamlessly embed cryptocurrency transactions and Web3 services directly into their applications.6 By handling the intricate backend processes of payment processing, compliance, and liquidity, MoonPay enables development teams to accelerate their time-to-market and focus on delivering unique value to their users.3

This whitepaper serves as a definitive architectural blueprint for integrating MoonPay's whitelabel services into an enterprise-grade.NET application. Recognizing the absence of a native.NET SDK, this document outlines a REST API-first strategy, providing specific guidance, architectural patterns, and C\# code examples. It is designed to equip a.NET development team with the technical knowledge and strategic context required to build a fully branded, secure, and compliant financial services experience, transforming a complex infrastructure challenge into a manageable and strategic integration project.

## **Section 1: The Strategic Value of Integrating MoonPay's Whitelabel Infrastructure**

Before delving into the technical implementation, it is crucial for a development team to understand the strategic value and business case for integrating a third-party financial infrastructure. MoonPay offers a portfolio of services that extend far beyond a simple "buy crypto" button, providing the foundational components for a complete digital asset ecosystem. This section explores the core service offerings, the value of abstracted complexity, and the market validation that underpins MoonPay's strategic position.

### **1.1 Beyond the Widget: A Portfolio of Core Service Offerings**

MoonPay's platform is a suite of distinct yet interconnected services, each accessible via API, that collectively enable a comprehensive Web3 user journey within a partner's application.

* **On-Ramp (Fiat-to-Crypto):** This is the foundational service that bridges the traditional financial world with the crypto economy. It allows users to purchase a wide array of over 100 cryptocurrencies using an extensive list of fiat payment methods.3 The global reach is significant, supporting over 160 countries and more than 80 fiat currencies.3 Payment options include standard credit and debit cards (Visa, Mastercard), regional bank transfer systems like SEPA in the EU, ACH in the US, and UK Faster Payments, as well as modern mobile wallets such as Apple Pay and Google Pay.3 This breadth of options is critical for maximizing conversion rates by meeting users where they are, with the payment methods they trust.8 For many users, the on-ramp is their first interaction with cryptocurrency, making a smooth and intuitive process essential for successful onboarding.11  
* **Off-Ramp (Crypto-to-Fiat):** Creating a closed-loop system is vital for user retention and platform utility. MoonPay's off-ramp service provides this critical function, allowing users to sell their digital assets and receive fiat currency directly to their bank accounts or cards.3 This crypto-to-fiat capability ensures that users can realize the value of their assets within the same application where they acquired them, completing the financial circle and reducing the likelihood of user churn to external exchanges.6  
* **Cross-Chain Swaps:** To further enhance in-app utility, MoonPay offers a crypto-to-crypto swap service. This feature enables users to exchange one digital asset for another directly within the partner's platform, often across different blockchains.3 By providing competitive exchange rates and eliminating the need for users to navigate the complexities of external decentralized exchanges (DEXs), this service increases user engagement and keeps them within the partner's ecosystem.  
* **NFT Commerce: Checkout and Minting:** MoonPay has invested heavily in becoming a leader in the NFT space, offering two distinct services that dramatically lower the barrier to entry for mainstream participation.  
  * **NFT Checkout:** This service is a game-changer for NFT marketplaces and creators. It allows end-users to purchase NFTs directly with fiat currencies like USD or EUR using their credit or debit cards, bypassing the traditional multi-step process of acquiring crypto first.3 Supporting over 40 blockchains, this feature makes NFT acquisition as simple as any other e-commerce transaction, opening the market to a much broader, non-crypto-native audience.6  
  * **HyperMint:** For larger brands and enterprises, HyperMint is a powerful platform-as-a-service for NFT creation and management. It enables the minting of NFT collections at an industrial scale—up to one million NFTs per hour—without requiring the partner to have deep smart contract or blockchain development expertise.5 High-profile brands like Universal Studios, Gucci, and Fox have leveraged this service for their Web3 initiatives, demonstrating its enterprise-grade capabilities.13

### **1.2 Abstracting Complexity: The Value of Managed Compliance and Security**

The most significant value proposition of integrating MoonPay is the abstraction of immense operational, legal, and security burdens. For any enterprise system, particularly one handling financial transactions, this is a paramount consideration.

* **Global Regulatory Adherence:** The regulatory landscape for digital assets is fragmented and constantly evolving. MoonPay navigates this complexity on behalf of its partners, managing KYC and AML verification processes tailored to the requirements of over 160 countries.2 The company maintains necessary registrations and licenses, such as its cryptoasset registration with the UK's Financial Conduct Authority (FCA) and, notably, its license under the EU's comprehensive Markets in Crypto-Assets (MiCA) regulation.15 This offloads a significant legal and compliance burden that would otherwise require a dedicated, expert in-house team.  
* **Enterprise-Grade Security Posture:** MoonPay's infrastructure is built to meet the highest security standards, allowing partners to inherit a posture of trust and resilience. This is validated by a portfolio of industry-leading certifications 13:  
  * **PCI DSS 4.0 Level 1:** This is the most stringent level of the Payment Card Industry Data Security Standard. Compliance ensures that all payment card data is handled in a highly secure environment, drastically reducing the partner's own PCI compliance scope and risk.18  
  * **SOC 2 Type 2:** This certification, verified by third-party auditors, attests to the long-term effectiveness of MoonPay's controls over security, availability, processing integrity, confidentiality, and privacy.3  
  * **ISO 27001 & ISO 27018:** These international standards certify a formal Information Security Management System (ISMS) and specific controls for protecting Personally Identifiable Information (PII) in the cloud, respectively.17  
* **Financial Risk Mitigation:** A core component of the FaaS model is the transfer of financial risk from the partner to MoonPay. The platform employs sophisticated, AI-driven risk management systems to prevent fraudulent transactions.2 Crucially, MoonPay offers its partners a guarantee of zero chargebacks, eliminating a significant source of financial loss and operational headache that is common in online payment processing.8

### **1.3 Market Validation: Analysis of Partner Success and Strategic Trajectory**

The strategic value of MoonPay is not merely theoretical; it is validated by quantifiable results from its partners and a clear forward-looking corporate strategy.

* **Case Study Deep Dive:** Analysis of public case studies reveals the tangible impact of integrating MoonPay's infrastructure:  
  * **Bitcoin.com:** This major crypto wallet and services provider faced challenges with a payment solution that couldn't support the breadth of payment methods its global user base demanded. After integrating MoonPay, Bitcoin.com achieved a staggering **\+545% increase in revenue** and a **\+230% increase in transaction volume**. The rapid integration, which took only days, and the comprehensive nature of the solution were key factors in this success.21  
  * **BRD Wallet:** This case demonstrates MoonPay's flexibility as a strategic partner. By running a targeted zero-fee promotional campaign, BRD saw a **31% increase in conversion** and an addition of **320 new user signups per day** in the first week alone, showcasing how the platform can be leveraged for specific marketing and growth initiatives.22  
  * **Changelly:** This cryptocurrency exchange provides a powerful lesson in user experience. By creating a dedicated landing page featuring the MoonPay widget, they simplified the purchasing journey and reduced the number of steps by 40%. The result was a **48% increase in overall conversion**, highlighting the direct link between a frictionless user flow and improved business outcomes.23  
  * **Zero Hash:** This partnership underscores MoonPay's ability to execute rapid market entry strategies. By leveraging Zero Hash's U.S.-based crypto-as-a-service infrastructure, MoonPay was able to launch in the complex U.S. market in under a month and scale to onboard nearly a million customers, demonstrating a nimble and strategic approach to expansion.24  
* **Strategic Ecosystem Expansion:** MoonPay's recent corporate activities signal a clear ambition to transcend its role as a simple on-ramp and become a central pillar of the future financial system. The initial focus on on-ramps solved the basic problem of crypto acquisition. The subsequent addition of off-ramps and swaps created a more complete ecosystem for user retention. Now, the company is making a significant move up the value chain. The acquisition of **Iron**, an API-first stablecoin infrastructure platform, was not merely an enhancement to existing products; it was the foundation for an entirely new enterprise service category.25 This was immediately leveraged in a landmark partnership with  
  **Mastercard**. This collaboration will use Iron's technology to issue stablecoin-powered Mastercard cards, enabling users to spend their stablecoin balances directly at over 150 million merchant locations worldwide where Mastercard is accepted.27 This strategic evolution demonstrates a shift from serving Web3 companies to providing foundational financial plumbing for fintechs, neobanks, and global enterprises looking to leverage the efficiency and programmability of stablecoins for real-world applications like cross-border payments and treasury management.

## **Section 2: Architectural Blueprint for.NET Integration**

This section provides the core technical guidance for a.NET development team tasked with integrating MoonPay. It details the available integration pathways, proposes a robust architectural strategy for a.NET environment, and provides specific C\# implementations for critical functions like authentication and request signing.

### **2.1 Integration Pathways: A Comparative Analysis**

MoonPay offers several methods for integration, each with distinct trade-offs between implementation speed and customization control. Understanding these pathways is the first step in designing a solution that aligns with the project's goals. While the term "whitelabel" is sometimes used to describe the customizable widget 3, a true, fully branded experience necessitates a deeper, API-driven approach.

* **The Widget/SDK Approach:** This is the most straightforward integration method. It involves embedding a pre-built, sandboxed UI component into an application.3 This widget can be displayed in several ways, such as a modal overlay on the existing interface, an embedded iframe, or by opening in a new browser tab or window.11 Customization is primarily limited to theming—adjusting colors, fonts, and border styles to better match the partner's branding.30 While this approach is fast to implement, it does not offer complete branding control, as the user is still interacting with MoonPay's UI and flow, and some branding artifacts may remain visible.32  
* **The API-First (Headless) Approach:** This is the most powerful and flexible integration method, involving direct server-to-server communication with MoonPay's REST APIs.33 In this model, the partner is responsible for building the entire user interface from the ground up. MoonPay operates as an invisible "engine" in the background, handling the complex financial and compliance tasks. This approach provides total control over the user experience and is the only path to achieving a true whitelabel implementation where the end-user may be entirely unaware of MoonPay's involvement.  
  **For a.NET team seeking maximum customization and a fully branded enterprise solution, this is the recommended strategy.**

The following table provides a clear decision-making framework, justifying the choice of an API-first strategy for a true whitelabel implementation.

Table 2.1: Comparison of MoonPay Integration Methods  
| Integration Method | Implementation Effort | UI/UX Customization | Branding Control | Primary Use Case |.NET Applicability |  
| :--- | :--- | :--- | :--- | :--- | :--- |  
| Widget (URL-based) | Low | Theming Only | Partial \- MoonPay branding present | Quick on-ramp for a website | Via WebView/Browser Control |  
| SDKs (JS/React/Mobile) | Medium | High (within widget context) | Partial | Mobile-native or React-based app experience | Not directly applicable |  
| REST API-First (Headless) | High | Total Control | Complete \- MoonPay can be invisible | Fully branded enterprise system | Recommended path via HttpClient |

### **2.2 The.NET Integration Landscape: A REST API-First Strategy**

It is important to explicitly acknowledge that, as of this writing, MoonPay does not offer a dedicated, officially supported.NET SDK.6 The primary SDKs are built for client-side JavaScript environments (vanilla JS, React, React Native) and native mobile platforms (iOS, Android).3

The absence of a.NET SDK is not a blocker but dictates the integration strategy. The solution for a.NET team is to interact with MoonPay's services by making direct, server-to-server HTTP requests to its comprehensive REST API endpoints. This is a standard and robust approach for service integration and is fully supported by the.NET framework's built-in HttpClient class.

An effective architectural pattern for this integration is the **Backend-for-Frontend (BFF)** model. In this design, the.NET server application (e.g., an ASP.NET Core Web API) acts as a secure intermediary between the client-side application (which could be a Blazor Web App, a React/Angular SPA, or a mobile app) and the MoonPay API. The client communicates only with the trusted.NET BFF. The BFF then handles all communication with MoonPay, securely managing API keys, performing sensitive operations like URL signing, and enforcing business logic before relaying data back to the client. This pattern enhances security by ensuring that secret keys and sensitive logic are never exposed on the client side.

### **2.3 Authentication and Request Signing in C\#**

Secure communication with the MoonPay API is paramount. The primary authentication mechanism involves API keys and, for certain operations, signed URLs.

* **API Key Management:** Upon registering on the MoonPay Developer Dashboard, a partner is issued a pair of API keys: a **publishable key** (prefixed with pk\_...) and a **secret key** (prefixed with sk\_...).3 The publishable key is safe to use in client-side code to identify the partner account. The secret key, however, is highly sensitive and must be treated like a password. It should be stored securely on the server using a dedicated secrets management solution, such as Azure Key Vault, AWS Secrets Manager, or, for local development, the.NET Secret Manager tool. It must never be embedded in client-side code or committed to a source control repository.  
* **URL Signing (Mandatory for Production):** To prevent malicious actors from tampering with transaction parameters, MoonPay requires that URLs passed to its widget be signed when they contain sensitive information, such as a pre-filled walletAddress.36 This signing process must be performed on the server-side using the secret key.

The signature is generated by creating a Hash-based Message Authentication Code (HMAC) with the SHA-256 algorithm. The message for the HMAC is the query string of the URL, and the key for the HMAC is the partner's secret API key.37

The following C\# code provides a practical implementation for signing a MoonPay URL within a.NET application:

C\#

// Required using statements:  
// using System.Security.Cryptography;  
// using System.Text;  
// using System.Web; // For HttpUtility, requires a reference to System.Web.dll or the System.Web.HttpUtility NuGet package.

public class MoonPaySignatureService  
{  
    /// \<summary\>  
    /// Signs a MoonPay widget URL with an HMAC-SHA256 signature.  
    /// \</summary\>  
    /// \<param name="unsignedUrl"\>The full widget URL with all query parameters.\</param\>  
    /// \<param name="apiSecretKey"\>Your MoonPay secret key (sk\_...).\</param\>  
    /// \<returns\>The signed URL, with the signature appended as a query parameter.\</returns\>  
    public string SignUrl(string unsignedUrl, string apiSecretKey)  
    {  
        var uri \= new Uri(unsignedUrl);  
        // The message to be signed is the query string, including the leading '?'.  
        string messageToSign \= uri.Query;

        if (string.IsNullOrEmpty(messageToSign))  
        {  
            // No query string to sign, return the original URL.  
            return unsignedUrl;  
        }

        byte keyBytes \= Encoding.UTF8.GetBytes(apiSecretKey);  
        byte messageBytes \= Encoding.UTF8.GetBytes(messageToSign);

        using (var hmac \= new HMACSHA256(keyBytes))  
        {  
            byte hashBytes \= hmac.ComputeHash(messageBytes);  
            // The resulting signature must be Base64 encoded.  
            string signature \= Convert.ToBase64String(hashBytes);  
              
            // The Base64 signature must then be URL-encoded to be safely passed in the URL.  
            string urlEncodedSignature \= HttpUtility.UrlEncode(signature);

            return $"{unsignedUrl}\&signature={urlEncodedSignature}";  
        }  
    }  
}

(This implementation is synthesized from the signing logic described in 37 and standard C\# HMAC practices found in 38).

### **2.4 Core API Endpoints and Transaction Flows**

For a headless integration, the.NET backend will interact with several key API endpoints to build a native transaction flow. While the full API reference provides a comprehensive list of capabilities 6, the following endpoints are essential for the core on-ramp and off-ramp functionalities.

* **On-Ramp Flow:**  
  1. **Get Currency Information:** Before displaying purchase options, the UI needs to know the rules. The backend calls GET /v3/currencies/:currencyCode/limits to fetch the minimum and maximum purchase amounts for a specific cryptocurrency, which is a required best practice for integration.42  
  2. **Get Real-Time Quote:** When the user enters a purchase amount, the backend calls GET /v3/transactions/buy\_quote. This endpoint returns a real-time price for the transaction, including a breakdown of all applicable fees. This allows the UI to display a transparent and accurate quote to the user before they commit.42  
  3. **Initiate Transaction:** Even in a headless flow, the final transaction is typically initiated by redirecting the user to a signed widget URL. The API-first approach adds value by programmatically pre-filling all known user data (e.g., walletAddress, email, baseCurrencyAmount) into this URL. This creates a highly streamlined experience where the user may only need to enter their payment details and complete the KYC process if they are a new user.36  
  4. **Track Transaction Status:** While polling GET /v3/transactions/:transactionId is possible, the recommended and more efficient method for tracking transaction status is via webhooks, which are discussed in the next section.6  
* **Off-Ramp Flow:**  
  1. **Get Sell Quote:** Similar to the on-ramp, the backend calls GET /v3/transactions/sell\_quote to get a real-time quote for selling a specified amount of cryptocurrency for a target fiat currency.6  
  2. **Track Sell Transaction:** The status of a sell transaction can be retrieved via GET /v3/transactions/sell/:transactionId.6  
  3. **Cancel Sell Transaction:** If necessary, a pending sell transaction can be canceled by calling DELETE /v3/sell\_transactions/:transactionId.6

The following table provides a quick reference for the most critical API endpoints.

Table 2.4: Critical MoonPay API Endpoints for Transaction Management  
| Function | HTTP Method & Endpoint | Key Parameters |.NET Implementation Notes |  
| :--- | :--- | :--- | :--- |  
| Get Buy Quote | GET /v3/transactions/buy\_quote | currencyCode, baseCurrencyAmount, paymentMethod | Use HttpClient.GetAsync(). Deserialize JSON response into a C\# model class representing the quote details. |  
| Get Currency Limits | GET /v3/currencies/:currencyCode/limits | currencyCode, paymentMethod | Call this endpoint to enforce purchase limits in your UI before the user proceeds. |  
| Get Transaction Status | GET /v3/transactions/:transactionId | transactionId | Primarily for ad-hoc checks. Use webhooks for real-time status updates. |  
| Get Sell Quote | GET /v3/transactions/sell\_quote | baseCurrencyCode, baseCurrencyAmount, paymentMethod | Similar to the buy quote, provides a transparent price for selling crypto. |  
| List Supported Currencies | GET /v3/currencies | type (crypto/fiat) | Use to dynamically populate currency selection lists in your application's UI. |

## **Section 3: Implementing Asynchronous Communication with Webhooks**

For an enterprise-grade system, relying on polling to check the status of transactions is inefficient and architecturally unsound. A robust, event-driven architecture using webhooks is the superior approach. Webhooks provide asynchronous, real-time notifications from MoonPay to your backend system, enabling immediate action upon events like transaction completion or failure.

### **3.1 Webhook Architecture: Designing a Resilient Listener in a.NET Environment**

Webhooks are HTTP POST requests that MoonPay sends to a URL endpoint that you provide.44 This endpoint, or "listener," is responsible for receiving and processing these event notifications.

* **Configuration:** Webhook endpoints are configured within the MoonPay Developer Dashboard under the "Webhook settings." You can add multiple endpoint URLs, for instance, one for your sandbox/testing environment and another for production. You can also select which specific events you want to be notified about for each endpoint.44  
* **.NET Implementation:** The ideal way to implement a webhook listener in a.NET environment is to create a dedicated endpoint within an ASP.NET Core Web API. This can be a controller action decorated with the \[HttpPost\] attribute, for example, \[HttpPost("/webhooks/moonpay")\]. For applications built on a serverless architecture, an Azure Function with an HTTP trigger provides an excellent, highly scalable, and cost-effective alternative.  
* **Architectural Best Practices:** To ensure resilience and reliability, the webhook listener should adhere to two key principles:  
  1. **Respond Quickly:** The listener's primary responsibility is to acknowledge receipt of the webhook. It should perform the absolute minimum work required—typically just signature validation—and then immediately return a 200 OK HTTP status code to MoonPay. This prevents timeouts on MoonPay's side and ensures they don't repeatedly retry sending the same event.3  
  2. **Queue for Processing:** Any complex or time-consuming business logic (e.g., updating a database, sending a user notification, interacting with other services) should not be performed synchronously within the listener. Instead, the validated webhook payload should be placed onto a durable message queue, such as Azure Service Bus or RabbitMQ. A separate, independent worker service can then dequeue and process these messages asynchronously. This pattern decouples event reception from event processing, making the entire system more robust, scalable, and resilient to transient failures in business logic execution.

### **3.2 Essential Webhook Events and Data Payloads**

MoonPay offers a range of webhook events covering its various services, including Buy (On-Ramp), Sell (Off-Ramp), Identity Checks, and Virtual Accounts.44 For a core on-ramp and off-ramp integration, the following events are the most critical to handle:

* transaction\_completed: This is the primary success event. When this webhook is received, it signifies that the transaction has been successfully processed and the crypto has been sent. Your backend should trigger the final order fulfillment logic.  
* transaction\_failed: This is the primary failure event. This indicates that the transaction could not be completed. The payload often includes a failureReason field. Your backend should update the order status accordingly and potentially notify the user.  
* transaction\_updated: This is a more generic event that fires whenever the status of a transaction changes (e.g., from pending to waitingPayment). It can be useful for building a detailed, real-time transaction tracker UI.  
* kyc\_passed / kyc\_failed: These events provide updates on the user's identity verification status. They can be used to update the user's profile within your system and manage their access to certain features.

The following table outlines these key events and the recommended actions for your.NET backend.

Table 3.2: Key MoonPay Webhook Events  
| Event Type (type field) | Description | Key Payload Fields | Recommended Action in.NET Backend |  
| :--- | :--- | :--- | :--- |  
| transaction\_completed | A buy or sell transaction has successfully completed. | id, status, baseCurrencyAmount, quoteCurrencyAmount, walletAddress | Update transaction status in the database to 'Completed'. Credit user's account or unlock purchased features. Send a success notification to the user. |  
| transaction\_failed | A transaction has failed. | id, status, failureReason | Update transaction status in the database to 'Failed'. Log the failureReason for support purposes. Send a failure notification to the user. |  
| kyc\_passed | The user has successfully completed identity verification. | externalCustomerId, status | Flag the user's account as KYC-verified in your database. This may unlock higher transaction limits or new features. |  
| kyc\_failed | The user's identity verification has failed. | externalCustomerId, status, failureReason | Update the user's KYC status. The UI could prompt the user to retry the verification process if applicable. |

### **3.3 Security Imperative: A Step-by-Step Guide to Validating Moonpay-Signature-V2 in C\#**

You must never process a webhook without first verifying its signature. This step is critical to ensure that the request was genuinely sent by MoonPay and has not been forged by a malicious third party.44 MoonPay signs its webhook requests by including a

Moonpay-Signature-V2 header.

* **The Header Structure:** This header contains two key-value pairs separated by a comma: a timestamp prefixed by t= and one or more signatures prefixed by s=.46  
* **The Validation Process:**  
  1. **Extract:** Parse the Moonpay-Signature-V2 header to extract the timestamp string and the signature string.  
  2. **Construct:** Create the signed\_payload string. This is done by concatenating the extracted timestamp string, a literal . character, and the raw JSON body of the incoming POST request. It is essential to use the raw, unmodified request body for this step.  
  3. **Compute:** Calculate an expected signature by generating an HMAC-SHA256 hash of the signed\_payload string. The secret key for this hashing operation is your unique webhook API key, which you can retrieve from the MoonPay Developer Dashboard.  
  4. **Compare:** Compare the signature you computed in the previous step with the signature extracted from the header. The comparison should be done in a way that is safe from timing attacks. If they match, the webhook is authentic.

The following C\# code provides a practical implementation for a middleware or filter in an ASP.NET Core application to validate incoming webhook signatures.

C\#

// Required using statements:  
// using System.Security.Cryptography;  
// using System.Text;

public class MoonPayWebhookValidator  
{  
    private readonly string \_webhookSecret;

    public MoonPayWebhookValidator(string webhookSecret)  
    {  
        \_webhookSecret \= webhookSecret?? throw new ArgumentNullException(nameof(webhookSecret));  
    }

    /// \<summary\>  
    /// Validates the signature of an incoming MoonPay webhook request.  
    /// \</summary\>  
    /// \<param name="signatureHeader"\>The value of the 'Moonpay-Signature-V2' header.\</param\>  
    /// \<param name="rawRequestBody"\>The raw, unmodified JSON body of the request.\</param\>  
    /// \<returns\>True if the signature is valid, otherwise false.\</returns\>  
    public bool IsValidSignature(string signatureHeader, string rawRequestBody)  
    {  
        if (string.IsNullOrEmpty(signatureHeader) |  
| string.IsNullOrEmpty(rawRequestBody))  
        {  
            return false;  
        }

        // 1\. Extract timestamp and signature from the header  
        var headerParts \= signatureHeader.Split(',')  
           .Select(part \=\> part.Trim().Split('='))  
           .ToDictionary(split \=\> split, split \=\> split);

        if (\!headerParts.TryGetValue("t", out var timestamp) ||\!headerParts.TryGetValue("s", out var receivedSignature))  
        {  
            return false;  
        }

        // Optional but recommended: Check if the timestamp is recent to mitigate replay attacks.  
        if (\!IsTimestampRecent(timestamp))  
        {  
            // Log potential replay attack  
            return false;  
        }

        // 2\. Construct the signed\_payload string  
        string signedPayload \= $"{timestamp}.{rawRequestBody}";

        // 3\. Compute the expected signature  
        byte secretBytes \= Encoding.UTF8.GetBytes(\_webhookSecret);  
        byte payloadBytes \= Encoding.UTF8.GetBytes(signedPayload);

        using (var hmac \= new HMACSHA256(secretBytes))  
        {  
            byte hashBytes \= hmac.ComputeHash(payloadBytes);  
            // The signature in the header is a hex-encoded string.  
            string computedSignatureHex \= BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            // 4\. Compare signatures using a constant-time comparison to prevent timing attacks.  
            return CryptographicOperations.FixedTimeEquals(  
                Encoding.UTF8.GetBytes(computedSignatureHex),  
                Encoding.UTF8.GetBytes(receivedSignature)  
            );  
        }  
    }

    private bool IsTimestampRecent(string timestampStr, int toleranceInSeconds \= 300) // 5-minute tolerance  
    {  
        if (long.TryParse(timestampStr, out long timestampSeconds))  
        {  
            var timestamp \= DateTimeOffset.FromUnixTimeSeconds(timestampSeconds);  
            return (DateTimeOffset.UtcNow \- timestamp).TotalSeconds \< toleranceInSeconds;  
        }  
        return false;  
    }  
}

(This implementation is synthesized from the validation logic described in 46 and standard C\# security practices from sources like.47 Note that the signature

s is a hexadecimal string, so the computed hash must be converted to its hex representation for comparison.)

## **Section 4: Advanced Customization and True Whitelabeling**

This section addresses the core requirement of implementing a "whitelabel" service. It moves beyond basic integration to explore how a.NET team can achieve a fully branded, seamless user experience by leveraging MoonPay's infrastructure in a headless fashion. This involves understanding the spectrum of customization, the technical requirements for owning the UI, and the associated business and pricing considerations.

### **4.1 The Spectrum of Customization: From Themed Widgets to a Headless API Integration**

The degree of customization and branding control is directly proportional to the integration effort. A development team must choose a path that aligns with its strategic goals for the user experience.

* **Level 1: Themed Widget.** This is the entry-level integration. It involves applying basic branding—such as colors, fonts, and logos—to the standard MoonPay widget via theming parameters.30 While this provides some visual alignment, the user is still fundamentally interacting with MoonPay's user flow and branding, which remains prominent.32 This option is best for partners who need to launch an on-ramp quickly with minimal development overhead.  
* **Level 2: SDK Integration.** This path offers a more deeply integrated feel, particularly for mobile applications. The official SDKs for iOS, Android, and React Native allow the widget to be presented in a more native manner, such as sliding up as a modal view rather than a full-page redirect.30 This improves the transition and makes the experience feel less like a hand-off to a third party. However, it still relies on MoonPay's pre-built UI.  
* **Level 3: Headless API Integration.** This represents the pinnacle of customization and is the only path to a true whitelabel solution. In this model, the partner's development team builds the entire user interface from scratch using their own technology stack (e.g., ASP.NET Core with Blazor or React). All user interactions—currency selection, amount input, payment method display—happen within the partner's native application interface. MoonPay's REST API is used as a backend engine to power these components, fetch quotes, and execute transactions. The end-user interacts only with the partner's brand, with MoonPay operating completely invisibly in the background.

### **4.2 Achieving a "Fully Branded" Experience: UI/UX Control and Branding Artifacts**

A headless, API-first integration gives the.NET team complete ownership over the front-end experience.

* **Owning the User Interface:** In this model, the team is responsible for designing and building every UI component. For an on-ramp flow, this would include:  
  * A currency selection screen, dynamically populated by calling MoonPay's /v3/currencies API endpoint.  
  * An amount input field, with validation rules enforced by data from the /v3/currencies/:currencyCode/limits endpoint.  
  * A quote display that clearly shows the user the final amount they will receive and the fee breakdown, powered by the /v3/transactions/buy\_quote endpoint.  
  * A transaction history or status tracking page, with data driven by asynchronous webhook events.

The primary goal of this approach is to make the process of buying or selling crypto feel like an intrinsic feature of the partner's application, not an external service. This is achieved by using MoonPay's APIs to pre-fill every possible piece of information—such as the user's email, the destination walletAddress, and the baseCurrencyAmount—to minimize user input, reduce friction, and skip as many steps as possible in the underlying MoonPay flow.36 As demonstrated by the Changelly case study, reducing friction has a direct and significant positive impact on conversion rates.23

* **Managing Branding Artifacts:** While a headless integration provides full control over the application UI, some MoonPay branding may persist in ancillary communications. For example, transaction confirmation emails or descriptors on a user's bank statement might mention MoonPay. The documentation notes that a partner's logo can be added to automated emails "upon request" 32, which implies that this is a configurable part of a managed partnership. Achieving the complete removal or replacement of these final branding artifacts is typically a matter of commercial negotiation with MoonPay's enterprise sales team.

### **4.3 Analyzing the Whitelabel Pricing Model and Calculating ROI**

A true whitelabel solution is a premium offering, and its pricing structure reflects the additional value and support provided.

* **Pricing Structure:** MoonPay's standard pricing is a transaction-based fee model, typically ranging from 1% to 4.5%, plus applicable network fees and spreads, which can vary by payment method, region, and other factors.3 For advanced integrations, MoonPay offers "Enterprise solutions with customizable fee arrangements" and "Premium pricing for fully branded solutions".3 The exact pricing for a whitelabel, API-first integration is not public and requires direct engagement with the MoonPay sales team.7  
* **Revenue Sharing and Ecosystem Fees:** The partnership model can also be a source of revenue. Partners can earn a flexible affiliate fee on transactions processed through their integration, typically ranging from 0.5% to 1.25%.52 Furthermore, some partners choose to add their own "Ecosystem Fee" on top of MoonPay's charges, which is disclosed to the user during checkout and collected by MoonPay on the partner's behalf.50  
* **Calculating Return on Investment (ROI):** The business justification for the premium cost of a whitelabel solution lies in the potential for significantly higher user conversion and transaction volume. A seamless, fully native user experience reduces friction and builds trust, leading to more completed transactions. The ROI calculation for the business would involve comparing the incremental revenue generated by this conversion lift against the premium fees of the whitelabel plan.  
  A simplified model would be:  
  ROI \= (Increase in Transaction Volume × Average Transaction Value × (Conversion Lift % × Revenue Share %)) \- Premium Whitelabel Fees  
  Given the 48% conversion lift seen by Changelly simply by creating a more direct user journey 23, it is evident that a fully native, API-driven experience has the potential to generate a substantial return that can easily justify the investment in a premium whitelabel partnership. The technical implementation of a headless API is therefore not just an engineering decision but a direct enabler of a key business strategy. This underscores the importance of the development team understanding that "whitelabel" is not a binary feature but a negotiated partnership. The level of branding removal and the associated commercial terms are directly linked, requiring close collaboration between the technical team and their business and legal counterparts from the outset of the project.

## **Section 5: Security and Compliance Deep Dive for Enterprise Systems**

For any enterprise integrating a financial service, security and compliance are non-negotiable. This section provides the necessary assurances for a.NET development team by clarifying the shared responsibility model and detailing the robust security posture that a partner inherits when integrating with MoonPay.

### **5.1 Inheriting Trust: A Technical Review of MoonPay's Certifications**

Integrating with MoonPay allows a partner to leverage its significant investment in security and compliance, inheriting a foundation of trust validated by rigorous, internationally recognized standards.

* **PCI DSS 4.0 Level 1:** By integrating MoonPay, the partner's system is removed from the scope of handling raw payment card information. MoonPay's infrastructure is certified at the highest level of the Payment Card Industry Data Security Standard, meaning it manages all sensitive cardholder data within its own secure and compliant environment.2 This dramatically reduces the partner's own PCI compliance burden, cost, and risk.  
* **SOC 2 Type 2:** This certification provides independent, third-party validation that MoonPay has established and maintains effective operational controls related to security, availability, processing integrity, confidentiality, and privacy over an extended audit period.3 For an enterprise partner, this report offers crucial assurance about the reliability and security of the service they are integrating.  
* **ISO 27001 & ISO 27018:** These certifications demonstrate adherence to the global gold standards for information security management. ISO 27001 certifies a comprehensive Information Security Management System (ISMS), while ISO 27018 provides specific controls for the protection of Personally Identifiable Information (PII) in public cloud environments.17  
* **Global Compliance Framework:** MoonPay operates as a regulated entity where required, holding registrations with bodies like the UK's FCA and a license under the EU's MiCA framework.15 This means MoonPay assumes the responsibility for conducting KYC/AML checks in accordance with the regulations of over 160 supported countries, abstracting this immense legal and operational complexity away from the partner.2

### **5.2 Data Flow and the Shared Responsibility Model**

A successful and secure integration requires a clear understanding of which party is responsible for each aspect of the system. The following matrix outlines this shared responsibility model, providing clarity for architectural design, risk assessment, and legal review.

Table 5.2: Security & Compliance Responsibility Matrix  
| Function | Managed by MoonPay | Managed by Partner (.NET Application) |  
| :--- | :--- | :--- |  
| User Interface (UI/UX) | N/A (in headless model) | Responsible. The partner builds, maintains, and secures the entire user-facing application. |  
| API Key Security | Provides the keys. | Responsible. The partner must securely store the secret API key and webhook secret on the server-side (e.g., Azure Key Vault) and never expose them client-side. |  
| Webhook Endpoint Security | Sends signed requests. | Responsible. The partner must secure their webhook listener endpoint (e.g., against DDoS attacks) and implement mandatory signature validation to ensure authenticity. |  
| KYC/AML Processing | Responsible. MoonPay conducts all identity and background checks required by global regulations. | Provides user data to MoonPay for verification. |  
| Payment Card Data (PCI DSS) | Responsible. MoonPay's PCI DSS compliant infrastructure handles all collection, storage, and processing of sensitive cardholder data. | The partner's system is kept out of PCI scope by never touching raw card data. |  
| Fraud & Chargeback Risk | Responsible. MoonPay's risk engines analyze transactions for fraud, and MoonPay absorbs the financial liability for chargebacks. | N/A |  
| Transaction Execution & Liquidity | Responsible. MoonPay manages the complexities of sourcing liquidity and executing the final crypto transaction on the blockchain. | N/A |  
| Data Encryption in Transit | Shared. MoonPay enforces TLS 1.2+. | Shared. The partner's.NET backend must also use modern TLS protocols when communicating with both its clients and the MoonPay API. |  
| User Data Encryption at Rest | Responsible. MoonPay encrypts all data it stores (e.g., user profiles, transaction history) using AES-256. | Responsible. The partner must implement its own encryption for data stored in its own databases. |

### **5.3 A Developer's Guide to MoonPay's Security Features**

Beyond certifications, MoonPay implements a range of security measures that contribute to the platform's overall resilience.

* **Data Encryption:** All data transmitted to or from MoonPay's infrastructure is encrypted in transit using strong TLS 1.2+ protocols. All data stored within MoonPay's systems is encrypted at rest using the AES-256 standard.9  
* **Non-Custodial Service:** MoonPay operates on a non-custodial basis for on-ramp and off-ramp transactions. This means it does not hold or custody customer cryptocurrency. Assets are transferred directly from MoonPay to the user-specified wallet address, minimizing risk.53  
* **Secure Software Development Lifecycle (SDLC):** MoonPay integrates security throughout its development process. This includes mandatory code reviews for all changes, the use of Static Application Security Testing (SAST) tools to identify insecure code patterns, automated dependency scanning to patch vulnerabilities in third-party libraries, and regular security training for its engineering teams.53  
* **Vulnerability Management and Penetration Testing:** The company actively seeks to identify and remediate vulnerabilities. This is supported by regular penetration tests conducted by independent third-party security firms and a public bug bounty program hosted on HackerOne, which invites the global security research community to find and report potential issues.16

## **Conclusion and Strategic Recommendations**

MoonPay offers a mature, secure, and comprehensive Financial Infrastructure-as-a-Service platform that extends far beyond a simple on-ramp widget. For a.NET development team, it represents a powerful opportunity to integrate sophisticated fiat-to-crypto capabilities into an enterprise application, abstracting away the immense complexity of liquidity, global payments, and regulatory compliance. By adopting a REST API-first strategy, a team can achieve a true whitelabel solution, providing a fully branded and seamless user experience that is proven to drive higher conversion rates. This approach allows the development team to focus on its core competency—building innovative products—while leveraging MoonPay's specialized infrastructure as a strategic accelerator.

Based on this analysis, the following actionable recommendations are provided for a.NET team tasked with this integration:

1. **Prioritize the API-First Strategy:** To meet the goal of a true whitelabel solution with maximum control over the user experience, the team should forgo the widget and SDK-based approaches. The focus should be on building a headless integration by communicating directly with MoonPay's REST API from the.NET backend.  
2. **Secure the Backend Implementation:** The highest security priorities for the partner's development team are the protection of API credentials and the validation of incoming webhooks. The secret API key and webhook secret must be stored in a secure vault (e.g., Azure Key Vault) and never exposed client-side. The webhook listener endpoint must implement mandatory, constant-time signature validation for every incoming request before processing its payload.  
3. **Engage Business Stakeholders Early:** The technical implementation of a whitelabel solution is only one part of the equation. The development team should work closely with business, legal, and partnership stakeholders from the project's inception. This collaboration is essential for negotiating the commercial terms of the enterprise/whitelabel partnership with MoonPay, including the premium pricing structure and the precise scope of branding removal from ancillary communications like emails and bank descriptors.  
4. **Adopt a Phased Integration Roadmap:** A structured, phased approach will mitigate risk and ensure a successful rollout.  
   * **Phase 1: Sandbox Proof of Concept (PoC):** Begin by integrating with MoonPay's free sandbox environment.3 The primary goal of this phase is to build and test the core C\# logic for API authentication (URL signing) and secure webhook handling. This allows the team to validate the architecture and resolve technical challenges without financial risk.  
   * **Phase 2: Pilot Launch:** Once the PoC is successful, move to the production environment with live API keys obtained from MoonPay.55 Launch the integration in a limited capacity, perhaps to a small subset of users or with lower transaction limits. This phase is crucial for testing the end-to-end flow with real payment methods and for monitoring system performance and user conversion metrics.  
   * **Phase 3: Full Rollout and Optimization:** Following a successful pilot, the integration can be scaled across the entire user base. In this phase, the team should leverage the full suite of MoonPay's API features to continuously optimize the user journey, pre-filling data and removing friction points to maximize conversion and deliver a best-in-class financial services experience.

#### **Works cited**

1. MoonPay launches new Web3 tool platform for brands venturing into crypto | The Block, accessed June 19, 2025, [https://www.theblock.co/post/296949/moonpay-web3-tool-platform](https://www.theblock.co/post/296949/moonpay-web3-tool-platform)  
2. Is MoonPay Safe? Security, Fees & User Protection Explained \- The Investors Centre, accessed June 19, 2025, [https://www.theinvestorscentre.co.uk/reviews/moonpay-review/is-moonpay-safe/](https://www.theinvestorscentre.co.uk/reviews/moonpay-review/is-moonpay-safe/)  
3. MoonPay | Avalanche Builder Hub, accessed June 19, 2025, [https://build.avax.network/integrations/moonpay](https://build.avax.network/integrations/moonpay)  
4. NORBr's White Label Payment Gateway with MoonPay, accessed June 19, 2025, [https://norbr.com/payment-providers/moonpay/](https://norbr.com/payment-providers/moonpay/)  
5. MoonPay \- Wikipedia, accessed June 19, 2025, [https://en.wikipedia.org/wiki/MoonPay](https://en.wikipedia.org/wiki/MoonPay)  
6. MoonPay, accessed June 19, 2025, [https://dev.moonpay.com/](https://dev.moonpay.com/)  
7. How to integrate MoonPay into my website/app/project?, accessed June 19, 2025, [https://support.moonpay.com/customers/docs/how-to-integrate-moonpay-into-my-website-app-project](https://support.moonpay.com/customers/docs/how-to-integrate-moonpay-into-my-website-app-project)  
8. On-Ramp | The best solution for buying crypto on your site \- MoonPay, accessed June 19, 2025, [https://www.moonpay.com/business/onramps](https://www.moonpay.com/business/onramps)  
9. Moonpay \- Akurateco, accessed June 19, 2025, [https://akurateco.com/payment-methods/moonpay](https://akurateco.com/payment-methods/moonpay)  
10. MoonPay's supported payment methods, accessed June 19, 2025, [https://support.moonpay.com/customers/docs/all-supported-payment-methods](https://support.moonpay.com/customers/docs/all-supported-payment-methods)  
11. MoonPay crypto onramping \- Magic Link, accessed June 19, 2025, [https://magic.link/docs/wallets/integrations/moonpay-integration](https://magic.link/docs/wallets/integrations/moonpay-integration)  
12. What is MoonPay? \- MoonPay Support, accessed June 19, 2025, [https://support.moonpay.com/customers/docs/what-is-moonpay](https://support.moonpay.com/customers/docs/what-is-moonpay)  
13. Web3 Platform \- MoonPay, accessed June 19, 2025, [https://www.moonpay.com/business/web3](https://www.moonpay.com/business/web3)  
14. MoonPay Checkout, accessed June 19, 2025, [https://www.moonpay.com/business/nfts](https://www.moonpay.com/business/nfts)  
15. Written evidence submitted by MoonPay \- UK Parliament Committees, accessed June 19, 2025, [https://committees.parliament.uk/writtenevidence/114746/pdf/](https://committees.parliament.uk/writtenevidence/114746/pdf/)  
16. Trust Center \- MoonPay, accessed June 19, 2025, [https://security.moonpay.com/faq](https://security.moonpay.com/faq)  
17. MoonPay: Trust Center, accessed June 19, 2025, [https://security.moonpay.com/](https://security.moonpay.com/)  
18. MoonPay earns PCI DSS 4.0 certification for better security \- The Paypers, accessed June 19, 2025, [https://thepaypers.com/cryptocurrencies/moonpay-earns-pci-dss-40-certification-for-better-security--1272871](https://thepaypers.com/cryptocurrencies/moonpay-earns-pci-dss-40-certification-for-better-security--1272871)  
19. MoonPay earns PCI DSS 4.0 certification, accessed June 19, 2025, [https://www.moonpay.com/newsroom/pci-dss-4](https://www.moonpay.com/newsroom/pci-dss-4)  
20. How MoonPay completes vendor security reviews 2x faster with Vanta, accessed June 19, 2025, [https://www.vanta.com/customers/moonpay](https://www.vanta.com/customers/moonpay)  
21. How Bitcoin.com increased revenue by over 500% | MoonPay, accessed June 19, 2025, [https://www.moonpay.com/business/case-studies/bitcoincom](https://www.moonpay.com/business/case-studies/bitcoincom)  
22. How BRD boosted user signups and conversion \- MoonPay, accessed June 19, 2025, [https://www.moonpay.com/business/case-studies/brd](https://www.moonpay.com/business/case-studies/brd)  
23. How Changelly significantly increased conversions \- MoonPay, accessed June 19, 2025, [https://www.moonpay.com/business/case-studies/changelly](https://www.moonpay.com/business/case-studies/changelly)  
24. MoonPay successfully scales partnership with Zero Hash across past 4 years, accessed June 19, 2025, [https://zerohash.com/wp-content/uploads/2024/07/MoonPay-case-study-2024-Web.pdf](https://zerohash.com/wp-content/uploads/2024/07/MoonPay-case-study-2024-Web.pdf)  
25. MoonPay Acquires Iron to Add Enterprise-Grade Stablecoin Solutions | PYMNTS.com, accessed June 19, 2025, [https://www.pymnts.com/acquisitions/2025/moonpay-acquires-iron-to-add-enterprise-grade-stablecoin-solutions/](https://www.pymnts.com/acquisitions/2025/moonpay-acquires-iron-to-add-enterprise-grade-stablecoin-solutions/)  
26. MoonPay acquires Iron (Stablecoin Payments Infrastructure) \- 733Park, accessed June 19, 2025, [https://www.733park.com/moonpay-acquires-iron-stablecoin-payments-infrastructure](https://www.733park.com/moonpay-acquires-iron-stablecoin-payments-infrastructure)  
27. Mastercard and MoonPay team up to mainstream stablecoin payments, accessed June 19, 2025, [https://www.mastercard.com/news/press/2025/may/mastercard-and-moonpay-team-up-to-mainstream-stablecoin-payments/](https://www.mastercard.com/news/press/2025/may/mastercard-and-moonpay-team-up-to-mainstream-stablecoin-payments/)  
28. Mastercard partners with MoonPay to boost stablecoin payments | Digital Watch Observatory, accessed June 19, 2025, [https://dig.watch/updates/mastercard-partners-with-moonpay-to-boost-stablecoin-payments](https://dig.watch/updates/mastercard-partners-with-moonpay-to-boost-stablecoin-payments)  
29. MoonPay is live on Orderly Network, accessed June 19, 2025, [https://www.moonpay.com/newsroom/orderly](https://www.moonpay.com/newsroom/orderly)  
30. Introducing the MoonPay SDK, accessed June 19, 2025, [https://www.moonpay.com/newsroom/moonpay-sdk](https://www.moonpay.com/newsroom/moonpay-sdk)  
31. Integrating the Widget, accessed June 19, 2025, [https://dev.moonpay.com/v1.0/docs/integrating-the-widget](https://dev.moonpay.com/v1.0/docs/integrating-the-widget)  
32. FAQ \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/docs/faqs](https://dev.moonpay.com/docs/faqs)  
33. How to go live with MoonPay?, accessed June 19, 2025, [https://support.moonpay.com/customers/docs/how-to-go-live-with-moonpay](https://support.moonpay.com/customers/docs/how-to-go-live-with-moonpay)  
34. MoonPay API \- Developer docs, APIs, SDKs, and auth. | API Tracker, accessed June 19, 2025, [https://apitracker.io/a/moonpay-io](https://apitracker.io/a/moonpay-io)  
35. Get partner account with its API keys, accessed June 19, 2025, [https://dev.moonpay.com/reference/getpartneraccount](https://dev.moonpay.com/reference/getpartneraccount)  
36. Integration, accessed June 19, 2025, [https://dev.moonpay.com/docs/checkout-integrating](https://dev.moonpay.com/docs/checkout-integrating)  
37. Quickstart \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/docs/quickstart](https://dev.moonpay.com/docs/quickstart)  
38. HMACSHA256 Class (System.Security.Cryptography) | Microsoft Learn, accessed June 19, 2025, [https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha256?view=net-9.0](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha256?view=net-9.0)  
39. Hashing and Validation of HMAC-SHA256 in C\# Implementation \- MojoAuth, accessed June 19, 2025, [https://mojoauth.com/hashing/hmac-sha256-in-c](https://mojoauth.com/hashing/hmac-sha256-in-c)  
40. C\# HMAC signing example \- Veracode Docs, accessed June 19, 2025, [https://docs.veracode.com/r/c\_hmac\_signing\_example\_c\_sharp](https://docs.veracode.com/r/c_hmac_signing_example_c_sharp)  
41. API Reference \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/docs/api-reference](https://dev.moonpay.com/docs/api-reference)  
42. Integration design guide \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/docs/design-your-integration](https://dev.moonpay.com/docs/design-your-integration)  
43. Integration design guide \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/docs/design-your-off-ramp-integration](https://dev.moonpay.com/docs/design-your-off-ramp-integration)  
44. Overview \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/reference/reference-webhooks-overview](https://dev.moonpay.com/reference/reference-webhooks-overview)  
45. Integration Guide \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/docs/virtual-accounts-integration-guide](https://dev.moonpay.com/docs/virtual-accounts-integration-guide)  
46. Request signing \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/reference/reference-webhooks-signature](https://dev.moonpay.com/reference/reference-webhooks-signature)  
47. Webhook Signatures \- Getting Started \- Tive, accessed June 19, 2025, [https://developers.tive.com/docs/webhook-signatures](https://developers.tive.com/docs/webhook-signatures)  
48. Receive Webhooks with C\# · Svix, accessed June 19, 2025, [https://www.svix.com/guides/receiving/receive-webhooks-with-c-sharp/](https://www.svix.com/guides/receiving/receive-webhooks-with-c-sharp/)  
49. How to validate an HMAC signature \- Docusign Developer Center, accessed June 19, 2025, [https://developers.docusign.com/platform/webhooks/connect/validate/](https://developers.docusign.com/platform/webhooks/connect/validate/)  
50. Pricing Disclosure \- MoonPay, accessed June 19, 2025, [https://www.moonpay.com/legal/pricing\_disclosure](https://www.moonpay.com/legal/pricing_disclosure)  
51. Pricing Disclosure \- MoonPay, accessed June 19, 2025, [https://www.moonpay.com/legal/europe\_pricing\_disclosure](https://www.moonpay.com/legal/europe_pricing_disclosure)  
52. Transak Vs MoonPay: Which Crypto Payments Gateway Is Best for Your DApp? \- Binance, accessed June 19, 2025, [https://www.binance.com/en/square/post/9060345184265](https://www.binance.com/en/square/post/9060345184265)  
53. MoonPay Security, accessed June 19, 2025, [https://www.moonpay.com/security](https://www.moonpay.com/security)  
54. MoonPay: Buy and sell Bitcoin, Ethereum, and other cryptos, accessed June 19, 2025, [https://www.moonpay.com/](https://www.moonpay.com/)  
55. Ramps launch guide \- MoonPay's Developer Documentation, accessed June 19, 2025, [https://dev.moonpay.com/v1.0/docs/ramps-launch-guide](https://dev.moonpay.com/v1.0/docs/ramps-launch-guide)