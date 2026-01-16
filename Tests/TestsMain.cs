using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using E33Randomizer;
using FluentAssertions;
using NUnit.Framework;
using Tests.Rules;
using Tests.RuleTests;

namespace Tests
{
    [TestFixture]
    public class RandomizerLogicTests
    {
        private Fixture _fixture;
        private List<OutputRuleBase> _rules;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _fixture.Customize<int>(c => c.FromFactory(() => new Random().Next(1, 100)));

            _rules = new List<OutputRuleBase>
            {
                new ChangeEncounterSize(),
                new NoSimonP2BeforeLune(),
                new EnsureBossesInBossEncounters(),
                new ReduceBossRepetition(),
                new ChangeCheckQuantities(),
                new ChangeCheckSize(),
                new ChangeEncounterSize(),
                new ChangeMerchantInventoryLocked(),
                new ChangeSizesOfNonRandomizedChecks(),
                new ChangeSizeOfNonRandomizedEncounters(),
                new EnsureBossesInBossEncounters(),
                new EnsurePaintedPowerFromPaintress(),
                new IncludeCutContentEnemies(),
                new IncludeCutContentItems(),
                new IncludeCutContentSkills(),
                new IncludeGearInPrologue(),
                new NoSimonP2BeforeLune(),
                new RandomizeAddedEnemies(),
                new RandomizeGestralBeachRewards(),
                new RandomizeGustaveStartingWeapon(),
                new RandomizeMerchantFights(),
                new RandomizeSkillUnlockCosts(),
                new RandomizeTreeEdges(),
                new ReduceBossRepetition(),
                new ReduceGearRepetition(),
                new ReduceKeyItemRepetition(),
                new ReduceSkillRepetition(),
                new UnlockGustaveSkills(),
            };

            RandomizerLogic.Init();
            TestLogic.OriginalData = TestLogic.CollectState();
        }

        [Test]
        public void TestRandomCases()
        {
            var failureDetails = new List<string>();
            const int iterations = 1;

            for (int i = 0; i < iterations; i++)
            {
                var settings = _fixture.Create<SettingsViewModel>();
                settings.Seed = TestLogic.Random.Next();
                settings.RandomizeEnemies = true;
                settings.RandomizeItems = true;
                settings.RandomizeSkills = true;
                
                var config = new Config(
                    settings,
                    new CustomEnemyPlacement(),
                    new CustomItemPlacement(),
                    new CustomSkillPlacement()
                    );
                
                
                var output = TestLogic.RunRandomizer(config);

                foreach (var rule in _rules)
                {
                    if (!rule.IsSatisfied(output, config))
                    {
                        failureDetails.Add(
                            $"[Run {i}] {rule.GetType().Name}: {rule.FailureMessage}"
                        );
                    }
                }
            }

            failureDetails.Should().BeEmpty(
                $"Found {failureDetails.Count} rule violations in {iterations} runs:\n" +
                string.Join("\n", failureDetails.Take(20)) +
                (failureDetails.Count > 20 ? $"\n... and {failureDetails.Count - 20} more failures" : "")
            );
        }

        [Test]
        public void TestReduceBossRepetition()
        {
            var failureDetails = new List<string>();
            
            var settings = new SettingsViewModel
            {
                Seed = new Random().Next(),
                ReduceBossRepetition = true,
                EnsureBossesInBossEncounters = true
            };
            
            var config = new Config(
                settings,
                new CustomEnemyPlacement(),
                new CustomItemPlacement(),
                new CustomSkillPlacement()
            );
            
            var output = TestLogic.RunRandomizer(config);

            foreach (var rule in _rules)
            {
                if (!rule.IsSatisfied(output, config))
                {
                    failureDetails.Add(rule.FailureMessage);
                }
            }

            failureDetails.Should().BeEmpty(
                $"Found {failureDetails.Count} rule violations:\n" +
                string.Join("\n", failureDetails.Take(20)) +
                (failureDetails.Count > 20 ? $"\n... and {failureDetails.Count - 20} more failures" : "")
            );
        }
    }

    #region Rule Infrastructure

    public interface IOutputRule
    {
        bool IsSatisfied(Output output, Config config);
    }

    public abstract class OutputRuleBase : IOutputRule
    {
        public abstract bool IsSatisfied(Output output, Config config);
        
        public string FailureMessage;

        protected OutputRuleBase()
        {
            FailureMessage = $"{GetType().Name} rule was not satisfied:\n\t";
        }
    }

    #endregion
}