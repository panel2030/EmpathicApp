# Empathic Cultural Network — Architecture

## Architectural style

EmpathicApp is a **Clean Architecture modular monolith**. It stays as one deployable application while enforcing dependency boundaries that make future extraction of infrastructure services possible without prematurely introducing microservices.

## Dependency rule

```text
Empathic.Api
    |
    v
Empathic.Application
    |
    v
Empathic.Domain

Empathic.Infrastructure --> Empathic.Application + Empathic.Domain
Empathic.Api -----------> Empathic.Infrastructure (composition only)
```

The Domain has no dependency on ASP.NET Core, persistence, blockchain SDKs, storage providers, or payment providers.

## Projects

### Empathic.Domain
Owns core cultural concepts:
- Creator
- CulturalWork
- creator verification state
- provenance state carried by the work

### Empathic.Application
Owns use cases and ports:
- creator registration and lookup
- cultural-work registration
- SHA-256 duplicate protection
- provenance anchoring workflow
- public verification lookup
- dashboard aggregation
- repository, hashing, and blockchain interfaces

### Empathic.Infrastructure
Implements external concerns:
- JSON persistence adapter for the MVP
- SHA-256 hashing adapter
- development blockchain anchor adapter

The JSON store is intentionally temporary. It implements repository interfaces so PostgreSQL/EF Core can replace it without changing use cases or API endpoints.

### Empathic.Api
Acts as the composition root and HTTP host:
- configures dependency injection
- exposes REST endpoints
- serves the static browser UI
- exposes `/health` for deployment checks

## Current provenance flow

```text
Creator
  -> Register Cultural Work
  -> Canonical fingerprint material
  -> SHA-256
  -> Duplicate check
  -> Off-chain persistence
  -> IBlockchainAnchorService
  -> Development anchor / future EVM adapter
  -> Transaction reference
  -> Public verification
```

## Blockchain principle

Store proof on-chain, not personal data or full creative works. The public-chain payload should be limited to compact provenance evidence such as the content hash, work identifier, registrant address and timestamp. Copyrighted media, licensing documents and personal data remain off-chain.

## Current smart-contract reference

`contracts/EmpathicProvenanceRegistry.sol` demonstrates the target EVM registry shape. The running application still uses `DevelopmentBlockchainAnchorService`; it does not claim that simulated transaction hashes are public-chain transactions.

## Production evolution

1. PostgreSQL + EF Core repository adapters
2. OpenID Connect authentication
3. verified creator/institution workflow with authorization
4. object storage for source files and media
5. direct file hashing rather than text fingerprint material
6. Polygon/EVM anchor adapter with managed signing
7. audit log, GDPR retention and deletion controls
8. QR/PDF provenance certificates
9. rights/licensing module
10. patronage, membership and payment modules

## Extraction strategy

Do not split into microservices by default. Extract a component only when independent scaling, security isolation, deployment cadence or operational ownership justifies it. Likely first extraction candidates are blockchain anchoring workers, notifications and payment processing.
