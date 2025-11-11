#!/usr/bin/env bash
set -euo pipefail

echo "Building IndexerLib C# Wrapper..."

# Build the wrapper library (sources are included via csproj)
pushd "$(dirname "$0")/csharp_lib" >/dev/null

unameOut="$(uname -s)"
case "${unameOut}" in
    Linux*)
        dotnet publish -c Release -r linux-x64 --self-contained
        echo
        echo "Build complete!"
        echo "Library location: csharp_lib/bin/Release/net8.0/linux-x64/publish/IndexerLibWrapper.so"
        ;;
    Darwin*)
        # Attempt Apple Silicon first, then Intel
        if dotnet publish -c Release -r osx-arm64 --self-contained; then
          echo
          echo "Build complete!"
          echo "Library location: csharp_lib/bin/Release/net8.0/osx-arm64/publish/IndexerLibWrapper.dylib"
        else
          dotnet publish -c Release -r osx-x64 --self-contained
          echo
          echo "Build complete!"
          echo "Library location: csharp_lib/bin/Release/net8.0/osx-x64/publish/IndexerLibWrapper.dylib"
        fi
        ;;
    *)
        echo "Unsupported platform: ${unameOut}"
        exit 1
        ;;
esac

popd >/dev/null
