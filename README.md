# Genetic Engine

[![Build Status](https://travis-ci.com/nightblade9/genetic-engine.svg?branch=master)](https://travis-ci.com/nightblade9/genetic-engine)

A simple genetic engine, for applying genetic programming or genetic algorithms to your C# projects.

You can see the `SampleSolutions` project as a sample. It uses genetic programming to derive an algorithm to generate solutions to various problems:

- The backpacking problem (optimizing value given weight and limited capacit)
- Linear regression (curve fitting)

# A Note about Fitness

Fitness must be deterministic. Since we calculate it in parallel, make sure all of your variables, references, etc. are copied. If they're not, or if there's use of random, you will end up with fitness dropping for no reason between generations, and an exception throws.

The reason is not related to elitism being broken; it's because the same solution, when evaluated in multiple threads or multiple times, results in a different fitness calculation. Check your code carefully. Use locks if nothing else works, but be prepared for slow evaluation.