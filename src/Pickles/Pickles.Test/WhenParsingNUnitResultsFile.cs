﻿using System;

using NUnit.Framework;

using PicklesDoc.Pickles.ObjectModel;
using PicklesDoc.Pickles.TestFrameworks;
using Should;

namespace PicklesDoc.Pickles.Test
{
    [TestFixture]
    public class WhenParsingNUnitResultsFile : WhenParsingTestResultFiles<NUnitResults>
    {
        public WhenParsingNUnitResultsFile()
            : base("results-example-nunit.xml")
        {
        }

        [Test]
        public void ThenCanReadFeatureResultSuccessfully()
        {
            // Write out the embedded test results file
            var results = ParseResultsFile();

            var feature = new Feature { Name = "Addition" };
            TestResult result = results.GetFeatureResult(feature);

            result.WasExecuted.ShouldBeTrue();
            result.WasSuccessful.ShouldBeFalse();
        }

        [Test]
        public void ThenCanReadScenarioOutlineResultSuccessfully()
        {
            var results = ParseResultsFile();

            var feature = new Feature { Name = "Addition" };

            var scenarioOutline = new ScenarioOutline { Name = "Adding several numbers", Feature = feature };
            TestResult result = results.GetScenarioOutlineResult(scenarioOutline);

            result.WasExecuted.ShouldBeTrue();
            result.WasSuccessful.ShouldBeTrue();

            TestResult exampleResult1 = results.GetExampleResult(scenarioOutline, new[] { "40", "50", "90" });
            exampleResult1.WasExecuted.ShouldBeTrue();
            exampleResult1.WasSuccessful.ShouldBeTrue();

            TestResult exampleResult2 = results.GetExampleResult(scenarioOutline, new[] { "60", "70", "130" });
            exampleResult2.WasExecuted.ShouldBeTrue();
            exampleResult2.WasSuccessful.ShouldBeTrue();
        }

        [Test]
        public void WithoutExampleSignatureBuilderThrowsInvalidOperationException()
        {
            var results = ParseResultsFile();
            results.SetExampleSignatureBuilder(null);

            var feature = new Feature { Name = "Addition" };

            var scenarioOutline = new ScenarioOutline { Name = "Adding several numbers", Feature = feature };

            Assert.Throws<InvalidOperationException>(() => results.GetExampleResult(scenarioOutline, new[] { "40", "50", "90" }));
        }

        [Test]
        public void ThenCanReadSuccessfulScenarioResultSuccessfully()
        {
            var results = ParseResultsFile();

            var feature = new Feature { Name = "Addition" };

            var passedScenario = new Scenario { Name = "Add two numbers", Feature = feature };
            TestResult result = results.GetScenarioResult(passedScenario);

            result.WasExecuted.ShouldBeTrue();
            result.WasSuccessful.ShouldBeTrue();
        }

        [Test]
        public void ThenCanReadFailedScenarioResultSuccessfully()
        {
            var results = ParseResultsFile();
            var feature = new Feature { Name = "Addition" };
            var scenario = new Scenario { Name = "Fail to add two numbers", Feature = feature };
            TestResult result = results.GetScenarioResult(scenario);

            result.WasExecuted.ShouldBeTrue();
            result.WasSuccessful.ShouldBeFalse();
        }

        [Test]
        public void ThenCanReadIgnoredScenarioResultSuccessfully()
        {
            var results = ParseResultsFile();
            var feature = new Feature { Name = "Addition" };
            var ignoredScenario = new Scenario { Name = "Ignored adding two numbers", Feature = feature };
            var result = results.GetScenarioResult(ignoredScenario);

            result.WasExecuted.ShouldBeFalse();
            result.WasSuccessful.ShouldBeFalse();
        }

        [Test]
        public void ThenCanReadInconclusiveScenarioResultSuccessfully()
        {
            var results = ParseResultsFile();
            var feature = new Feature { Name = "Addition" };
            var inconclusiveScenario = new Scenario
            {
                Name = "Not automated adding two numbers",
                Feature = feature
            };
            var result = results.GetScenarioResult(inconclusiveScenario);

            result.WasExecuted.ShouldBeFalse();
            result.WasSuccessful.ShouldBeFalse();
        }

