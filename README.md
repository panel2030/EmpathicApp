# Empathic Cultural Network — Phase 1 MVP

A .NET 9 prototype for creator identity, cultural-work provenance, blockchain anchoring and public verification.

## What works now

- Register creators
- Register cultural works
- Generate SHA-256 content fingerprints
- Prevent duplicate fingerprint registration
- View provenance records
- Anchor a record through a chain-neutral service interface
- Verify work by content hash
- Dashboard counts
- Responsive browser UI
- Reference Solidity provenance contract

> The current blockchain adapter is deliberately a **development ledger simulator**. It does not represent a real Polygon/Ethereum transaction. This keeps the prototype safe and testable until wallet custody, network and regulatory decisions are made.

## Run locally

Requirements: .NET 9 SDK

```bash
dotnet restore
dotnet run --project src/Empathic.Api
```

Open the local URL shown by ASP.NET Core.

## Recommended next build

1. PostgreSQL persistence
2. Login + verified creator onboarding
3. Upload/file hashing rather than pasted fingerprint material
4. Real Polygon Amoy testnet anchoring
5. Certificate PDF/QR verification
6. Rights/licensing workflows
7. Cultural membership and patronage module

See `docs/ARCHITECTURE.md` and `docs/MONETIZATION.md`.
