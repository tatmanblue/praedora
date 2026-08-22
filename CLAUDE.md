# Project

praedora is a application(s) used to help make job searching less tedious by managing some of the data gathered and used during the search, including integration with Chrome and email.

## Instructions for Claude

All changes must be approved before creating these changes.  Please prepare a plan of proposed changes and get confirmation before proceeding.

All configuration settings must come from either .env file or from the configuration database.  No hardcoded values for keys and other sensitive information.

## Tech stack
- .NET 10.0, Blazor Server, .NET Aspire (AppHost V2)
- UI: Blazor.Bootstrap, TinyMCE.Blazor
- Logging: Serilog (file sink)
- Config: DotNetEnv (.env files)


## Coding Conventions
- Modern C# features enabled: primary constructors, implicit usings, nullable reference types (#nullable enable)
- Blazor components use code-behind pattern (.razor + .razor.cs)
- Dependency injection via [Inject] in Blazor, constructor injection elsewhere
- Database operations go through IDatabase / IUserDatabase interfaces in Interfaces/
- Factory methods preferred for model creation (e.g., BlogPost.Create())
- Route constants live in General/Constants.cs — add new routes there

## Specific Coding Style Instructions
Please adhere to standard C# conventions unless instructed elsewhere.  If there appears to be conflicts or discrenpancies, please raise these during planning.

- do not use var
- do not use underscore _ for class instance variables

## Build & Run
- Set up a .env (uses DotNetEnv)
- Run via the AppHost V2 Aspire project for full orchestration


## Doc version

2026.08.20