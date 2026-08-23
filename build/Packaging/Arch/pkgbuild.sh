set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
VERSION="${1:-}"
OUTPUT="${2:?An output directory is required}"
PROJECT_URL="https://github.com/fxderico/fedestrap"

if [ -z "$VERSION" ]; then
  VERSION="$(sed -n 's:^[[:space:]]*<FedestrapVersion>\(.*\)</FedestrapVersion>[[:space:]]*$:\1:p' "$ROOT/Directory.Build.props" | head -n 1)"
fi

if [[ ! "$VERSION" =~ ^[0-9]+([.][0-9]+){1,3}$ ]]; then
  echo "The package version is invalid"
  exit 1
fi

mkdir -p "$OUTPUT"
OUTPUT="$(cd "$OUTPUT" && pwd)"
TARGET="$OUTPUT/PKGBUILD"

cat > "$TARGET" <<PKGBUILD
pkgname=fedestrap-bin
pkgver=$VERSION
pkgrel=1
pkgdesc='Fedestrap Roblox desktop launcher'
arch=('x86_64' 'aarch64')
url='$PROJECT_URL'
license=('LicenseRef-Fedestrap')
depends=('glibc' 'gcc-libs' 'zlib' 'libx11' 'libice' 'libsm' 'fontconfig' 'freetype2' 'libglvnd' 'openssl' 'ca-certificates' 'hicolor-icon-theme' 'desktop-file-utils')
optdepends=('flatpak: required to install and run the Sober Roblox runtime'
            'xdg-utils: protocol handler registration'
            'libnotify: desktop notifications'
            'libsecret: credential storage'
            'vulkan-icd-loader: Vulkan rendering backend'
            'wayland: Wayland rendering backend'
            'webkit2gtk-4.1: embedded web views')
provides=('fedestrap')
conflicts=('fedestrap')
options=('!strip' '!debug')
source=("fedestrap-\$pkgver.desktop::\$url/raw/v\$pkgver/build/Packaging/Linux/fedestrap.desktop"
        "fedestrap-\$pkgver.png::\$url/raw/v\$pkgver/src/Fedestrap.App/Fedestrap.png"
        "fedestrap-\$pkgver.license::\$url/raw/v\$pkgver/LICENSE.FEDESTRAP")
source_x86_64=("\$url/releases/download/v\$pkgver/Fedestrap_\${pkgver}_linux-x64.tar.gz")
source_aarch64=("\$url/releases/download/v\$pkgver/Fedestrap_\${pkgver}_linux-arm64.tar.gz")
sha256sums=('SKIP' 'SKIP' 'SKIP')
sha256sums_x86_64=('SKIP')
sha256sums_aarch64=('SKIP')

package() {
    install -Dm755 "\$srcdir/Fedestrap/Fedestrap" "\$pkgdir/usr/lib/fedestrap/Fedestrap"
    install -dm755 "\$pkgdir/usr/bin"
    ln -s /usr/lib/fedestrap/Fedestrap "\$pkgdir/usr/bin/fedestrap"
    install -Dm644 "\$srcdir/fedestrap-\$pkgver.desktop" "\$pkgdir/usr/share/applications/fedestrap.desktop"
    install -Dm644 "\$srcdir/fedestrap-\$pkgver.png" "\$pkgdir/usr/share/icons/hicolor/256x256/apps/fedestrap.png"
    install -Dm644 "\$srcdir/fedestrap-\$pkgver.license" "\$pkgdir/usr/share/licenses/\$pkgname/LICENSE"
}
PKGBUILD

echo "Wrote $TARGET"
