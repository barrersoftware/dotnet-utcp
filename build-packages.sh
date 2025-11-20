#!/bin/bash
# UTCP .NET 10 - Multi-Platform Package Builder
# Builds binaries and native packages for all platforms

set -e

VERSION="1.0.0"
PROJECT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BUILD_DIR="$PROJECT_DIR/releases"
CLI_PROJECT="$PROJECT_DIR/src/UTCP.CLI/UTCP.CLI.csproj"

echo "🏴‍☠️ UTCP Package Builder - Version $VERSION"
echo ""

# Clean previous builds
rm -rf "$BUILD_DIR"
mkdir -p "$BUILD_DIR"

# Array of runtime identifiers
declare -a RUNTIMES=(
    "linux-x64"
    "linux-arm64"
    "win-x64"
    "win-arm64"
    "osx-x64"
    "osx-arm64"
)

echo "📦 Building self-contained binaries..."
for rid in "${RUNTIMES[@]}"; do
    echo "  Building $rid..."
    dotnet publish "$CLI_PROJECT" \
        -c Release \
        -r "$rid" \
        --self-contained \
        -p:PublishSingleFile=true \
        -p:PublishTrimmed=true \
        -p:EnableCompressionInSingleFile=true \
        -o "$BUILD_DIR/$rid"
done

echo ""
echo "📦 Creating distribution packages..."

# Linux .deb package (Debian/Ubuntu)
echo "  Creating .deb package..."
mkdir -p "$BUILD_DIR/deb/utcp-cli_${VERSION}/DEBIAN"
mkdir -p "$BUILD_DIR/deb/utcp-cli_${VERSION}/usr/bin"
mkdir -p "$BUILD_DIR/deb/utcp-cli_${VERSION}/usr/share/doc/utcp-cli"

cp "$BUILD_DIR/linux-x64/utcp" "$BUILD_DIR/deb/utcp-cli_${VERSION}/usr/bin/utcp"
chmod +x "$BUILD_DIR/deb/utcp-cli_${VERSION}/usr/bin/utcp"

cat > "$BUILD_DIR/deb/utcp-cli_${VERSION}/DEBIAN/control" << EOF
Package: utcp-cli
Version: ${VERSION}
Section: utils
Priority: optional
Architecture: amd64
Maintainer: Captain CP <captain-cp@barrersoftware.com>
Description: Universal Tool Calling Protocol CLI (.NET 10)
 First .NET 10 implementation of UTCP with Ollama integration.
 Features auto-model detection and multiple transport protocols.
EOF

dpkg-deb --build "$BUILD_DIR/deb/utcp-cli_${VERSION}" "$BUILD_DIR/utcp-cli_${VERSION}_amd64.deb" 2>/dev/null || echo "  Note: dpkg-deb not available, skipping .deb build"

# Linux .rpm package (RedHat/Fedora/CentOS)
echo "  Creating .rpm package spec..."
mkdir -p "$BUILD_DIR/rpm/BUILD"
mkdir -p "$BUILD_DIR/rpm/RPMS"
mkdir -p "$BUILD_DIR/rpm/SOURCES"
mkdir -p "$BUILD_DIR/rpm/SPECS"
mkdir -p "$BUILD_DIR/rpm/SRPMS"

cat > "$BUILD_DIR/rpm/SPECS/utcp-cli.spec" << 'EOF'
Name:           utcp-cli
Version:        1.0.0
Release:        1%{?dist}
Summary:        Universal Tool Calling Protocol CLI (.NET 10)

License:        Apache-2.0
URL:            https://github.com/barrersoftware/dotnet-utcp

%description
First .NET 10 implementation of UTCP with Ollama integration.
Features auto-model detection and multiple transport protocols.

%install
mkdir -p %{buildroot}/usr/bin
cp %{_sourcedir}/utcp %{buildroot}/usr/bin/utcp
chmod +x %{buildroot}/usr/bin/utcp

%files
/usr/bin/utcp

%changelog
* Wed Nov 20 2025 Captain CP <captain-cp@barrersoftware.com> - 1.0.0-1
- Initial release with Ollama integration
EOF

cp "$BUILD_DIR/linux-x64/utcp" "$BUILD_DIR/rpm/SOURCES/"
rpmbuild --define "_topdir $BUILD_DIR/rpm" -bb "$BUILD_DIR/rpm/SPECS/utcp-cli.spec" 2>/dev/null || echo "  Note: rpmbuild not available, skipping .rpm build"

# Create tarballs for manual installation
echo "  Creating distribution tarballs..."
for rid in "${RUNTIMES[@]}"; do
    cd "$BUILD_DIR/$rid"
    tar -czf "$BUILD_DIR/utcp-cli-${VERSION}-${rid}.tar.gz" *
    cd "$PROJECT_DIR"
done

echo ""
echo "✅ Build complete!"
echo ""
echo "📂 Packages created in: $BUILD_DIR"
echo ""
echo "Available formats:"
echo "  - Self-contained binaries (all platforms)"
echo "  - .deb package (Debian/Ubuntu)"
echo "  - .rpm package (RedHat/Fedora/CentOS)"
echo "  - .tar.gz archives (manual installation)"
echo ""
echo "🏴‍☠️ Captain CP - First .NET 10 UTCP Implementation"
