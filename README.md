# The Empathic Movement — Platform Prototype

A deployable .NET 9 platform for the **Empathic Movement**: culture, empathy, humanity and technology in one ecosystem.

The current release combines a premium public website with a working cultural-provenance prototype. Menotti Lerro and FNG are presented as the movement's founding cultural and technology/strategy supporters. **EMPATH** is currently described as a proposed utility and recognition layer—not as a public investment offer.

## What is included

### Public website

- Responsive Empathic Movement landing page
- Movement mission and participation loop
- Proof of Empathy framework explanation
- Cultural engine: poetry, art, heritage and future AI experiences
- EMPATH utility-token concept and launch-status disclaimer
- Founding-supporter profiles
- Product roadmap and FAQ
- Privacy and prototype terms pages
- Live registry statistics when the API is available
- Accessible navigation and reduced-motion support

### Working provenance prototype

- Register creators
- Register cultural works
- Generate SHA-256 content fingerprints
- Prevent duplicate fingerprint registration
- View provenance records
- Anchor a record through a chain-neutral service interface
- Verify a work by content hash
- Dashboard counts
- Reference Solidity provenance contract

> The current blockchain adapter is deliberately a **development ledger simulator**. It does not represent a live Polygon/Ethereum transaction. This keeps the prototype safe and testable until wallet custody, production network and regulatory decisions are made.

## Run locally

Requirements: .NET 9 SDK

```bash
dotnet restore EmpathicCulturalNetwork.sln
dotnet run --project src/Empathic.Api
```

Open the URL printed by ASP.NET Core. The public website is `/` and the working registry is `/platform.html`.

## Deploy with Docker

Requirements: Docker Engine + Docker Compose.

```bash
docker compose up -d --build
```

Then open:

```text
http://localhost:8080
```

Health check:

```bash
curl http://localhost:8080/health
```

The `empathic_data` Docker volume preserves prototype registry data across container recreation.

See **[DEPLOYMENT.md](DEPLOYMENT.md)** for VPS, reverse-proxy and HTTPS instructions.

## Architecture

```text
Browser
  │
  ├── Public movement website
  └── Registry prototype
          │
          ▼
     Empathic.Api (.NET 9)
          │
     Application services
          │
   ┌──────┴────────┐
   ▼               ▼
JSON prototype   Blockchain
persistence      abstraction
                   │
             Development anchor
```

The project follows a clean modular-monolith structure across Domain, Application, Infrastructure and API projects.

## Production priorities

1. Authentication and role-based access control
2. PostgreSQL or another production data store
3. File upload + server-side hashing rather than pasted fingerprint material
4. Production-grade audit logging and monitoring
5. Verified creator/organisation onboarding
6. Proof of Empathy activities, evidence and credentials
7. QR-linked cultural passports
8. AI cultural guide with source-aware responses
9. Production blockchain adapter only after chain/key-custody decisions
10. Jurisdiction-specific legal and token review before any broader EMPATH issuance

## CI/CD

GitHub Actions now:

- restores and builds the .NET solution
- publishes the API + website
- verifies required website assets
- builds the Docker image
- uploads a deployable publish artifact

See `.github/workflows/build.yml`.

## Documentation

- `DEPLOYMENT.md` — deployment and HTTPS
- `docs/ARCHITECTURE.md` — technical architecture
- `docs/MONETIZATION.md` — commercial model
- `contracts/` — reference smart contracts
