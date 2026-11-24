# FlapChat — Gameplay Programming Showcase
# By Cristian De Santis

Gameplay code from FlapChat, a 2D Unity game developed by a team of eight in 2024.
FlapChat is a 2D Platformer Shoot'em'up with Dating sim mechanics.
I worked as Lead Programmer and Technical Game Designer.

In this folder you will be able to see some scripts from the game.

## FlapChat_PlayerController.cs

This script holds all the variables for the player controller and the main logics behind player's movement.
It uses States to determine which action can and cannot be executed.
Among the main mechanics there is a sliding-centred one. During a slide, a surface query provides the terrain normal. The controller aligns the character to the surface and uses velocity along the slope to determine which way the sprite faces.

## FlapChat_EnemyMovement.cs

This script handles the first type of enemies, holding its variables and accessing the required data in order to follow the player.
This enemy's main mechanic is to move towards the player and deal damage if close enough.

## FlapChat_ShotgunCode.cs

This script holds all the code and logic for the Shotgun, the player's main weapon.
It handles recoil, damage output, and unique mechanics like charged shot or "ShootDown" which allows the player to consume both the ammos to boost themselves upwards by shooting below them.

## FlapChat_BeekDMG.cs

This script handles the logic for the beak's damage output.
The player can deal damage with its beak when sliding (by holding the 'S' or the 'Left Shift' keys) and the damage is based on player's momentum. The script checks the player's current movement speed and keeps the output damage balanced not to feel too powerful and make the player abuse the mechanic.

## FlapChat_DamageToEnemies.cs

This script handles the logic behind enemies getting damaged. It uses an Enum which makes it easier to handle various cases and, based on maximum health, handles how much damage the player deals to the enemies. Additionally, it also handles "game juice" features, such as sound and visual effects.

## MusicChanger

The game features an original song for each character in the game which also has an intensity version that plays during the last round (wave 3). The music changes over time as soon as the last stage begins.

## Running the game

This repository contains selected scripts from the original Unity project.
The scenes, prefabs, and assets needed to run the game are outside this repository.

In this folder you can find 3 gifs attached which show some of the gameplay features I've mentioned.

A previous version of the game can be played at my itch.io page

https://cristian-de-santis.itch.io/