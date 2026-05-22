# Building Arctos

The current public solution build is expected to work as a normal entry point.

## Verified During Release Prep

The following full solution build succeeded:

```powershell
dotnet build "Arctos 2.sln" -v:minimal
```

The following project build also succeeded:

```powershell
dotnet build "Arctos.Wpf\Arctos.Wpf.csproj" -v:minimal
```

## Notes

- The supported public story should describe normal `Build` usage, not forced internal targets such as `-t:Compile`.
- Some projects may continue to move in or out of the public solution as newer work is split into separate branches.
