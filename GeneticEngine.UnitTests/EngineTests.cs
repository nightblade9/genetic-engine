using System;
using NUnit.Framework;
using GeneticEngine;
using System.Collections.Generic;

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
            
        }
    }
}
