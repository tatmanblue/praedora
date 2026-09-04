# Getting Started

How to get Praedora's source, configure it, and run it locally. For a tour of what the app
actually does once it's running, see the **[User Guide](User_Guide.md)**.

## 1. Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- The [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) (`aspire`
  on your PATH) — the AppHost project uses it to orchestrate the API and Web projects together
- A Google Cloud project with the Gmail API enabled and an OAuth client, if you want Gmail sync
  (optional — see §2)
- An [Anthropic API key](https://console.anthropic.com/), if you want job-description extraction
  and email classification (optional — see §2)

## 2. Clone and configure

```
git clone <this repo>
cd praedora
cp .env.example .env
```

Edit `.env`. `PRAEDORA_DB_PROVIDER` and `PRAEDORA_DB_CONNECTION` are the only two required
values — the app throws on startup without a connection string:

```
PRAEDORA_DB_PROVIDER=Sqlite
PRAEDORA_DB_CONNECTION=Data Source=praedora.db
```

Only `Sqlite` is supported today; Postgres/SQL Server aren't wired up yet.

Everything else is optional, and the app degrades gracefully without it:

| Variable | Needed for | Behavior if unset |
|---|---|---|
| `GMAIL_OAUTH_CLIENT_ID` / `GMAIL_OAUTH_CLIENT_SECRET` | Reading your inbox for status changes (§4 of the User Guide) | The background sync and "Sync now" both fail with a clear error on the Review Queue page and in the logs. Manual tracking and captures still work fully. |
| `CLAUDE_API_KEY` | Job-description extraction (skills/salary/location) on capture, and email classification for Gmail sync | Captures still succeed; the "Extracted" section on an application's Details tab is just left empty, and Gmail sync can't classify anything even if it's otherwise configured. |
| `CLAUDE_MODEL` | Overriding the extraction model | Defaults to `claude-opus-5`. |

**Never commit `.env`** — it's gitignored, and it holds real credentials.

### Setting up Gmail OAuth (optional)

1. In [Google Cloud Console](https://console.cloud.google.com/), create a project (or reuse one),
   enable the **Gmail API**, and create an **OAuth 2.0 Client ID** of type "Desktop app."
2. Copy the client ID and secret into `.env`.
3. The first time anything triggers a sync (the background timer or the "Sync now" button),
   `GmailEmailProvider` opens your default browser for the Google consent screen. Approve it once
   — the resulting token is cached in the database (`AppSettings` table) and reused silently after
   that.

## 3. Build

```
dotnet build Praedora.slnx
```

## 4. Run

Run the `Praedora.AppHost` project — this is the supported way to run Praedora, and starts both
`Praedora.Api` and `Praedora.Web` together along with the Aspire dashboard:

```
cd src/Praedora.AppHost
dotnet run
```

The dashboard prints a URL (typically `https://localhost:17xxx`) — open it to see both resources
and their actual assigned ports, since Aspire allocates them dynamically per run rather than using
fixed ports. Click through to `praedora-web`'s endpoint to open the app itself.

If you instead run `Praedora.Api` or `Praedora.Web` directly with `dotnet run` (bypassing
AppHost), each falls back to the fixed ports in its own `Properties/launchSettings.json`
(`Praedora.Api`: `https://localhost:7111`; `Praedora.Web`: `https://localhost:7064`) — but you'll
need to run both yourself, and `Praedora.Web`'s calls to the API won't resolve without Aspire's
service discovery unless you also adjust its configuration.

On first run, the API applies EF Core migrations automatically and creates `praedora.db` next to
wherever it's running from.

## 5. Run the tests

```
dotnet test Praedora.slnx
```

This covers `Praedora.Core.Tests`, `Praedora.Application.Tests` (service-layer logic against
in-memory fakes), and `Praedora.IntegrationTests` (the capture endpoint against a real, temporary
database).

## 6. Load the Chrome extension (optional)

The extension lives in `src/Praedora.Extension` and isn't published anywhere — load it unpacked:

1. `chrome://extensions` → enable **Developer mode** → **Load unpacked** → select
   `src/Praedora.Extension`.
2. Right-click the extension's icon → **Options** (or use the "API settings" link at the bottom of
   the popup) and set the API base URL to wherever `Praedora.Api` is actually reachable (see §4) —
   the packaged default doesn't match either run mode above, so you'll need to set this once per
   environment.

See the **[User Guide](User_Guide.md#capturing-a-job)** for how to use it day to day.

## Known limitations

- **Sqlite only** — Postgres/SQL Server support isn't implemented yet, despite being read from
  `PRAEDORA_DB_PROVIDER`; any other value throws on startup.
- **Contacts** are modeled in the database and shown on an application's Details tab, but nothing
  in the UI or API creates one yet — the section will always read "No contacts yet."
- **No desktop shell** — the README mentions a possible .NET MAUI Blazor Hybrid wrapper; it
  doesn't exist yet. Praedora runs as a web app today.
- **Single-user, no auth** — there's no login; anyone who can reach the API can use it. This is a
  deliberate local-trust posture (see the CORS comment in `Praedora.Api/Program.cs`), not an
  oversight, but it means you shouldn't expose the API to an untrusted network.

## File Version

2026.09.04