        [Test]
        public void ThenCanReadInconclusiveFeatureResultSuccessfully()
        {
            var results = ParseResultsFile();
            var result = results.GetFeatureResult(InconclusiveFeature());
            Assert.AreEqual(TestResult.Inconclusive, result);
        }


        [Test]
        public void ThenCanReadPassedFeatureResultSuccessfully()
        {
            var results = ParseResultsFile();
            var result = results.GetFeatureResult(PassingFeature());
            Assert.AreEqual(TestResult.Passed, result);
        }

        [Test]
        public void ThenCanReadFailedFeatureResultSuccessfully()
        {
            var results = ParseResultsFile();
            var result = results.GetFeatureResult(FailingFeature());
            Assert.AreEqual(TestResult.Failed, result);
        }

        private Feature FailingFeature()
        {
            return new Feature {Name = "Failing"};
        }

        private Feature InconclusiveFeature()
        {
            return new Feature { Name = "Inconclusive" };
        }

        private Feature PassingFeature()
        {
            return new Feature { Name = "Passing" };
        }

        [Test]
        public void ThenCanReadNotFoundScenarioCorrectly()
        {
            var results = ParseResultsFile();
            var feature = new Feature { Name = "Addition" };
            var notFoundScenario = new Scenario
            {
                Name = "Not in the file at all!",
                Feature = feature
            };

            var result = results.GetScenarioResult(notFoundScenario);

            result.WasExecuted.ShouldBeFalse();
            result.WasSuccessful.ShouldBeFalse();
        }

        [Test]
        public void ThenCanReadNotFoundFeatureCorrectly()
        {
            var results = ParseResultsFile();
            var feature = new Feature {Name = "NotInTheFile"};
            var result = results.GetFeatureResult(feature);
            result.WasExecuted.ShouldBeFalse();
            result.WasSuccessful.ShouldBeFalse();
        }

        [Test]
        public void ThenCanReadIndividualResultsFromScenarioOutline_AllPass_ShouldBeTestResultPassed()
        {
          var results = ParseResultsFile();
          results.SetExampleSignatureBuilder(new NUnitExampleSignatureBuilder());

          var feature = new Feature { Name = "Scenario Outlines" };

          var scenarioOutline = new ScenarioOutline { Name = "This is a scenario outline where all scenarios pass", Feature = feature };

          TestResult exampleResultOutline = results.GetScenarioOutlineResult(scenarioOutline);
          exampleResultOutline.ShouldEqual(TestResult.Passed);

          TestResult exampleResult1 = results.GetExampleResult(scenarioOutline, new[] { "pass_1" });
          exampleResult1.ShouldEqual(TestResult.Passed);

          TestResult exampleResult2 = results.GetExampleResult(scenarioOutline, new[] { "pass_2" });
          exampleResult2.ShouldEqual(TestResult.Passed);

          TestResult exampleResult3 = results.GetExampleResult(scenarioOutline, new[] { "pass_3" });
          exampleResult3.ShouldEqual(TestResult.Passed);
        }

        [Test]
        public void ThenCanReadIndividualResultsFromScenarioOutline_OneInconclusive_ShouldBeTestResultInconclusive()
        {
          var results = ParseResultsFile();
          results.SetExampleSignatureBuilder(new NUnitExampleSignatureBuilder());

          var feature = new Feature { Name = "Scenario Outlines" };

          var scenarioOutline = new ScenarioOutline { Name = "This is a scenario outline where one scenario is inconclusive", Feature = feature };

          TestResult exampleResultOutline = results.GetScenarioOutlineResult(scenarioOutline);
          exampleResultOutline.ShouldEqual(TestResult.Inconclusive);

          TestResult exampleResult1 = results.GetExampleResult(scenarioOutline, new[] { "pass_1" });
          exampleResult1.ShouldEqual(TestResult.Passed);

          TestResult exampleResult2 = results.GetExampleResult(scenarioOutline, new[] { "pass_2" });
          exampleResult2.ShouldEqual(TestResult.Passed);

          TestResult exampleResult3 = results.GetExampleResult(scenarioOutline, new[] { "inconclusive_1" });
          exampleResult3.ShouldEqual(TestResult.Inconclusive);
        }

