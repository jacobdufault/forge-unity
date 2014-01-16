## Forge Unity

Forge Unity provides the core [Forge](https://github.com/jacobdufault/forge) bindings to the Unity game engine (version 4.3). There is a sample that demos Forge [here](https://github.com/jacobdufault/forge-sample).

This project is open source under the MIT license.

## Installation

Using Forge in Unity is simple.

### New Project
If you just want to test Forge out, this repository contains a full Unity 4.3 project.

1. Download this repository (git clone or the zip download on the sidebar in github)
2. Open an existing Unity project pointed at the downloaded package
3. Select `Forge` in the menu and then `Create Level`

### Existing Project
If you're using Forge in a preexisting project, it's a little bit more complicated, but not much.

1. Copy `Assets/forge` into your project
2. Make sure that you're using .NET 2.0 (not the subset). You can change this by following:
  1. Select `Edit` in the menu
  2. Select `Project Settings`
  3. Select `Player`
  4. Select `Other Settings` in the player menu
  5. Select `Api Compatibility Level` and set it to `.NET 2.0` (not .NET 2.0 Subset)
3. Restart Unity (to reload the menu so that `Forge` appears)
4. Select `Forge` in the menu and then `Create Level`

## Forge Editor

This package only provides the core Unity runtime bindings. To get the full Forge experience, you need the Forge Editor which will be available in the Unity Asset Store soon.
 
