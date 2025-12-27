# TerminalFoundry

TerminalFoundry is a PHP-powered CLI UI toolkit with a Windows launcher that renders JSON-described UIs into a native desktop window. Build pages from modular elements (sections, cards, tables, inputs, buttons) and support interactive navigation with routes and actions.

## Features
- Declarative UI elements (pages, sections, cards, tables, inputs, buttons).
- JSON UI payloads for a native Windows renderer.
- Action routing for interactive navigation.
- Portable distribution with bundled PHP runtime.

## Quick Start
1) Install dependencies:
```bash
composer install
```

2) Run from PHP:
```bash
php index.php main home
```

3) Build the Windows launcher:
```bash
./build.ps1
```

## Structure
- `App/` - Application pages and runtime configuration.
- `Framework/` - UI components and renderer.
- `Framework/Cli/Launcher/` - Windows launcher (C#).
- `dist/` - Build output (generated).

## Debugging
Set `LAUNCHER_DEBUG=1` in `.env` to enable launcher debug UI/logging, then rebuild.

## License
MIT