        [Test]
        public void ThenCanReadIndividualResultsFromScenarioOutline_OneFailed_ShouldBeTestResultFailed()
        {
          var results = ParseResultsFile();
          results.SetExampleSignatureBuilder(new NUnitExampleSignatureBuilder());

          var feature = new Feature { Name = "Scenario Outlines" };

          var scenarioOutline = new ScenarioOutline { Name = "This is a scenario outline where one scenario fails", Feature = feature };

          TestResult exampleResultOutline = results.GetScenarioOutlineResult(scenarioOutline);
          exampleResultOutline.ShouldEqual(TestResult.Failed);

          TestResult exampleResult1 = results.GetExampleResult(scenarioOutline, new[] { "pass_1" });
          exampleResult1.ShouldEqual(TestResult.Passed);

          TestResult exampleResult2 = results.GetExampleResult(scenarioOutline, new[] { "pass_2" });
          exampleResult2.ShouldEqual(TestResult.Passed);

          TestResult exampleResult3 = results.GetExampleResult(scenarioOutline, new[] { "fail_1" });
          exampleResult3.ShouldEqual(TestResult.Failed);
        }

        [Test]
        public void ThenCanReadIndividualResultsFromScenarioOutline_MultipleExampleSections_ShouldBeTestResultFailed()
        {
          var results = ParseResultsFile();
          results.SetExampleSignatureBuilder(new NUnitExampleSignatureBuilder());

          var feature = new Feature { Name = "Scenario Outlines" };

          var scenarioOutline = new ScenarioOutline { Name = "And we can go totally bonkers with multiple example sections.", Feature = feature };

          TestResult exampleResultOutline = results.GetScenarioOutlineResult(scenarioOutline);
          exampleResultOutline.ShouldEqual(TestResult.Failed);

          TestResult exampleResult1 = results.GetExampleResult(scenarioOutline, new[] { "pass_1" });
          exampleResult1.ShouldEqual(TestResult.Passed);

          TestResult exampleResult2 = results.GetExampleResult(scenarioOutline, new[] { "pass_2" });
          exampleResult2.ShouldEqual(TestResult.Passed);

          TestResult exampleResult3 = results.GetExampleResult(scenarioOutline, new[] { "inconclusive_1" });
          exampleResult3.ShouldEqual(TestResult.Inconclusive);

          TestResult exampleResult4 = results.GetExampleResult(scenarioOutline, new[] { "inconclusive_2" });
          exampleResult4.ShouldEqual(TestResult.Inconclusive);

          TestResult exampleResult5 = results.GetExampleResult(scenarioOutline, new[] { "fail_1" });
          exampleResult5.ShouldEqual(TestResult.Failed);

          TestResult exampleResult6 = results.GetExampleResult(scenarioOutline, new[] { "fail_2" });
          exampleResult6.ShouldEqual(TestResult.Failed);
        }

      [Test]
      public void ThenCanReadResultsWithBackslashes()
      {
        var results = ParseResultsFile();
        results.SetExampleSignatureBuilder(new NUnitExampleSignatureBuilder());

        var feature = new Feature { Name = "Scenario Outlines" };

        var scenarioOutline = new ScenarioOutline { Name = "Deal correctly with backslashes in the examples", Feature = feature };

        TestResult exampleResultOutline = results.GetScenarioOutlineResult(scenarioOutline);
        exampleResultOutline.ShouldEqual(TestResult.Passed);

        TestResult exampleResult1 = results.GetExampleResult(scenarioOutline, new[] { @"c:\Temp\" });
        exampleResult1.ShouldEqual(TestResult.Passed);
      }
    }
}