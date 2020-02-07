# Genetic Engine

[![Build Status](https://travis-ci.com/nightblade9/genetic-engine.svg?branch=master)](https://travis-ci.com/nightblade9/genetic-engine)

![screenshot](https://i.imgur.com/YekspM7.png)

A simple genetic engine, for applying genetic programming or genetic algorithms to your C# projects.

You can see the `GeneticRoguelike` project as a sample. It uses genetic programming to derive an algorithm to generate a roguelike map, like the sample above. The process:

- Define fitness as the average distance between ten randomly-selected points (not directly next to each other)
- Define our solution as a list of "primitive" dungeon operations (create a room, walk ten times randomly and clear those tiles, etc.)
- Add a `SadConsole` visualization - every generation, the best solution is drawn on-screen in ASCII characters
