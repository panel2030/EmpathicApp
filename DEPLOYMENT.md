# Empathic Movement — Deployment Guide

The application is a .NET 9 web app. The public website, legal pages, EMPATH pre-launch portal and provenance prototype are served from the same ASP.NET Core process, so there is no separate front-end build step.

## What is deployed

- `/` — public Empathic Movement website
- `/join.html` — movement registration page
- `/token-launch.html` — EMPATH pre-launch and founder-allocation preview
- `/platform.html` — provenance registry prototype
- `/privacy.html` and `/terms.html` — prototype legal pages
- `/api/token/launch-config` — public token-launch display configuration
- `/api/*` — creator, work, movement, verification and dashboard APIs
- `/health` — health endpoint
- `/App_Data` — server-side JSON persistence used by the current prototype

> The current blockchain anchor service is a development adapter. The EMPATH launch page is also intentionally non-transactional. Do not describe either as a live public-chain token sale until production smart contracts, custody, legal review and jurisdiction controls are complete.

## Option A — Docker Compose (recommended for a VPS)

Requirements: Docker Engine and Docker Compose.

```bash
git clone https://github.com/panel2030/EmpathicApp.git
cd EmpathicApp
cp .env.example .env
```

Edit `.env` to set the **public receiving addresses only** for the two founder allocation slots. Do not place private keys, seed phrases or signing credentials there.

```dotenv
TOKEN_FOUNDER1_LABEL=Menotti Lerro
TOKEN_FOUNDER1_WALLET=0xPUBLIC_ADDRESS_ONE
TOKEN_FOUNDER1_PERCENT=50
TOKEN_FOUNDER2_LABEL=FNG
TOKEN_FOUNDER2_WALLET=0xPUBLIC_ADDRESS_TWO
TOKEN_FOUNDER2_PERCENT=50
```

Then deploy:

```bash
docker compose up -d --build
```

The site will listen on:

```text
http://SERVER_IP:8080
```

Check health:

```bash
curl http://127.0.0.1:8080/health
```

Check token-launch configuration:

```bash
curl http://127.0.0.1:8080/api/token/launch-config
```

View logs:

```bash
docker compose logs -f empathic
```

Update later:

```bash
git pull
docker compose up -d --build
```

The named Docker volume `empathic_data` preserves prototype registry and movement-registration data across container recreation.

## Option B — Run directly with .NET 9

```bash
dotnet restore EmpathicCulturalNetwork.sln
dotnet publish src/Empathic.Api/Empathic.Api.csproj -c Release -o ./publish
ASPNETCORE_URLS=http://0.0.0.0:8080 dotnet ./publish/Empathic.Api.dll
```

When running directly, token-launch display settings can be supplied using standard ASP.NET Core environment variables such as:

```bash
export TokenLaunch__Founder1Label="Menotti Lerro"
export TokenLaunch__Founder1Wallet="0xPUBLIC_ADDRESS_ONE"
export TokenLaunch__Founder1Percent="50"
export TokenLaunch__Founder2Label="FNG"
export TokenLaunch__Founder2Wallet="0xPUBLIC_ADDRESS_TWO"
export TokenLaunch__Founder2Percent="50"
```

For a real server, run the process under systemd or another service manager rather than keeping it in an interactive shell.

## Reverse proxy and HTTPS

Keep ASP.NET Core on localhost/port 8080 and place Caddy, Nginx or another reverse proxy in front of it. Terminate TLS at the proxy.

Example Caddyfile:

```caddy
example.org {
    reverse_proxy 127.0.0.1:8080
}
```

After DNS points the domain to the VPS, Caddy can obtain and renew TLS certificates automatically when the server is reachable on ports 80 and 443.

Example Nginx server block:

```nginx
server {
    listen 80;
    server_name example.org www.example.org;

    location / {
        proxy_pass http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

Use Certbot or your hosting provider's TLS tooling if you choose Nginx.

## Production checklist

Before a public production launch:

1. Replace prototype JSON persistence with a managed database or harden the storage/backup model.
2. Add authentication and role-based authorisation before exposing write operations broadly.
3. Add rate limiting, request-size limits and production security headers.
4. Configure a production blockchain adapter only after wallet/key custody and chain selection are approved.
5. Add monitoring, structured logs and off-server backups.
6. Replace prototype privacy/terms text with jurisdiction-specific legal documents.
7. Keep the token launch non-transactional until regulatory, KYC/AML, smart-contract security, treasury/custody and geographic-eligibility work is complete.
8. Keep founder private keys and seed phrases outside source control, Docker images and browser code.
9. Ensure founder token-allocation percentages total exactly 100% and document vesting separately before any production deployment.
10. Configure the final domain name in DNS and HTTPS before sharing the site publicly.

See `docs/TOKEN_LAUNCH.md` for the EMPATH-specific launch checklist.

## GitHub Actions

`.github/workflows/build.yml` validates the .NET solution, publishes the deployable app, checks that the public website assets are present, builds the Docker image and uploads a release-ready publish artifact.
