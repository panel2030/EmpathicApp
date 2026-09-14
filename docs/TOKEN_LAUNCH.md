# EMPATH pre-launch portal

The repository includes a public EMPATH pre-launch page at `/token-launch.html`.

## Current status

The page is intentionally **non-transactional**. It does not accept money, connect to a buyer wallet, execute a token purchase, issue investment promises or route investor funds. `GET /api/token/launch-config` always reports `saleEnabled: false` in this version.

This keeps the site suitable for community-building, product explanation, tokenomics preparation and testnet planning while legal, regulatory, custody and smart-contract work remains incomplete.

## Two configurable founder allocation slots

The launch page displays two founder token-allocation slots. Defaults are 50% / 50% and the percentages must total 100% for a valid allocation preview.

These values describe **token allocation only**. They are not company equity, legal shareholding or voting ownership in the operating company.

Copy `.env.example` to `.env` and set the public receiving addresses:

```bash
cp .env.example .env
```

Example:

```dotenv
TOKEN_FOUNDER1_LABEL=Menotti Lerro
TOKEN_FOUNDER1_WALLET=0xPUBLIC_ADDRESS_ONE
TOKEN_FOUNDER1_PERCENT=50
TOKEN_FOUNDER2_LABEL=FNG
TOKEN_FOUNDER2_WALLET=0xPUBLIC_ADDRESS_TWO
TOKEN_FOUNDER2_PERCENT=50
```

Then rebuild:

```bash
docker compose up -d --build
```

The values are surfaced by the server at:

```text
GET /api/token/launch-config
```

## Security rule

Only public receiving addresses belong in the environment variables above. Never put any of the following in the repository, browser JavaScript, `.env.example`, Docker image or public API response:

- private keys
- seed phrases
- wallet passwords
- signing keys
- exchange API secrets
- custody credentials

Actual wallet creation and signing/custody should be handled outside the public website using an appropriate secure wallet or custody process.

## Before any real token offering

A production public offering should not be enabled merely by adding a Buy button. At minimum, complete:

1. jurisdiction-specific legal classification and offering review;
2. adult-authorised corporate and founder approvals;
3. KYC/AML/sanctions and geographic eligibility design where required;
4. production tokenomics, supply, allocation and vesting documentation;
5. audited smart contracts and testnet validation;
6. secure custody and treasury controls, preferably with multi-signature approval;
7. transaction reconciliation, refund/error handling and accounting;
8. privacy, consumer disclosures, risk warnings and terms;
9. monitoring for fraud, abuse and suspicious activity;
10. a clear rule for pausing the launch if compliance or security checks fail.

The public page should never claim guaranteed returns, guaranteed liquidity, guaranteed listing or guaranteed token-price appreciation.
