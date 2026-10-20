#!/usr/bin/env bash
set -euo pipefail
cd /workspace/nexacore-pim-iv
export DOTNET_ROOT="${NEXACORE_DOTNET_ROOT:-/workspace/.tools/dotnet}"
export DOTNET_CLI_HOME=/workspace/.tools/dotnet-home
export NUGET_PACKAGES=/workspace/.tools/nuget
export TMPDIR=/workspace/.tools/tmp
export npm_config_cache=/workspace/.tools/npm-cache
export PATH="$DOTNET_ROOT:$PATH"
mkdir -p "$DOTNET_ROOT" "$DOTNET_CLI_HOME" "$NUGET_PACKAGES" "$TMPDIR"
if ! test -x "$DOTNET_ROOT/dotnet" || test "$(dotnet --version)" != 8.0.426; then
  archive="$TMPDIR/dotnet-sdk-8.0.426.tar.gz"
  curl --fail --show-error --location --retry 2 https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.426/dotnet-sdk-8.0.426-linux-x64.tar.gz --output "$archive"
  printf '%s  %s\n' '8141d01bdc53f604aed3bb661b604707a64209f8a947cd81bf95b4f518951593d77864069191f54ea5e84362a409da6b0f69074bf893b283d7e0cf93e2bf20a8' "$archive" | sha512sum --check --status
  tar -xzf "$archive" -C "$DOTNET_ROOT"
  rm "$archive"
fi
dotnet restore backend/PIM3SemBackend.sln --locked-mode
dotnet tool restore
npm ci --prefix tests/browser
docker pull mcr.microsoft.com/mssql/server:2022-CU18-ubuntu-22.04@sha256:ea73825f3d88a23c355ac2f9fdc6bd960fec90171c12c572109b36a558f77bb8
dotnet build backend/PIM3SemBackend.sln --no-restore
