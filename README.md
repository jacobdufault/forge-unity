## Forge Unity

Forge Unity provides the core [Forge](https://github.com/jacobdufault/forge) bindings to the Unity game engine (version 4.3). There is a sample that demos Forge [here](https://github.com/jacobdufault/forge-sample).

This project is open source under the MIT license.

## Installation

Using Forge in Unity is simple.

1. Create a new Unity 4.3 project or open an existing one
2. Copy `Forge` into your `Assets` directory
3. Make sure that you're using .NET 2.0 (not the subset). You can change this by following:
  1. Select `Edit` in the menu
  2. Select `Project Settings`
  3. Select `Player`
  4. Select `Other Settings` in the player menu
  5. Select `Api Compatibility Level` and set it to `.NET 2.0` (not .NET 2.0 Subset)
4. Restart Unity (to reload the menu so that `Forge` appears)
5. Select `Forge` in the menu and then `Create Level`

## Inspector / Editor Support

This package contains all of the Forge Unity integration experience, but the editing aspects of Forge Unity require the [Full Inspector](http://jacobdufault.github.io/fullinspector/). It will be available on the Unity Asset Store soon.
