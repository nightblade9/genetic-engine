# Genetic Engine

[![Build Status](https://travis-ci.com/nightblade9/genetic-engine.svg?branch=master)](https://travis-ci.com/nightblade9/genetic-engine)

A simple genetic engine, for applying genetic programming or genetic algorithms to your C# projects.

You can see the `SampleSolutions` project as a sample. It uses genetic programming to derive an algorithm to generate solutions to various problems:

- The backpacking problem (optimizing value given weight and limited capacit)
- Linear regression (curve fitting)

# TODOs

There's a bug where fitness sometimes drops, even with elitism enabled. I am not clear why. If you re-enable parallel fitness calculation in `Engine.cs`, even for deterministic problems like curve-fitting, you will find that the same solution has multiple, different scores (in the same population - eg. `x^2` with fitnesses of `-100` and `-56`).

Turning of parallel fitness calculation hides the problem most of the time, at an approximate 10x drop in performance (seconds instead of milliseconds per generation in curve-fitting).