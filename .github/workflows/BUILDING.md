# How to build and release the ketch EXE using GitHub Actions

This workflow builds a self-contained Windows EXE (x64) using .NET 7 and publishes it as a GitHub Release.

How to run:
- Option 1 (recommended): In GitHub, go to Actions → "Build and release ketch" → Run workflow → choose branch main → Run workflow. The workflow will create a release and attach the EXE.
- Option 2: Create a tag locally and push it (e.g., git tag v1.0.0 && git push origin v1.0.0) — the workflow triggers on tags matching "v*".

Notes:
- The build uses `win-x64`. If you need `win-arm64`, modify the `dotnet publish` args in the workflow.
- If your font license prevents publishing the font in a public release, consider removing the font file from the release artifacts or changing repo visibility.
