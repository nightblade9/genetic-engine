using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace GeneticEngine.UnitTests
{
    [TestFixture]
    public class EngineTests
    {
        [Test]
        public void TournamentSelectionThrowsIfCandidateListIsNull()
        {
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            Assert.Throws<ArgumentException>(() => engine.TournamentSelection(null));
        }

        [Test]
        public void TournamentSelectionThrowsIfThereAreNoCandidates()
        {
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            Assert.Throws<ArgumentException>(() => engine.TournamentSelection(new List<CandidateSolution<int>>()));
        }

        [Test]
        public void TournamentSelectionReturnsOneOfThreeRandomCandidates()
        {
            // Arrange
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            var solutions = new List<CandidateSolution<int>>() {
                new CandidateSolution<int>() { Solution = 1, Fitness = 1 },
                new CandidateSolution<int>() { Solution = 4, Fitness = 4 },
                new CandidateSolution<int>() { Solution = 9, Fitness = 9 },
                new CandidateSolution<int>() { Solution = 16, Fitness = 16 },
                new CandidateSolution<int>() { Solution = 25, Fitness = 25 },
            };
            var bestFitness = solutions.Max(s => s.Fitness);
            
            // Act: do this ten times, at least one shouldn't be the best
            var actual = new List<CandidateSolution<int>>();
            while (actual.Count < 10)
            {
                actual.Add(engine.TournamentSelection(solutions));
            }

            // Assert
            Assert.That(actual.Any(s => s.Fitness != bestFitness)); // not always best
            Assert.That(actual.Distinct().Count(), Is.GreaterThan(1)); // not always the same
            Assert.That(actual.All(s => solutions.Contains(s))); // always from our pool
        }

        [Test]
        public void SolveThrowsIfPopulationIsNotSet()
        {
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            var ex = Assert.Throws<InvalidOperationException>(() => engine.Solve());
            Assert.That(ex.Message.Contains("CreateInitialPopulation"));
        }

        [Test]
        public void SolveThrowsIfFitnessMethodIsNotSet()
        {
            // Arrange
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            engine.CreateInitialPopulation(() => 7);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => engine.Solve());

            // Assert
            Assert.That(ex.Message.Contains("SetFitnessMethod"));
        }

        [Test]
        public void SolveThrowsIfCrossOverMethodIsNotSet()
        {
            // Arrange
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            engine.CreateInitialPopulation(() => 7);
            engine.SetFitnessMethod((a) => a + 1);
            //engine.SetCrossOverMethod((a, b) => new List<int>() { a + 1, b - 1});

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => engine.Solve());

            // Assert
            Assert.That(ex.Message.Contains("SetCrossOverMethod"));
        }

        [Test]
        public void SolveThrowsIfSelectionMethodIsNotSet()
        {
            // Arrange
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            engine.CreateInitialPopulation(() => 7);
            engine.SetFitnessMethod((a) => a + 1);
            engine.SetCrossOverMethod((a, b) => new List<int>() { a + 1, b - 1});

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => engine.Solve());

            // Assert
            Assert.That(ex.Message.Contains("SetSelectionMethod"));
        }

        [Test]
        public void SolveThrowsIfMutationMethodIsNotSet()
        {
            // Arrange
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            engine.CreateInitialPopulation(() => 7);
            engine.SetFitnessMethod((a) => a);
            engine.SetCrossOverMethod((a, b) => new List<int>() { a + 1, b - 1});
            engine.SetSelectionMethod(engine.TournamentSelection);

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() => engine.Solve());

            // Assert
            Assert.That(ex.Message.Contains("SetMutationMethod"));
        }

        [Test]
        public void SolveSolvesInTenGenerations()
        {
            int numGenerations = 0;
            float bestFitness = 0;

            // Arrange
            var engine = new Engine<int, int>(1000, 0.1f, 0.5f, 0.1f);
            engine.CreateInitialPopulation(() => 7);
            // Guaranteed to solve quickly because of a static population of 7s and a static fitness
            engine.SetFitnessMethod((a) => 99);
            engine.SetCrossOverMethod((a, b) => new List<int>() { a + 1, b - 1});
            engine.SetSelectionMethod(engine.TournamentSelection);
            engine.SetMutationMethod((a) => a);

            engine.SetOnGenerationCallback((generation, best) => {
                bestFitness = best.Fitness;
                numGenerations = generation;
            });

            // Act
            engine.Solve();

            // Assert: didn't throw, and gives us the expected fitness
            Assert.That(bestFitness, Is.EqualTo(99));
            Assert.That(numGenerations, Is.EqualTo(10));
        }
    }
}
