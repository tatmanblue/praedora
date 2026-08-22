# Praedora

*noun, invented Latin, from praedator ("hunter") — stalks the job listings so you don't have to refresh your inbox forty times a day.*

Praedora is a self-hosted job application tracker that does the one thing every spreadsheet secretly refuses to do: it reads your email and updates itself. No more manually copy-pasting "unfortunately, at this time..." into a status column. No more forgetting you even applied somewhere until the rejection shows up eight weeks later like a ghost.

You apply. Praedora watches. When something changes, it knows.

**→ [Getting Started](docs/Getting_Started.md)** — how to run it and use what's actually built so far.

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
| API / background processing | ASP.NET Core (minimal APIs) | Fast, familiar, plays nicely with everything below it |
| Frontend | Blazor Server | Live UI updates without hand-rolling a JS framework |
| Real-time updates | SignalR | Pushes status changes to the board the moment they're detected |
| Persistence | EF Core + SQLite/Postgres | Boring and reliable, which is exactly what you want from a database |
| Email ingestion | Gmail API (OAuth, on-demand + timer polling) | One provider done well beats two done half-heartedly |
| Email classification | Claude (Haiku-class model) | Structured JSON output, cheap enough to run on every inbound email |
| Job description extraction | Claude Opus 5 (structured output) | Turns a raw posting into skills/salary/location/remote-policy fields, one call per capture |
| Background queue | `System.Threading.Channels` | In-process, no broker needed for a single-user tool |
| Optional desktop shell | .NET MAUI Blazor Hybrid | Reuses the same Razor components, adds a tray icon and native OS notifications |

Single language, single ecosystem, top to bottom: C#/.NET. No context-switching required to work on any layer of this thing.

## Status

Actively hunting. 🎯

## File Version

2026.08.22