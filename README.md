# Windows Search Manager

[![PSGallery](https://img.shields.io/powershellgallery/v/WindowsSearchManager.svg?logo=powershell&label=PSGallery&color=orange&logoColor=white)](https://www.powershellgallery.com/packages/WindowsSearchManager/)
[![CI/CD](https://github.com/mawosoft/WindowsSearchManager/actions/workflows/ci.yml/badge.svg)](https://github.com/mawosoft/WindowsSearchManager/actions/workflows/ci.yml)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Powershell module for managing Windows Search. With WindowsSearchManager you can:
- Manage global Windows Search settings across catalogs
- Manage individual search catalogs (content indexes)
- Manage search roots (content stores)
- Manage search rules (what is and what isn't indexed)

To learn more, see the [Documentation](https://mawosoft.github.io/WindowsSearchManager/).

## Installation

You can install WindowsSearchManager from the PowerShell Gallery.

```powershell
Install-Module -Name WindowsSearchManager
```

### CI Feed

To install the latest build from the [CI feed](https://dev.azure.com/mawosoft-de/public/_packaging?_a=feed&feed=public):

```powershell
  Register-PSRepository -Name mawosoft-nightly -SourceLocation https://pkgs.dev.azure.com/mawosoft-de/public/_packaging/public/nuget/v2/
  Install-Module -Name WindowsSearchManager -Repository mawosoft-nightly -AllowPrerelease -Force
```
