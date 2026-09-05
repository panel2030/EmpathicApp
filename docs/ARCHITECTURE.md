# Empathic Cultural Network — Phase 1 Architecture

## Goal
Create a trustworthy cultural-provenance platform before introducing any tradable token.

## Current MVP flow

Creator -> Creator Registry -> Cultural Work -> SHA-256 Fingerprint -> Provenance Registry -> Blockchain Anchor -> Public Verification

## Components

- **ASP.NET Core 9 API** — creators, works, verification and anchoring endpoints.
- **Static web prototype** — dashboard, creator registration, work registration and verification.
- **PlatformStore** — JSON-backed prototype persistence. Replace with PostgreSQL before production.
- **HashingService** — SHA-256 fingerprints for canonical work material.
- **IBlockchainAnchorService** — chain-neutral interface.
- **DevelopmentBlockchainAnchorService** — safe prototype adapter that generates deterministic-looking development transaction IDs; it does not claim to be a public blockchain transaction.
- **EmpathicProvenanceRegistry.sol** — reference EVM provenance contract for a later testnet deployment.

## Production evolution

1. PostgreSQL + EF Core
2. Authentication via OpenID Connect
3. Creator/institution verification workflow
4. Object storage/IPFS for optional media archival
5. Polygon/Ethereum-compatible anchor adapter using a managed signer
6. Audit log and GDPR/data-retention controls
7. Rights/licensing module
8. Payment provider integration

## Important design principle
Store proof on-chain, not personal data or full creative works. Personal data, licensing documents and copyrighted media remain off-chain.
