# Getting Started

What you can actually do with Praedora today, and how to do it. This reflects build sequence
steps 1–3 in `Praedora_Consolidated_Technical_Design.md` §12 — see the **Not implemented yet**
section at the bottom for what's still stubbed.

## 1. First-time setup

Copy `.env.example` to `.env` in the repo root and fill in what you need:

```
PRAEDORA_DB_PROVIDER=Sqlite
PRAEDORA_DB_CONNECTION=Data Source=praedora.db
```

These two are required — the app throws on startup without `PRAEDORA_DB_CONNECTION`. Everything
else in `.env.example` is optional for now:

- `CLAUDE_API_KEY` / `CLAUDE_MODEL` — only needed if you want job-description extraction (§3
  below). Leave blank and captures still work, just without the extracted skills/salary/location
  fields.
- `GMAIL_OAUTH_CLIENT_ID` / `GMAIL_OAUTH_CLIENT_SECRET` — not read by anything yet (see **Not
  implemented yet**). No point setting these today.
- `PRAEDORA_LISTEN_PORT` — also not read by anything. Ignore it; see §2 for how the actual port
  is determined.

## 2. Running the app

Run the `Praedora.AppHost` project (this is the supported way — see the root `CLAUDE.md`). It
starts both `Praedora.Api` and `Praedora.Web` and opens the Aspire dashboard.

**The API's actual URL is whatever the Aspire dashboard shows for the `praedora-api` resource** —
check its console/endpoint there rather than assuming a fixed port. If you instead run
`Praedora.Api` directly with `dotnet run` (bypassing AppHost), it uses `launchSettings.json`:
`http://localhost:5136` (`https://localhost:7111` for https). `Praedora.Web` similarly defaults
to `http://localhost:5106` / `https://localhost:7064` when run standalone.

## 3. Manual tracking (works today)

Open `Praedora.Web` in a browser — the Kanban board is the home page. **+ New Application** opens
a form (company, role, source URL, job description) that creates the application directly via
`POST /api/applications`. This path does **not** run job-description extraction — that's specific
to the Chrome extension's capture path (§4/§5). Drag-free status changes happen by picking a new
status on a card; every change is recorded in that application's `StatusHistory`.

## 4. Chrome extension — one-click capture

The extension lives in `src/Praedora.Extension` and isn't published anywhere — load it unpacked:

1. `chrome://extensions` → enable **Developer mode** → **Load unpacked** → select
   `src/Praedora.Extension`.
2. Right-click the extension's icon → **Options** (or use the "API settings" link at the bottom
   of the popup) and set the API base URL to whatever you found in §2 — the packaged default
   (`http://localhost:5080`) doesn't match either real scenario described above, so you'll need to
   set this once per environment.
3. Navigate to a job posting and click the extension icon. It looks for a `schema.org/JobPosting`
   JSON-LD block on the page first (present on most boards — LinkedIn, Indeed, Greenhouse, Lever)
   and falls back to the page title/text if there isn't one. Review/edit the pre-filled fields,
   then **Capture**.
4. This posts to `POST /api/capture`, which creates the application (status `Captured`) and — if
   `CLAUDE_API_KEY` is set — attaches the extracted job-description fields in the same request.

## 5. Job description extraction

Only the extension's capture path (§4) runs extraction; it's best-effort — if `CLAUDE_API_KEY` is
missing or the Claude call fails for any reason, the capture still succeeds and `JdExtract` is
just left empty (check the API's Serilog file output in `logs/` for the warning). When it does
run, it uses `claude-opus-5` by default (override with `CLAUDE_MODEL`) and returns structured
fields: required skills, nice-to-have skills, salary range, location, remote policy, and years of
experience.

## Not implemented yet

- **Gmail sync** — `GmailEmailProvider` throws `NotImplementedException`; the background
  `EmailSyncWorker` runs but does nothing every cycle. Setting the `GMAIL_OAUTH_*` env vars has no
  effect today.
- **Email classification** — `ClaudeClassifier` is a stub for the same reason.
- **Review queue** — the `ReviewQueuePanel` page and `/api/review-queue` endpoints exist but
  return 501 / do nothing; there's nothing to review until Gmail sync exists.
- **Database-backed Log Viewer** — `ILogSink`/`DatabaseLogSink` also stub out; use the Serilog
  file sink under `logs/` for diagnostics in the meantime.
- **Postgres / SQL Server / Azure deployment** — Sqlite and local hosting only for now.
