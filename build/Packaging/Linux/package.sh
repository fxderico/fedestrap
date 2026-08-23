set -euo pipefail

ROOT="$(cd "$(dirname "$0")/../../.." && pwd)"
RID="${1:?A Linux runtime identifier is required}"
VERSION="${2:-}"
FORMAT="${3:?A package format is required}"
OUTPUT="${4:?An output directory is required}"

if [ -z "$VERSION" ]; then
  VERSION="$(sed -n 's:^[[:space:]]*<FedestrapVersion>\(.*\)</FedestrapVersion>[[:space:]]*$:\1:p' "$ROOT/Directory.Build.props" | head -n 1)"
fi

case "$RID" in
  linux-x64) DEB_ARCH="amd64"; RPM_ARCH="x86_64"; APPIMAGE_ARCH="x86_64" ;;
  linux-arm64) DEB_ARCH="arm64"; RPM_ARCH="aarch64"; APPIMAGE_ARCH="aarch64" ;;
  linux-musl-x64) DEB_ARCH="amd64"; RPM_ARCH="x86_64"; APPIMAGE_ARCH="x86_64" ;;
  linux-musl-arm64) DEB_ARCH="arm64"; RPM_ARCH="aarch64"; APPIMAGE_ARCH="aarch64" ;;
  *) echo "Unsupported Linux runtime identifier"; exit 1 ;;
esac

case "$FORMAT" in
  deb|rpm|appimage|tar) ;;
  *) echo "Unsupported package format"; exit 1 ;;
esac

case "$RID:$FORMAT" in
  linux-musl-x64:deb|linux-musl-x64:rpm|linux-musl-x64:appimage|linux-musl-arm64:deb|linux-musl-arm64:rpm|linux-musl-arm64:appimage)
    echo "This package format requires a glibc runtime identifier"
    exit 1
    ;;
esac

case "$FORMAT" in
  deb) command -v dpkg-deb >/dev/null 2>&1 || { echo "The Debian package tool is unavailable"; exit 1; } ;;
  rpm) command -v rpmbuild >/dev/null 2>&1 || { echo "The RPM package tool is unavailable"; exit 1; } ;;
  appimage) command -v appimagetool >/dev/null 2>&1 || { echo "The AppImage package tool is unavailable"; exit 1; } ;;
  tar) command -v tar >/dev/null 2>&1 || { echo "The tar package tool is unavailable"; exit 1; } ;;
esac

if [[ ! "$VERSION" =~ ^[0-9]+([.][0-9]+){1,3}$ ]]; then
  echo "The package version is invalid"
  exit 1
fi

mkdir -p "$OUTPUT"
OUTPUT="$(cd "$OUTPUT" && pwd)"
case "$FORMAT" in
  deb) FINAL_TARGET="$OUTPUT/Fedestrap_${VERSION}_${DEB_ARCH}.deb" ;;
  rpm) FINAL_TARGET="$OUTPUT/Fedestrap_${VERSION}_${RPM_ARCH}.rpm" ;;
  appimage) FINAL_TARGET="$OUTPUT/Fedestrap_${VERSION}_${APPIMAGE_ARCH}.AppImage" ;;
  tar) FINAL_TARGET="$OUTPUT/Fedestrap_${VERSION}_${RID}.tar.gz" ;;
esac
if [ -e "$FINAL_TARGET" ]; then
  echo "The requested output already exists"
  exit 1
fi
LOCK="$OUTPUT/.fedestrap-linux-$RID.lock"
if ! mkdir "$LOCK" 2>/dev/null; then
  echo "Another package operation is already running"
  exit 1
fi
STAGE=""

cleanup() {
	if [ -n "$STAGE" ] && [ -e "$STAGE" ]; then
		rm -rf "$STAGE"
	fi
	rmdir "$LOCK" 2>/dev/null || true
}

trap cleanup EXIT
STAGE="$(mktemp -d "$OUTPUT/.fedestrap-linux.XXXXXX")"
PUBLISH="$STAGE/publish"
mkdir -p "$PUBLISH"
dotnet publish "$ROOT/src/Fedestrap.Cross/Fedestrap.Cross.csproj" -c Release -r "$RID" --self-contained true -o "$PUBLISH" -p:BaseIntermediateOutputPath="obj-$RID/" -p:Version="$VERSION" -p:DebugType=none -p:DebugSymbols=false
test -s "$PUBLISH/Fedestrap"
if [ "$(find "$PUBLISH" -mindepth 1 | wc -l)" -ne 1 ]; then
  echo "The Linux publish is not a single file"
  find "$PUBLISH" -mindepth 1
  exit 1
fi
chmod 755 "$PUBLISH/Fedestrap"

