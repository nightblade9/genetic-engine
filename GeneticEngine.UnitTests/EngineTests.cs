using System;
using NUnit.Framework;
using GeneticEngine;
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
    }
}
