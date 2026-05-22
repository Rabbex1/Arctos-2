# Public Release Checklist

## Must Fix Or Decide

- Remove tracked generated/cache artifacts from the repository.

## Done In This Release-Prep Pass

- Added a root `README.md`.
- Added a root `BUILDING.md`.
- Added this release checklist.
- Added a root MIT `LICENSE`.
- Added ignore rules for tracked cache artifacts.
- Added ignore rules for local `*.DotSettings` files.
- Removed tracked `*.lscache` files.
- Removed tracked project-level `*.DotSettings` files.
- Removed local diagnostic/build investigation artifacts from the repo root.
- Removed placeholder files that did not belong in the public repo.
- Verified the full solution builds cleanly with `dotnet build "Arctos 2.sln" -v:minimal`.

## Nice To Have

- Add a root architecture note explaining how `Arctos`, `Arctos.Shell`, `Arctos.Wpf`, and the supporting libraries fit together.
- Add minimal contribution guidance once the build story is cleaner.
- Add tests for the most reusable non-UI layers if the intent is to attract outside contributors.
- Audit older naming and spelling inconsistencies only after the repo is safely public.
