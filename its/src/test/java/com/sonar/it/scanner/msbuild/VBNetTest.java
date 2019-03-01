/*
 * Scanner for MSBuild :: Integration Tests
 * Copyright (C) 2016-2018 SonarSource SA
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
package com.sonar.it.scanner.msbuild;

import com.sonar.orchestrator.Orchestrator;
import com.sonar.orchestrator.build.BuildResult;
import com.sonar.orchestrator.container.Edition;
import com.sonar.orchestrator.locator.FileLocation;
import com.sonar.orchestrator.locator.MavenLocation;
import java.nio.file.Path;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;
import java.util.stream.Collectors;
import javax.annotation.CheckForNull;
import javax.annotation.Nullable;
import org.junit.Assume;
import org.junit.Before;
import org.junit.BeforeClass;
import org.junit.ClassRule;
import org.junit.Test;
import org.junit.rules.TemporaryFolder;
import org.sonar.wsclient.issue.Issue;
import org.sonar.wsclient.issue.IssueQuery;
import org.sonarqube.ws.WsMeasures;
import org.sonarqube.ws.client.HttpConnector;
import org.sonarqube.ws.client.WsClient;
import org.sonarqube.ws.client.WsClientFactories;
import org.sonarqube.ws.client.measure.ComponentWsRequest;

import static java.util.Objects.requireNonNull;
import static org.assertj.core.api.Assertions.assertThat;

/**
 * Only vbnet, without C# plugin
 *
 */
public class VBNetTest {

  @BeforeClass
  public static void checkSkip() {
    Assume.assumeTrue("Disable for old scanner (needs C# plugin installed to get the payload)",
      TestUtils.getScannerVersion(ORCHESTRATOR) == null || !TestUtils.getScannerVersion(ORCHESTRATOR).equals("2.1.0.0"));
  }

  @ClassRule
  public static Orchestrator ORCHESTRATOR = Orchestrator.builderEnv()
    .setSonarVersion(TestUtils.replaceLtsVersion(System.getProperty("sonar.runtimeVersion", "LATEST_RELEASE")))
    .setEdition(Edition.DEVELOPER)
    .addPlugin(MavenLocation.of("org.sonarsource.dotnet", "sonar-vbnet-plugin", "LATEST_RELEASE"))
    .activateLicense()
    .build();

  private static final String PROJECT_KEY = "my.project";

  @ClassRule
  public static TemporaryFolder temp = TestUtils.createTempFolder();

  @Before
  public void cleanup() {
    ORCHESTRATOR.resetData();
  }

  @Test
  public void testVBNetOnly() throws Exception {
    ORCHESTRATOR.getServer().restoreProfile(FileLocation.of("projects/ConsoleMultiLanguage/TestQualityProfileVBNet.xml"));
    ORCHESTRATOR.getServer().provisionProject(PROJECT_KEY, "multilang");
    ORCHESTRATOR.getServer().associateProjectToQualityProfile(PROJECT_KEY, "vbnet", "ProfileForTestVBNet");

    Path projectDir = TestUtils.projectDir(temp, "ConsoleMultiLanguage");
    ORCHESTRATOR.executeBuild(TestUtils.newScanner(ORCHESTRATOR, projectDir)
      .addArgument("begin")
      .setProjectKey(PROJECT_KEY)
      .setProjectName("multilang")
      .setProjectVersion("1.0")
      .setDebugLogs(true));

    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Rebuild");

    ORCHESTRATOR.executeBuild(TestUtils.newScanner(ORCHESTRATOR, projectDir)
      .addArgument("end"));

    List<Issue> issues = ORCHESTRATOR.getServer().wsClient().issueClient().find(IssueQuery.create()).list();

    List<String> keys = issues.stream().map(Issue::ruleKey).collect(Collectors.toList());
    assertThat(keys).containsAll(Arrays.asList("vbnet:S3385",
      "vbnet:S2358"));

    assertThat(getMeasureAsInteger(PROJECT_KEY, "ncloc")).isEqualTo(23);
    assertThat(getMeasureAsInteger(getFileKey(), "ncloc")).isEqualTo(10);
  }

  @Test
  public void checkExternalIssuesVB() throws Exception {
    ORCHESTRATOR.getServer().restoreProfile(FileLocation.of("projects/ExternalIssuesVB/TestQualityProfileExternalIssuesVB.xml"));
    ORCHESTRATOR.getServer().provisionProject(PROJECT_KEY, "sample");
    ORCHESTRATOR.getServer().associateProjectToQualityProfile(PROJECT_KEY, "vbnet", "ProfileForTestExternalIssuesVB");

    Path projectDir = TestUtils.projectDir(temp, "ExternalIssuesVB");
    ORCHESTRATOR.executeBuild(TestUtils.newScanner(ORCHESTRATOR, projectDir)
      .addArgument("begin")
      .setProjectKey(PROJECT_KEY)
      .setProjectName("sample")
      .setProjectVersion("1.0"));

    TestUtils.runMSBuild(ORCHESTRATOR, projectDir, "/t:Rebuild");

    BuildResult result = ORCHESTRATOR.executeBuild(TestUtils.newScanner(ORCHESTRATOR, projectDir)
      .addArgument("end"));

    List<Issue> issues = ORCHESTRATOR.getServer().wsClient().issueClient().find(IssueQuery.create()).list();
    List<String> keys = issues.stream().map(Issue::ruleKey).collect(Collectors.toList());

    // The same set of Sonar issues should be reported, regardless of whether
    // external issues are imported or not
    assertThat(keys).containsAll(Arrays.asList(
      "vbnet:S112",
      "vbnet:S3385"));

    if (ORCHESTRATOR.getServer().version().isGreaterThanOrEquals(7,4))
    {
      // if external issues are imported, then there should also be some CodeCracker errors.
      assertThat(keys).containsAll(Arrays.asList(
        "external_roslyn:CC0021",
        "external_roslyn:CC0062"));

      assertThat(issues).hasSize(4);

    } else {
      // Not expecting any external issues
      assertThat(issues).hasSize(2);
    }
  }

  @CheckForNull
  private static Integer getMeasureAsInteger(String componentKey, String metricKey) {
    WsMeasures.Measure measure = getMeasure(componentKey, metricKey);
    return (measure == null) ? null : Integer.parseInt(measure.getValue());
  }

  @CheckForNull
  private static WsMeasures.Measure getMeasure(@Nullable String componentKey, String metricKey) {
    WsMeasures.ComponentWsResponse response = newWsClient().measures().component(new ComponentWsRequest()
      .setComponentKey(componentKey)
      .setMetricKeys(Collections.singletonList(metricKey)));
    List<WsMeasures.Measure> measures = response.getComponent().getMeasuresList();
    return measures.size() == 1 ? measures.get(0) : null;
  }

  private static WsClient newWsClient() {
    return WsClientFactories.getDefault().newClient(HttpConnector.newBuilder()
      .url(ORCHESTRATOR.getServer().getUrl())
      .build());
  }

  private String getFileKey() {
    return TestUtils.hasModules(ORCHESTRATOR) ? "my.project:my.project:60FFCB5D-A35A-43B2-8FE3-F37C8F3B742B:Module1.vb" : "my.project:ConsoleVBNet/Module1.vb";
  }
}
