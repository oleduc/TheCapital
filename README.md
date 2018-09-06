# The Capital (Rimworld Mod)

The capital is a Rimworld mod that aims to expand on the game's politics and power dynamics. It is meant to give the player a challenge that must be managed from the early-mid game all the way into the very late game. The capital is a very large industrial city that occupies several tiles and its economy, population and military power are all based on its infrastructure rather than it being proportional to the player's wealth like other factions. Its dominance over the planet and its very existence is a balancing act as its resources and influence are limited and as such, it is up to the player to make that balance tip one way or another.

<p align="center">
  <img src="https://raw.githubusercontent.com/oleduc/TheCapital/master/Assets/Screenshots/worldgen.png" />
</p>

## Features
- [X] Adds a special powerful faction in the map
- [X] Adds a massive city to the world map
- [X] Adds roads and tiles of to the world map

## In development
- [ ] Adds air vehicles that the capital uses to dominate the planet
- [ ] Local maps for each tile type that the player can attack

## Planned
- [ ] A tracker that takes in account what ressources the capital has to derive its power
- [ ] Adds special power weapons
- [ ] Systematically subjugate nearby settlements
- [ ] Interact with the player's settlement (Inspections, atrocities, occupation, artillery attacks, diplomacy)
- [ ] Reacts to world event (May start expanding, may start attacking settlements, may start trying to wipe out a faction)
- [ ] A custom interaction dialog for advanced faction interactions


## Contributing

All contributions are welcomed and encouraged. Make sure to open an issue or comment on an existing issue to let us know that you intend to work on something :)

## Source setup

If you are using Jetbrains rider as an IDE, then all you to do is procure the following dependencies:

- *UnityEngine.dll* from your game folder (.\RimWorld\RimWorldWin_Data\Managed)
- *Assembly-CSharp.dll* from your game folder (.\RimWorld\RimWorldWin_Data\Managed)
- *0Harmony.dll* by building the source in [this repository](https://github.com/pardeike/Harmony).

We are using bash scripts to copy the mod into the Rimworld folder create a copy of it and make sure to update the RW_PATH constant. Don't forget to add it to your .gitignore file. If you do not have a bash shell you can use the one provided by [Git for windows](https://git-scm.com/download/win).

## Resources

- https://spdskatr.github.io/RWModdingResources/
- https://github.com/roxxploxx/RimWorldModGuide/wiki
