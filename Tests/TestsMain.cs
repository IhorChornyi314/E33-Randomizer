using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using E33Randomizer;
using FluentAssertions;
using NUnit.Framework;
using NUnit.Framework.Internal;
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
                new ExcludeBossDuollisteP2(),
                new HardcodeSpecialEncounters(),
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
                new RemoveBrokenObjects(),
                new UnlockGustaveSkills(),
            };

            RandomizerLogic.Init();
            TestLogic.OriginalData = TestLogic.CollectState(null);
            TestLogic.OriginalData.Encounters = TestLogic.OriginalData.Encounters
                .Where(e => e.Enemies.All(i => !RemoveBrokenObjects.BrokenEnemies.Contains(i.CodeName))).ToList();
        }

        [Test]
        public void TestRandomCases()
        {
            var failureDetails = new List<string>();
            const int iterations = 50;

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
                EnsureBossesInBossEncounters = true,
                RandomizeSkills = true
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

        [Test]
        public void TestCustomPlacementExcluded()
        {
            var settings = new SettingsViewModel
            {
                Seed = new Random().Next(),
                EnableEnemyOnslaught = true,
                EnemyOnslaughtAdditionalEnemies = 99,
                EnemyOnslaughtEnemyCap = 99,
                RandomizeAddedEnemies = true
            };
            
            var config = new Config(
                settings,
                new CustomEnemyPlacement(),
                new CustomItemPlacement(),
                new CustomSkillPlacement()
            );
            
            var output = TestLogic.RunRandomizer(config);
            
            CustomPlacementTestLogic.TestExcluded(output, config).Should().BeTrue();
        }
        
        [Test]
        public void TestCustomPlacementNotRandomized()
        {
            var settings = new SettingsViewModel
            {
                Seed = new Random().Next(),
                EnableEnemyOnslaught = true,
                EnemyOnslaughtAdditionalEnemies = 99,
                EnemyOnslaughtEnemyCap = 99,
                RandomizeAddedEnemies = true
            };
            
            var config = new Config(
                settings,
                new CustomEnemyPlacement(),
                new CustomItemPlacement(),
                new CustomSkillPlacement()
            );
            
            var output = TestLogic.RunRandomizer(config);
            
            CustomPlacementTestLogic.TestNotRandomized(output, config).Should().BeTrue();
        }

        [Test]
        public void TestGetRandomWeighted()
        {
            var weights = new Dictionary<string, float>
            {
                ["a"] = 0.05f,
                ["b"] = 0.25f,
                ["c"] = 0.35f,
                ["d"] = 0.35f
            };

            List<string> banned = ["b"];
            
            var pickedCount = new Dictionary<string, int>
            {
                ["a"] = 0,
                ["b"] = 0,
                ["c"] = 0,
                ["d"] = 0
            };

            var gensCount = 100000;
            
            for (int i = 0; i < gensCount; i++)
            {
                var pick = Utils.GetRandomWeighted(weights, banned);
                pickedCount.TryAdd(pick, 0);
                pickedCount[pick]++;
            }
            
            var frequencies = pickedCount.Select(kvp => (float)kvp.Value / gensCount).ToList();

            frequencies[0].Should().BeInRange(0.064f, 0.069f);
            frequencies[1].Should().Be(0);
            frequencies[2].Should().BeInRange(0.464f, 0.469f);
            frequencies[3].Should().BeInRange(0.464f, 0.469f);
        }
        
        [Test]
        public void TestCustomPlacementFrequencyAdjustment()
        {
            var settings = new SettingsViewModel
            {
                Seed = TestLogic.Random.Next(),
                EnableEnemyOnslaught = true,
                EnemyOnslaughtAdditionalEnemies = 99,
                EnemyOnslaughtEnemyCap = 99,
                RandomizeAddedEnemies = true
            };

            var config = new Config(
                settings,
                new CustomEnemyPlacement(),
                new CustomItemPlacement(),
                new CustomSkillPlacement()
            );
            
            var adjustedOutput = TestLogic.RunRandomizer(config);
            
            config.Settings.Seed = TestLogic.Random.Next();
            config.CustomEnemyPlacement.FrequencyAdjustments.Clear();
            config.CustomEnemyPlacement.Update();
            
            var unadjustedOutput = TestLogic.RunRandomizer(config);

            config.CustomEnemyPlacement = new CustomEnemyPlacement();
            
            CustomPlacementTestLogic.TestFrequencyAdjustment(unadjustedOutput, adjustedOutput, config).Should().BeTrue();
        }
        
        [Test]
        public void TestDefaultCustomPlacement()
        {
            var settings = new SettingsViewModel
            {
                Seed = new Random().Next(),
            };
            
            var config = new Config(
                settings,
                new CustomEnemyPlacement(),
                new CustomItemPlacement(),
                new CustomSkillPlacement()
            );
            
            var output = TestLogic.RunRandomizer(config);
            
            CustomPlacementTestLogic.TestDefaultCustomPlacement(output, config).Should().BeTrue();
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