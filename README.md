<p align="center">
<a href="https://github.com/fxderico/fedestrap">
<img src="src/Fedestrap.App/Fedestrap.png" alt="Fedestrap" width="100px"/>
</a>
</p>

<h1 align="center"><b>Fedestrap</b></h1>

<p align="center">
  <a href="https://github.com/fxderico/fedestrap/releases/latest">Latest release</a> |
  <a href="https://fedestrap.fede.one">Website</a> |
  <a href="https://github.com/fxderico/fedestrap/wiki">Documentation</a>
</p>

<div align="center">

[![Total Downloads][shield-repo-total]][repo-releases]
[![Latest Downloads][shield-repo-downloads]][repo-latest]
[![Latest Release][shield-repo-latest]][repo-latest]
[![Stars][shield-repo-stars]][repo-stargazers]

</div>

> [!NOTE]
> Fedestrap is a custom bootstrapper for Roblox, personally maintained for
> me, built on the [Bloxstrap][bloxstrap] → [Fishstrap][fishstrap] →
> [Voidstrap][voidstrap] lineage. It currently targets **Windows 10 and
> above**; the codebase carries cross-platform scaffolding for macOS and
> Linux inherited from Voidstrap, but those targets aren't actively
> maintained here. For Mac/Linux, try [AppleBlox][appleblox] or
> [Sober][sober].

> [!WARNING]
> The only official places to get Fedestrap are **this GitHub repository**
> and **[fedestrap.fede.one][website]**. Any other site offering downloads or
> claiming to be this project is not controlled by Fede - don't download
> from them.

If you found a bug, [open an issue][repo-new-issue].

## Installation

1. Download the latest release: https://github.com/fxderico/fedestrap/releases/latest
2. Run the installer and finish setup
3. Launch Fedestrap

## Features

Fedestrap inherits Voidstrap's much larger feature set on top of the usual
Bloxstrap-family basics (FastFlags editor with an allowlist warning, mod
manager, channel switching, cache cleaner, custom emoji fonts converted to
the COLR v0 format Roblox actually renders):

- Discord Rich Presence, RGB/theme customization, custom cursors and fonts
- Window manipulation, frame generation and render tuning options
- Google Fonts integration, translation service, gradient backgrounds
- Controller support (ViGEm), audio ducking, headset audio routing
- Game chat overlay and server matchmaking helpers
- A real, self-hosted account system - sign in, register, change your
  password, and edit your username/display name, all natively in the app
  (no browser round-trip) - see [Accounts](#accounts) below for exactly
  what that does and doesn't cover

Not every corner of the original Voidstrap platform is running here - see
[Accounts](#accounts).

## Accounts

Voidstrap's sign-in, friends, quests, marketplace, forums, notifications,
chat overlay, and theme-sharing features all depend on a live companion
backend that the original author runs. That's a full platform in its own
right (Roblox OAuth, real-time notifications, a database-backed social
graph, a marketplace, forums), not something reasonable to fork alongside
the desktop app.

Fedestrap ships with its own from-scratch, self-hosted **accounts API**
instead, at [fedestrap.fede.one][website]. What's actually real:

- Sign in, create an account, and sign out, entirely inside the app (no
  browser tab involved) - capped at 2 accounts per IP
- Change your password and edit your username/display name from Settings
- An admin panel for managing accounts (not user-facing - operator only)

What's *not* backed by anything: Roblox OAuth sign-in, friends, quests,
marketplace, forums, chat, and theme sharing. Features that depend on those
degrade gracefully when the calls fail (cached data or a plain "couldn't
load" message) rather than crashing.

## How to Fork

Fedestrap is built using **C# and .NET**.

1. Go to https://github.com/fxderico/fedestrap
2. Click **Fork** (top right)
3. This creates your own copy under your GitHub account

## Special thanks

- [pizzaboxer](https://github.com/pizzaboxer) and the [Bloxstrap][bloxstrap] project this is ultimately built on
- [returnrqt](https://github.com/return-rqt) and the [Fishstrap][fishstrap] project
- [Bratic](https://github.com/KloBraticc) and the [Voidstrap][voidstrap] project — the direct base for this build's architecture and feature set
- Other independent contributors

<table style="width: 100%; border-collapse: collapse;">
  <tr>
    <td style="width: 33%; text-align: left;">© Fedestrap</td>
    <td style="width: 33%; text-align: right;"><a href="LICENSE" target="_blank">MIT</a></td>
  </tr>
</table>

[shield-repo-downloads]:  https://img.shields.io/github/downloads/fxderico/fedestrap/latest/total?color=981bfe
[shield-repo-total]:      https://img.shields.io/github/downloads/fxderico/fedestrap/total?color=8a2be2
[shield-repo-latest]:     https://img.shields.io/github/v/release/fxderico/fedestrap?color=7a39fb
[shield-repo-stars]:      https://img.shields.io/github/stars/fxderico/fedestrap?color=ffd700

[repo-releases]:    https://github.com/fxderico/fedestrap/releases
[repo-latest]:       https://github.com/fxderico/fedestrap/releases/latest
[repo-stargazers]:  https://github.com/fxderico/fedestrap/stargazers
[repo-new-issue]:   https://github.com/fxderico/fedestrap/issues/new/choose

[website]:    https://fedestrap.fede.one
[bloxstrap]:  https://github.com/bloxstraplabs/bloxstrap
[fishstrap]:  https://github.com/fishstrap/fishstrap
[voidstrap]:  https://github.com/KloBraticc/Voidstrap
[appleblox]:  https://github.com/AppleBlox/appleblox
[sober]:      https://sober.vinegarhq.org