case "$FORMAT" in
  deb)
    TARGET="$STAGE/Fedestrap_${VERSION}_${DEB_ARCH}.deb"
    ROOTFS="$STAGE/deb"
    mkdir -p "$ROOTFS/DEBIAN" "$ROOTFS/usr/lib/fedestrap" "$ROOTFS/usr/bin" "$ROOTFS/usr/share/applications" "$ROOTFS/usr/share/icons/hicolor/256x256/apps"
    cp -R "$PUBLISH/." "$ROOTFS/usr/lib/fedestrap/"
    ln -s /usr/lib/fedestrap/Fedestrap "$ROOTFS/usr/bin/fedestrap"
    cp "$ROOT/build/Packaging/Linux/fedestrap.desktop" "$ROOTFS/usr/share/applications/fedestrap.desktop"
    cp "$ROOT/src/Fedestrap.App/Fedestrap.png" "$ROOTFS/usr/share/icons/hicolor/256x256/apps/fedestrap.png"
    printf 'Package: fedestrap\nVersion: %s\nSection: games\nPriority: optional\nArchitecture: %s\nMaintainer: Fedestrap\nDepends: libc6, libgcc-s1 | libgcc1, libstdc++6, libx11-6, libice6, libsm6, libfontconfig1, libfreetype6, libgl1 | libgl1-mesa-glx, libegl1 | libegl1-mesa, zlib1g, ca-certificates\nRecommends: flatpak, xdg-utils, libnotify-bin, libsecret-tools, libgtk-3-0, libvulkan1, mesa-vulkan-drivers, libwayland-client0, libwebkit2gtk-4.1-0 | libwpewebkit-2.0-1\nDescription: Fedestrap Roblox desktop launcher\n' "$VERSION" "$DEB_ARCH" > "$ROOTFS/DEBIAN/control"
    dpkg-deb --root-owner-group --build "$ROOTFS" "$TARGET"
    ;;
  rpm)
    TARGET="$STAGE/Fedestrap_${VERSION}_${RPM_ARCH}.rpm"
    RPMROOT="$STAGE/rpmbuild"
    mkdir -p "$RPMROOT/BUILD" "$RPMROOT/BUILDROOT" "$RPMROOT/RPMS" "$RPMROOT/SOURCES" "$RPMROOT/SPECS" "$RPMROOT/SRPMS"
    cp -R "$PUBLISH/." "$RPMROOT/SOURCES/publish"
    cp "$ROOT/build/Packaging/Linux/fedestrap.desktop" "$RPMROOT/SOURCES/fedestrap.desktop"
    cp "$ROOT/src/Fedestrap.App/Fedestrap.png" "$RPMROOT/SOURCES/fedestrap.png"
    printf 'Name: fedestrap\nVersion: %s\nRelease: 1\nSummary: Fedestrap Roblox desktop launcher\nLicense: Custom\nBuildArch: %s\nRequires: libX11\nRequires: libICE\nRequires: libSM\nRequires: fontconfig\nRequires: freetype\nRequires: mesa-libGL\nRequires: mesa-libEGL\nRequires: ca-certificates\nRecommends: flatpak\nRecommends: xdg-utils\nRecommends: libnotify\nRecommends: libsecret\nRecommends: vulkan-loader\nRecommends: mesa-vulkan-drivers\nRecommends: webkit2gtk4.1\nSource0: publish\nSource1: fedestrap.desktop\nSource2: fedestrap.png\n%%description\nFedestrap Roblox desktop launcher\n%%install\nmkdir -p %%{buildroot}%%{_libdir}/fedestrap %%{buildroot}%%{_bindir} %%{buildroot}%%{_datadir}/applications %%{buildroot}%%{_datadir}/icons/hicolor/256x256/apps\ncp -a %%{_sourcedir}/publish/. %%{buildroot}%%{_libdir}/fedestrap/\nln -s %%{_libdir}/fedestrap/Fedestrap %%{buildroot}%%{_bindir}/fedestrap\ninstall -m 644 %%{_sourcedir}/fedestrap.desktop %%{buildroot}%%{_datadir}/applications/fedestrap.desktop\ninstall -m 644 %%{_sourcedir}/fedestrap.png %%{buildroot}%%{_datadir}/icons/hicolor/256x256/apps/fedestrap.png\n%%files\n%%{_libdir}/fedestrap\n%%{_bindir}/fedestrap\n%%{_datadir}/applications/fedestrap.desktop\n%%{_datadir}/icons/hicolor/256x256/apps/fedestrap.png\n' "$VERSION" "$RPM_ARCH" > "$RPMROOT/SPECS/fedestrap.spec"
    rpmbuild --define "_topdir $RPMROOT" --target "$RPM_ARCH" -bb "$RPMROOT/SPECS/fedestrap.spec"
    cp "$RPMROOT/RPMS/$RPM_ARCH/fedestrap-$VERSION-1.$RPM_ARCH.rpm" "$TARGET"
    ;;
  appimage)
    TARGET="$STAGE/Fedestrap_${VERSION}_${APPIMAGE_ARCH}.AppImage"
    APPDIR="$STAGE/AppDir"
    mkdir -p "$APPDIR/usr/lib/fedestrap" "$APPDIR/usr/bin"
    cp -R "$PUBLISH/." "$APPDIR/usr/lib/fedestrap/"
    ln -s ../lib/fedestrap/Fedestrap "$APPDIR/usr/bin/fedestrap"
    ln -s usr/bin/fedestrap "$APPDIR/AppRun"
    cp "$ROOT/build/Packaging/Linux/fedestrap.desktop" "$APPDIR/fedestrap.desktop"
    cp "$ROOT/src/Fedestrap.App/Fedestrap.png" "$APPDIR/fedestrap.png"
    ARCH="$APPIMAGE_ARCH" appimagetool "$APPDIR" "$TARGET"
    ;;
  tar)
    TARGET="$STAGE/Fedestrap_${VERSION}_${RID}.tar.gz"
    BUNDLE="$STAGE/Fedestrap"
    mkdir -p "$BUNDLE"
    cp "$PUBLISH/Fedestrap" "$BUNDLE/Fedestrap"
    chmod 755 "$BUNDLE/Fedestrap"
    tar --sort=name --mtime="@${SOURCE_DATE_EPOCH:-0}" --owner=0 --group=0 --numeric-owner -C "$STAGE" -czf "$TARGET" Fedestrap
    ;;
esac

mv -n "$TARGET" "$FINAL_TARGET"
if [ -e "$TARGET" ]; then
  echo "The requested output already exists"
  exit 1
fi
