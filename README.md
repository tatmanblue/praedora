<img src="docs/branding/praedora_logo.svg" alt="Praedora" width="320" />

*noun, invented Latin, from praedator ("hunter") — stalks the job listings so you don't have to refresh your inbox forty times a day.*

Praedora is a self-hosted job application tracker that does the one thing every spreadsheet secretly refuses to do: it reads your email and updates itself. No more manually copy-pasting "unfortunately, at this time..." into a status column. No more forgetting you even applied somewhere until the rejection shows up eight weeks later like a ghost.

You apply. Praedora watches. When something changes, it knows.

## Getting started

```
git clone <this repo>
cd praedora
cp .env.example .env   # fill in at least PRAEDORA_DB_CONNECTION
cd src/Praedora.AppHost
dotnet run
```

That starts the API, the web UI, and the Aspire dashboard together. From there:

- **[Getting Started](docs/Getting_Started.md)** — prerequisites, full `.env` reference, building,
  running, testing, and loading the Chrome extension.
- **[User Guide](docs/User_Guide.md)** — how the app actually works, page by page, with
  screenshots: the Board, capturing a job, the Review Queue, Application Details, and Admin.

## What it actually does

- **Tracks your pipeline** — a kanban board of every application, from "applied" to "offer" to "well, that's a rejection." Standard stuff, but done right.
- **Reads your inbox so you don't have to** — an LLM classifier watches for job-related emails, figures out what they mean (interview request? rejection? radio silence disguised as an "update"?), and matches them to the right application automatically.
- **Keeps receipts** — every status change is logged with a timestamp and the email snippet that triggered it, so you always know *why* a card moved, not just that it did.
- **Doesn't trust itself blindly** — low-confidence detections land in a "needs review" queue instead of silently rewriting your board. You get a quick confirm/dismiss, not a surprise.
- **Remembers the job description** — full JD text plus a structured extract (skills, salary range, location, remote policy) saved at the moment you add the role, so you're not digging through a dead posting URL three weeks later trying to remember what they actually asked for.
- **Captures a job in one click** — a Chrome extension reads the posting off the page you're looking at and sends it straight to Praedora as a draft, extraction and all, so you never have to copy-paste a JD by hand.

## Why this exists

Every mainstream job tracker (Teal, Huntr, Simplify, etc.) is great at manual tracking and mediocre-to-absent at the one part that's actually tedious: parsing your inbox. Praedora exists to close that specific gap, not to reinvent the kanban board.

## Technology used

| Layer | Choice | Why |
|---|---|---|
| API | ASP.NET Core (minimal APIs) | Fast, familiar, plays nicely with everything below it |
| Frontend | Blazor Server | Live UI updates without hand-rolling a JS framework |
| Live updates | Client-side polling today; SignalR hub scaffolded (planned) | The Board and Review Queue poll the API every few seconds — good enough for a single-user tool; a SignalR push path exists in skeleton form for later |
| Persistence | EF Core + SQLite | Boring and reliable, which is exactly what you want from a database — Postgres/SQL Server aren't implemented yet |
| Email ingestion | Gmail API (OAuth, on-demand + timer polling) | One provider done well beats two done half-heartedly |
| Email classification | Claude (`claude-opus-5` by default, overridable) | Structured JSON output — decides whether an email is job-related and what it means |
| Job description extraction | Claude (`claude-opus-5` by default, overridable) | Turns a raw posting into skills/salary/location/remote-policy fields, one call per capture |
| Background processing | ASP.NET Core `BackgroundService` (timer-based) | Sequential per-sync processing, no broker or queue needed for a single-user tool |
| Optional desktop shell (planned) | .NET MAUI Blazor Hybrid | Would reuse the same Razor components, adding a tray icon and native OS notifications — not built yet |

Single language, single ecosystem, top to bottom: C#/.NET. No context-switching required to work on any layer of this thing.

## Status

Actively hunting. 🎯

## File Version

2026.09.04