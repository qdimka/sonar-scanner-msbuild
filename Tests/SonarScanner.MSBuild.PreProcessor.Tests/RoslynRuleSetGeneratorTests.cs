﻿/*
 * SonarScanner for MSBuild
 * Copyright (C) 2016-2019 SonarSource SA
 * mailto:info AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarScanner.MSBuild.Common;
using SonarScanner.MSBuild.PreProcessor.Roslyn.Model;

namespace SonarScanner.MSBuild.PreProcessor.Tests
{
    [TestClass]
    public class RoslynRuleSetGeneratorTests
    {
        [TestMethod]
        public void RoslynRuleSet_ConstructorArgumentChecks()
        {
            Action act = () => new RoslynRuleSetGenerator(null);
            act.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void RoslynRuleSet_GeneratorArgumentChecks()
        {
            IDictionary<string, string> dict = new Dictionary<string, string>();
            var generator = new RoslynRuleSetGenerator(dict);
            IEnumerable<SonarRule> activeRules = new List<SonarRule>();
            IEnumerable<SonarRule> inactiveRules = new List<SonarRule>();
            var language = "cs";

            Action act1 = () => generator.Generate(null, activeRules, inactiveRules);
            act1.Should().ThrowExactly<ArgumentNullException>();

            Action act2 = () => generator.Generate(language, activeRules, null);
            act2.Should().ThrowExactly<ArgumentNullException>();

            Action act3 = () => generator.Generate(language, null, inactiveRules);
            act3.Should().ThrowExactly<ArgumentNullException>();
        }

        [TestMethod]
        public void RoslynRuleSet_Empty()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>());
            var activeRules = new List<SonarRule>
            {
                new SonarRule("repo", "key"),
            };
            var inactiveRules = new List<SonarRule>();

            // Act
            var ruleSet = generator.Generate("cs", activeRules, inactiveRules);

            // Assert
            ruleSet.Rules.Should().BeEmpty(); // No analyzers
            ruleSet.Description.Should().Be("This rule set was automatically generated from SonarQube");
            ruleSet.ToolsVersion.Should().Be("14.0");
            ruleSet.Name.Should().Be("Rules for SonarQube");
        }

        [TestMethod]
        public void RoslynRuleSet_ActiveRuleAction_None()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                ["sonaranalyzer-cs.analyzerId"] = "SonarAnalyzer.CSharp",
                ["sonaranalyzer-cs.ruleNamespace"] = "SonarAnalyzer.CSharp",
            });

            var activeRules = new List<SonarRule>
            {
                new SonarRule("csharpsquid", "rule1", isActive: true),
                new SonarRule("csharpsquid", "rule2", isActive: true),
            };

            // Act
            generator.ActiveRuleAction = RuleAction.None;
            var ruleSet = generator.Generate("cs", activeRules, Enumerable.Empty<SonarRule>());

            // Assert
            ruleSet.Rules.Should().HaveCount(1);
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo("None", "None");
        }

        [TestMethod]
        public void RoslynRuleSet_ActiveRuleAction_Info()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                ["sonaranalyzer-cs.analyzerId"] = "SonarAnalyzer.CSharp",
                ["sonaranalyzer-cs.ruleNamespace"] = "SonarAnalyzer.CSharp",
            });

            var activeRules = new List<SonarRule>
            {
                new SonarRule("csharpsquid", "rule1", isActive: true),
                new SonarRule("csharpsquid", "rule2", isActive: true),
            };

            // Act
            generator.ActiveRuleAction = RuleAction.Info;
            var ruleSet = generator.Generate("cs", activeRules, Enumerable.Empty<SonarRule>());

            // Assert
            ruleSet.Rules.Should().HaveCount(1);
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo("Info", "Info");
        }

        [TestMethod]
        public void RoslynRuleSet_ActiveRuleAction_Warning()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                ["sonaranalyzer-cs.analyzerId"] = "SonarAnalyzer.CSharp",
                ["sonaranalyzer-cs.ruleNamespace"] = "SonarAnalyzer.CSharp",
            });

            var activeRules = new List<SonarRule>
            {
                new SonarRule("csharpsquid", "rule1", isActive: true),
                new SonarRule("csharpsquid", "rule2", isActive: true),
            };

            // Act
            generator.ActiveRuleAction = RuleAction.Warning;
            var ruleSet = generator.Generate("cs", activeRules, Enumerable.Empty<SonarRule>());

            // Assert
            ruleSet.Rules.Should().HaveCount(1);
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo("Warning", "Warning");
        }

        [TestMethod]
        public void RoslynRuleSet_ActiveRuleAction_Error()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                ["sonaranalyzer-cs.analyzerId"] = "SonarAnalyzer.CSharp",
                ["sonaranalyzer-cs.ruleNamespace"] = "SonarAnalyzer.CSharp",
            });

            var activeRules = new List<SonarRule>
            {
                new SonarRule("csharpsquid", "rule1", isActive: true),
                new SonarRule("csharpsquid", "rule2", isActive: true),
            };

            // Act
            generator.ActiveRuleAction = RuleAction.Error;
            var ruleSet = generator.Generate("cs", activeRules, Enumerable.Empty<SonarRule>());

            // Assert
            ruleSet.Rules.Should().HaveCount(1);
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo("Error", "Error");
        }

        [TestMethod]
        public void RoslynRuleSet_ActiveRuleAction_Hidden()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                ["sonaranalyzer-cs.analyzerId"] = "SonarAnalyzer.CSharp",
                ["sonaranalyzer-cs.ruleNamespace"] = "SonarAnalyzer.CSharp",
            });

            var activeRules = new List<SonarRule>
            {
                new SonarRule("csharpsquid", "rule1", isActive: true),
                new SonarRule("csharpsquid", "rule2", isActive: true),
            };

            // Act
            generator.ActiveRuleAction = RuleAction.Hidden;
            var ruleSet = generator.Generate("cs", activeRules, Enumerable.Empty<SonarRule>());

            // Assert
            ruleSet.Rules.Should().HaveCount(1);
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo("Hidden", "Hidden");
        }

        [TestMethod]
        public void RoslynRuleSet_ActiveRuleAction_Unsupported()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>());

            // Act & Assert
            var action = new Action(() => generator.ActiveRuleAction = (RuleAction)5);
            action.Should().ThrowExactly<NotSupportedException>().And
                .Message.Should().Be("5 is not a supported RuleAction.");
        }

        [TestMethod]
        public void RoslynRuleSet_InactiveRules_Always_None()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                ["sonaranalyzer-cs.analyzerId"] = "SonarAnalyzer.CSharp",
                ["sonaranalyzer-cs.ruleNamespace"] = "SonarAnalyzer.CSharp",
            });

            var inactiveRules = new List<SonarRule>
            {
                new SonarRule("csharpsquid", "rule1", isActive: false),
                new SonarRule("csharpsquid", "rule2", isActive: false),
            };

            // Act
            generator.ActiveRuleAction = RuleAction.Error;
            var ruleSet = generator.Generate("cs", Enumerable.Empty<SonarRule>(), inactiveRules);

            // Assert
            ruleSet.Rules.Should().HaveCount(1);
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo("None", "None");
        }

        [TestMethod]
        public void RoslynRuleSet_Unsupported_Rules_Ignored()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>());

            var activeRules = new[]
            {
                new SonarRule("other.repo", "other.rule1", true),
            };
            var inactiveRules = new[]
            {
                new SonarRule("other.repo", "other.rule2", false),
            };

            // Act
            var ruleSet = generator.Generate("cs", activeRules, inactiveRules);

            // Assert
            ruleSet.Rules.Should().BeEmpty();
        }

        [TestMethod]
        public void RoslynRuleSet_RoslynSDK_Rules_Added()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                ["custom1.analyzerId"] = "CustomAnalyzer1",
                ["custom1.ruleNamespace"] = "CustomNamespace1",
                ["custom2.analyzerId"] = "CustomAnalyzer2",
                ["custom2.ruleNamespace"] = "CustomNamespace2",
            });

            var activeRules = new[]
            {
                new SonarRule("roslyn.custom1", "active1", true),
                new SonarRule("roslyn.custom2", "active2", true),
            };
            var inactiveRules = new[]
            {
                new SonarRule("roslyn.custom1", "inactive1", false),
                new SonarRule("roslyn.custom2", "inactive2", false),
            };

            // Act
            var ruleSet = generator.Generate("cs", activeRules, inactiveRules);

            // Assert
            ruleSet.Rules.Should().HaveCount(2);

            ruleSet.Rules[0].RuleNamespace.Should().Be("CustomNamespace1");
            ruleSet.Rules[0].AnalyzerId.Should().Be("CustomAnalyzer1");
            ruleSet.Rules[0].RuleList.Should().HaveCount(2);
            ruleSet.Rules[0].RuleList.Select(r => r.Id).Should().BeEquivalentTo("active1", "inactive1");
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo("Warning", "None");

            ruleSet.Rules[1].RuleNamespace.Should().Be("CustomNamespace2");
            ruleSet.Rules[1].AnalyzerId.Should().Be("CustomAnalyzer2");
            ruleSet.Rules[1].RuleList.Should().HaveCount(2);
            ruleSet.Rules[1].RuleList.Select(r => r.Id).Should().BeEquivalentTo("active2", "inactive2");
            ruleSet.Rules[1].RuleList.Select(r => r.Action).Should().BeEquivalentTo("Warning", "None");
        }

        [TestMethod]
        public void RoslynRuleSet_Sonar_Rules_Added()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                { "sonaranalyzer-cs.analyzerId", "SonarAnalyzer.CSharp" },
                { "sonaranalyzer-cs.ruleNamespace", "SonarAnalyzer.CSharp" },
            });

            var activeRules = new[]
            {
                new SonarRule("csharpsquid", "active1", true),
                // Even though this rule is for VB it will be added as C#, see NOTE below
                new SonarRule("vbnet", "active2", true),
            };
            var inactiveRules = new[]
            {
                new SonarRule("csharpsquid", "inactive1", false),
                // Even though this rule is for VB it will be added as C#, see NOTE below
                new SonarRule("vbnet", "inactive2", false),
            };

            // Act
            var ruleSet = generator.Generate("cs", activeRules, inactiveRules);

            // Assert
            ruleSet.Rules.Should().HaveCount(1);

            // NOTE: The RuleNamespace and AnalyzerId are taken from the language parameter of the
            // Generate method. The FetchArgumentsAndRulesets method will retrieve active/inactive
            // rules from SonarQube per language/quality profile and mixture of VB-C# rules is not
            // expected.
            ruleSet.Rules[0].RuleNamespace.Should().Be("SonarAnalyzer.CSharp");
            ruleSet.Rules[0].AnalyzerId.Should().Be("SonarAnalyzer.CSharp");
            ruleSet.Rules[0].RuleList.Should().HaveCount(4);
            ruleSet.Rules[0].RuleList.Select(r => r.Id).Should().BeEquivalentTo(
                "active1", "active2", "inactive1", "inactive2");
            ruleSet.Rules[0].RuleList.Select(r => r.Action).Should().BeEquivalentTo(
                "Warning", "Warning", "None", "None");
        }

        [TestMethod]
        public void RoslynRuleSet_Common_Parameters()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>());

            // Act
            var ruleSet = generator.Generate("cs", Enumerable.Empty<SonarRule>(), Enumerable.Empty<SonarRule>());

            // Assert
            ruleSet.Description.Should().Be("This rule set was automatically generated from SonarQube");
            ruleSet.ToolsVersion.Should().Be("14.0");
            ruleSet.Name.Should().Be("Rules for SonarQube");
        }

        [TestMethod]
        public void RoslynRuleSet_AnalyzerId_Proprety_Missing()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                { "sonaranalyzer-cs.ruleNamespace", "SonarAnalyzer.CSharp" },
            });

            var activeRules = new[]
            {
                new SonarRule("csharpsquid", "active1", true),
            };

            // Act & Assert
            var action = new Action(() => generator.Generate("cs", activeRules, Enumerable.Empty<SonarRule>()));
            action.Should().ThrowExactly<AnalysisException>().And
                .Message.Should().StartWith(
                    "Property does not exist: sonaranalyzer-cs.analyzerId. This property should be set by the plugin in SonarQube.");
        }

        [TestMethod]
        public void RoslynRuleSet_RuleNamespace_Proprety_Missing()
        {
            // Arrange
            var generator = new RoslynRuleSetGenerator(new Dictionary<string, string>
            {
                { "sonaranalyzer-cs.analyzerId", "SonarAnalyzer.CSharp" },
            });

            var activeRules = new[]
            {
                new SonarRule("csharpsquid", "active1", true),
            };

            // Act & Assert
            var action = new Action(() => generator.Generate("cs", activeRules, Enumerable.Empty<SonarRule>()));
            action.Should().ThrowExactly<AnalysisException>().And
                .Message.Should().StartWith(
                    "Property does not exist: sonaranalyzer-cs.ruleNamespace. This property should be set by the plugin in SonarQube.");
        }
    }
}
