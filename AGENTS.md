# EmpathicApp repository instructions

EmpathicApp is the digital platform for **The Empathic Movement**. The product combines culture, empathy, provenance, verified participation, AI-assisted experiences and a proposed utility/reward layer named **EMPATH**.

The platform is founded and controlled by the two project founders. Token or credential ownership must never be presented as company equity or automatic ownership of the platform.

## Product intent

Build technology that helps people discover culture, participate in empathy-oriented activities, verify cultural provenance and recognise positive participation.

Core product areas:

- Public Empathic Movement website
- Creator and cultural-work registry
- Digital Product Passports / provenance records
- Proof of Empathy activities and credentials
- Member profiles and dashboards
- EMPATH utility/reward balance
- Cultural content, exhibitions and digital experiences
- AI-assisted cultural discovery and explanation

Do not turn the product into a speculative crypto landing page. EMPATH should be described as a **proposed utility and recognition layer** until a production token launch, legal review and network deployment have actually happened.

## Architecture

The solution is a .NET 9 clean modular monolith with these primary projects:

- `src/Empathic.Domain` — domain entities and rules
- `src/Empathic.Application` — use cases, contracts and application services
- `src/Empathic.Infrastructure` — persistence, hashing and external adapters
- `src/Empathic.Api` — HTTP API and deployable website
- `src/Empathic.Api/wwwroot` — public website and browser UI
- `contracts/` — reference smart contracts and blockchain-related artifacts
- `docs/` — architecture, product and deployment documentation

Respect dependency direction. Domain code must not depend on Infrastructure or web concerns. Application code should depend on abstractions; infrastructure provides implementations.

Prefer small, testable services and explicit interfaces over large controller or endpoint files.

## Website rules

The public website should communicate the movement first and technology second.

Primary positioning:

> Culture × Empathy × Humanity × Technology

Preferred tone:

- human
- culturally respectful
- credible
- internationally understandable
- optimistic without exaggerated claims

Avoid fabricated statistics, fake partners, fake endorsements, invented adoption figures or statements that a feature is live when it is only a prototype.

Do not imply that blockchain itself guarantees truth, legal ownership, copyright, authenticity or regulatory compliance. It can provide tamper-evident evidence only when the source data and verification process are trustworthy.

## EMPATH rules

Until explicitly approved for production deployment:

- EMPATH is a proposed utility/reward concept.
- Do not add a public token sale, ICO purchase flow or guaranteed financial return.
- Do not describe EMPATH as shares, equity, profit participation or ownership of the company.
- Do not promise token appreciation.
- Do not fabricate a contract address, exchange listing, market cap, token price or circulating supply.
- Prototype balances may be represented as internal reward units.

If tokenomics are implemented, keep allocation, vesting, treasury and governance rules configurable rather than hard-coded into marketing pages.

## Proof of Empathy

Proof of Empathy is an **impact-verification workflow**, not a blockchain consensus algorithm.

A typical flow is:

```text
Activity created
    ↓
Participant joins
    ↓
Evidence / attendance recorded
    ↓
Approved verifier checks evidence
    ↓
Credential issued
    ↓
Optional blockchain evidence anchored
    ↓
Recognition / EMPATH reward recorded
```

Verification must be evidence-based. Never let the browser award itself a verified credential or trusted reward without server-side validation.

Credentials representing personal participation should normally be non-transferable.

## Cultural provenance

For each cultural work, preserve at minimum:

- creator identity
- title and work type
- canonical content fingerprint or file hash
- provenance status
- rights/licensing statement
- timestamps
- anchoring network and transaction evidence when applicable

Duplicate content fingerprints should remain detectable.

Do not claim that hashing a work transfers copyright or independently proves authorship. The registry records submitted and verified evidence; legal rights remain subject to applicable law and agreements.

## Blockchain

The current blockchain adapter may operate as a development simulator. Never label simulated transaction identifiers as live Ethereum, Polygon or other public-chain transactions.

For a real network integration:

- use a testnet first
- keep RPC URLs and credentials outside source control
- never commit private keys or seed phrases
- separate signing/custody from browser code
- make network and contract addresses environment-driven
- confirm transaction success before presenting an asset as anchored

Smart-contract changes require tests before production deployment.

## AI features

AI may support cultural explanation, translation, discovery, summarisation and guided experiences.

AI-generated output must not be presented as authoritative provenance, legal advice, historical fact without verification, or a direct quotation from a creator unless the source is available and the wording is accurate.

Keep creator-authored material distinct from AI-generated commentary.

## API and security

- Validate all untrusted input server-side.
- Do not expose administrative operations without authentication/authorization when those features become production-facing.
- Never commit API keys, passwords, signing keys, connection secrets or personal access tokens.
- Avoid storing sensitive personal information unless it is required for a documented feature.
- Escape or safely render user-supplied content in the browser.
- Preserve HTTPS-first production deployment.
- Keep health endpoints free of secrets and internal credentials.

## Persistence

The current JSON-backed store is suitable for prototype/demo use. Treat migration to PostgreSQL or another production database as a separate infrastructure concern and preserve repository abstractions so the domain/application layers do not need redesign.

Any migration should include:

- durable IDs
- unique constraints for content fingerprints where appropriate
- audit timestamps
- concurrency handling
- migration/versioning strategy
- backups

## Validation

Before committing application changes, run:

```bash
dotnet restore EmpathicCulturalNetwork.sln
dotnet build EmpathicCulturalNetwork.sln --configuration Release
```

For deployable changes also run:

```bash
dotnet publish src/Empathic.Api/Empathic.Api.csproj --configuration Release --output ./publish
docker build -t empathicapp:local .
```

When tests exist for the changed area, run them as part of the same validation. New domain rules and security-sensitive application logic should include tests.

The GitHub Action must remain green before merging changes to `main`.

## Deployment

`src/Empathic.Api` is the deployable application and serves the public website from `wwwroot`.

Production deployment should use the existing Docker/VPS guidance in `DEPLOYMENT.md`, persistent storage for `App_Data` while the JSON store is in use, and a reverse proxy with HTTPS.

Do not copy secrets into Docker images or repository files.

## Change discipline

When implementing a feature:

1. Understand the relevant domain rule before editing UI.
2. Keep public claims aligned with what the backend actually supports.
3. Prefer additive, backwards-compatible API evolution for the prototype.
4. Update documentation when architecture, deployment, token positioning or verification behaviour changes.
5. Keep the site accessible and responsive.
6. Do not remove existing provenance/verification functionality unless the replacement is complete.
7. Do not merge with failing CI.

The goal is a credible, deployable empathy-and-culture platform — not a collection of disconnected crypto features.