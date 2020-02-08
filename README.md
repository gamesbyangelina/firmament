# Firmament

A (very) simple entity-component roguelike example, made in Unity.

![](https://imgur.com/fGMqrWh.png)

About
=====

Lately I've been experimenting with *entity-component systems*, which is a way of building software using modules (or components) that do specific things, and that can be easily plugged into other objects without knowing what's going on around them. Unity itself uses an entity-component system for its game objects, letting you snap on components like rigid bodies for physics, or sprite renderers for visuals.

There's a lot of nice resources online about ECS, including some great talks by roguelike developers, like this one by Brian Bucklew of Caves of Qud: https://www.youtube.com/watch?v=U03XXzcThGU&t=917s

However, I couldn't find a lot of good examples online. There were some nice tutorials in languages I don't use, but they would often have weird twists on ECS that I wasn't expecting. There's no accepted way to do an ECS either, so talks can be conflicting or have weird small details that break a pattern you thought you'd noticed. 

The long and short of it is, I decided to make a simple one myself as a base to do stuff with, and I thought I'd share it. It's not perfect, it's not documented (I'll try but no promises) and it's not the only way to build an ECS. But it might help you! I hope so. 

Note that I've removed the art assets I used as I don't have a licence to distribute them. I believe I can distribute Sinput, DOTween and TextMeshPro though. If you want to actually run the code you'll need to fix the art stuff (the UI will also break because I didn't distribute the font I used). But mostly it's here for the code, not to run as an example game, so I hope you find it useful.

Features
========

* A simple cellular-automata dungeon, with skeletons, random grass, potions and our brave knight.
* Basic inventory stuff like picking up, dropping, equipping and throwing items.
* Armour, elemental attacks and defence, special magical equipment.
* Burning, fire that spreads across grassy tiles.
* Dynamic descriptions and log text.

Enjoy!

Mike
