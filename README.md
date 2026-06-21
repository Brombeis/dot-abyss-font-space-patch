# dot-abyss-font-space-patch

A BepInEx plugin for ドットアビスX (Dot Abyss X) that forces `_fontSpace` to `-6.5` on all `NovelText` instances.

## Setup (required before building)

The project references BepInEx and game DLLs via a `Directory.Build.props` file that is **not** committed to source control. You must create it yourself before building:

1. Copy `Directory.Build.props.example` to `Directory.Build.props`
2. Edit `Directory.Build.props` and set `GameDir` to your game's install folder:

```xml
<Project>
  <PropertyGroup>
    <GameDir>C:\Path\To\Your\dotabyss_x_cl</GameDir>
  </PropertyGroup>
</Project>
```

`Directory.Build.props` is gitignored — never commit it.

## What it does

Patches `Project.Novel.NovelText` at two points:
- **`Initialize` postfix** — sets font spacing immediately after a `NovelText` is constructed
- **`SetParam` postfix** — overrides whatever `fontSpace` value the game passes in, so it can't be changed back

## Requirements

- BepInEx 6.0.0 IL2CPP (already installed in the game directory)
- .NET 10 SDK (for building)

## Building

```
dotnet build -c Release
copy bin\Release\net6.0\FontSpacePatch.dll <GameDir>\BepInEx\plugins\
```

## Project structure

```
Directory.Build.props.example   — template for the required local config file
Directory.Build.props           — your local game path (gitignored, create from example)
FontSpacePatch.csproj           — project file, references DLLs via $(GameDir)
Plugin.cs                       — plugin entry point + Harmony patch classes
MyPluginInfo.cs                 — GUID, name, version constants
```
