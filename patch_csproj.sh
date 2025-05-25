#!/usr/bin/env bash
set -euo pipefail

echo "1/3 ➤ Creating top-level tests directory…"
mkdir -p tests

echo "2/3 ➤ Moving SearchService.Tests into tests/…"
mv search-service/SearchService.Tests tests/SearchService.Tests

echo "3/3 ➤ Patching tests project reference…"
sed -i 's|<ProjectReference Include="\(.*\)\.\./\.\./search-service.csproj"|<ProjectReference Include="../search-service/SearchService.csproj"|' tests/SearchService.Tests/SearchService.Tests.csproj

echo "✅ Done! Your layout is now:

.
├── search-service/
│   ├── SearchService.csproj
│   └── (all your Controllers, Models, Services…)
└── tests/
    └── SearchService.Tests/
        ├── SearchService.Tests.csproj
        └── (all your test files)

Next steps:
  • cd search-service && dotnet build
  • cd ../tests/SearchService.Tests && dotnet test
"
