import {
  readFileSync,
  writeFileSync,
  mkdirSync,
  existsSync,
  cpSync,
  readdirSync,
} from "node:fs";
import { createServer } from "node:net";
import { resolve } from "node:path";
import { spawnSync } from "node:child_process";
const [action, kind] = process.argv.slice(2);
const dir = process.env.TEST_KIND_DIR;
if (!dir || !["unit", "components", "ui"].includes(kind))
  throw new Error("Run through make");
const json = (file) => JSON.parse(readFileSync(file, "utf8"));
const save = (file, data) =>
  writeFileSync(resolve(dir, file), JSON.stringify(data, null, 2));
const nx = (args) => {
  const result = spawnSync(resolve("node_modules/.bin/nx"), args, {
    encoding: "utf8",
    maxBuffer: 32 * 1024 * 1024,
  });
  if (result.status !== 0)
    throw new Error(
      result.stderr || result.stdout || result.error?.message || "Nx failed",
    );
  return JSON.parse(result.stdout);
};
const file = resolve(dir, "result/projects.json");
// Which projects a kind applies to is read from the workspace as Nx sees it - no testing
// declaration of our own in a project: scenarios where a project has feature files,
// component tests where it has the standard "test" target, browser tests where it has the
// standard "e2e" target.
const hasFeatures = (root) => {
  const walk = (folder) =>
    readdirSync(folder, { withFileTypes: true }).some((entry) =>
      entry.isDirectory()
        ? !["node_modules", "dist", "tmp"].includes(entry.name) &&
          walk(resolve(folder, entry.name))
        : entry.name.endsWith(".feature"),
    );
  return existsSync(root) && walk(root);
};
const applies = {
  unit: (config) => hasFeatures(config.root),
  components: (config) => !!config.targets?.test,
  ui: (config) => !!config.targets?.e2e,
};
const freePort = () =>
  new Promise((done, reject) => {
    const server = createServer();
    server.on("error", reject);
    server.listen(0, "127.0.0.1", () => {
      const port = server.address().port;
      server.close(() => done(String(port)));
    });
  });
if (action === "run") {
  const selection = {
    kind,
    projects: [],
    errors: [],
    delta: process.env.TEST_RUN_PURPOSE === "check" && !!process.env.DELTA_BASE,
  };
  try {
    const names = nx(["show", "projects", "--json"]).sort();
    if (!names.length) throw new Error("Workspace has no projects");
    let affected = names;
    if (selection.delta) {
      const ref = spawnSync(
        "git",
        ["rev-parse", "--verify", `${process.env.DELTA_BASE}^{commit}`],
        { encoding: "utf8" },
      );
      if (ref.status !== 0)
        throw new Error("DELTA_BASE does not name a commit");
      affected = nx([
        "show",
        "projects",
        "--affected",
        `--base=${ref.stdout.trim()}`,
        "--json",
      ]);
    }
    for (const name of names) {
      const config = nx(["show", "project", name, "--json"]);
      const enabled = applies[kind](config);
      selection.projects.push({
        name,
        root: config.root,
        enabled,
        selected: enabled && affected.includes(name),
        reason: !enabled
          ? `no ${kind === "unit" ? "feature file" : kind === "components" ? '"test" target' : '"e2e" target'}`
          : !affected.includes(name)
            ? "unaffected"
            : "selected",
      });
    }
    if (!selection.projects.some((project) => project.enabled))
      selection.errors.push(`No project has ${kind} tests`);
  } catch (error) {
    selection.errors.push(error.message);
  }
  save("result/projects.json", selection);
  if (
    !selection.errors.length &&
    !selection.projects.some((project) => project.selected)
  ) {
    selection.skipReason = `no affected projects with ${kind} tests since ${process.env.DELTA_BASE}`;
    save("result/projects.json", selection);
    process.exit(0);
  }
  // One run per selected project, each into a directory of its own, so a result always
  // says which project it belongs to and a missing one is noticed.
  let log = "";
  for (const project of selection.errors.length
    ? []
    : selection.projects.filter((project) => project.selected)) {
    const projectDir = resolve(dir, "projects", project.name);
    for (const folder of ["result", "report"])
      mkdirSync(resolve(projectDir, folder), { recursive: true });
    const env = { ...process.env, TEST_KIND_DIR: projectDir };
    const bin = (name) => resolve("node_modules/.bin", name);
    let command, args;
    if (kind === "unit") {
      env.NX_TEST_PROJECT_ROOT = project.root;
      command = bin("cucumber-js");
      args = [
        "--format",
        "progress",
        "--format",
        `json:${projectDir}/result/unit.native.json`,
        "--format",
        `message:${projectDir}/result/messages.ndjson`,
      ];
    } else if (kind === "components") {
      command = bin("nx");
      args = [
        "run",
        `${project.name}:test`,
        "--skip-nx-cache",
        "--reporter=json",
        `--outputFile=${projectDir}/result/components.native.json`,
        "--passWithNoTests=false",
        "--coverage",
        `--coverage.reportsDirectory=${projectDir}/report/components/coverage`,
        "--coverage.reporter=html",
        "--coverage.reporter=json-summary",
      ];
    } else {
      env.UI_TEST_PORT = await freePort();
      command = bin("nx");
      args = ["run", `${project.name}:e2e`, "--skip-nx-cache"];
    }
    const result = spawnSync(command, args, {
      env,
      encoding: "utf8",
      maxBuffer: 32 * 1024 * 1024,
    });
    const output = [result.stdout, result.stderr, result.error?.message]
      .filter(Boolean)
      .join("\n");
    writeFileSync(resolve(projectDir, "runner.log"), output);
    log += `== ${project.name}\n${output}\n`;
    const code = result.status ?? 1;
    if (code !== 0) selection.errors.push(`${project.name}: runner exit ${code}`);
    if (!existsSync(resolve(projectDir, "result", `${kind}.native.json`)))
      selection.errors.push(`${project.name}: native result missing`);
  }
  process.stdout.write(log);
  writeFileSync(resolve(dir, "runner.log"), log);
  selection.runnerExit = selection.errors.length ? 1 : 0;
  save("result/projects.json", selection);
  process.exitCode = selection.runnerExit;
} else if (action === "native") {
  const selection = json(file),
    suites = [];
  const report = resolve(dir, "report", kind);
  mkdirSync(report, { recursive: true });
  for (const project of selection.projects.filter(
    (project) => project.selected,
  )) {
    const projectDir = resolve(dir, "projects", project.name);
    try {
      const native = json(resolve(projectDir, "result", `${kind}.native.json`));
      if (kind === "components") {
        const tests = (native.testResults ?? []).flatMap(
          (suite) => suite.assertionResults ?? [],
        );
        if (!tests.length)
          selection.errors.push(`${project.name}: empty component suite`);
        if (native.success !== true || native.numRuntimeErrorTestSuites > 0)
          selection.errors.push(
            `${project.name}: unsuccessful component runner`,
          );
        for (const suite of native.testResults ?? [])
          suites.push({
            ...suite,
            assertionResults: (suite.assertionResults ?? []).map((test) => ({
              ...test,
              fullName: `[${project.name}] ${test.fullName ?? test.title}`,
            })),
          });
      } else {
        const count = (suite) =>
          (suite.specs ?? []).reduce(
            (n, spec) => n + (spec.tests?.length ?? 0),
            0,
          ) + (suite.suites ?? []).reduce((n, child) => n + count(child), 0);
        if (!(native.suites ?? []).reduce((n, suite) => n + count(suite), 0))
          selection.errors.push(`${project.name}: empty browser suite`);
        selection.errors.push(
          ...(native.errors ?? []).map(
            (error) => `${project.name}: ${error.message}`,
          ),
        );
        suites.push({ title: project.name, suites: native.suites ?? [] });
      }
    } catch (error) {
      selection.errors.push(`${project.name}: ${error.message}`);
    }
    const target = resolve(report, "projects", project.name);
    mkdirSync(target, { recursive: true });
    if (existsSync(resolve(projectDir, "report", kind)))
      cpSync(resolve(projectDir, "report", kind), target, { recursive: true });
    if (existsSync(resolve(projectDir, "runner.log")))
      cpSync(resolve(projectDir, "runner.log"), resolve(target, "runner.log"));
  }
  const errors = [...new Set(selection.errors)];
  save(
    `result/${kind}.native.json`,
    kind === "components"
      ? {
          success: !errors.length,
          testResults: suites,
          numRuntimeErrorTestSuites: errors.length,
        }
      : { suites, errors: errors.map((message) => ({ message })) },
  );
  if (existsSync(resolve(dir, "runner.log")))
    cpSync(resolve(dir, "runner.log"), resolve(report, "runner.log"));
  const result = spawnSync(
    process.execPath,
    [
      "tools/testing/angular-results.mjs",
      kind,
      String(errors.length ? 1 : (selection.runnerExit ?? 1)),
    ],
    { stdio: "inherit" },
  );
  const escape = (value) =>
    String(value).replace(
      /[&<>"']/g,
      (c) =>
        ({
          "&": "&amp;",
          "<": "&lt;",
          ">": "&gt;",
          '"': "&quot;",
          "'": "&#39;",
        })[c],
    );
  const rows = selection.projects
    .map((project) => {
      const links = project.selected
        ? [
            "runner.log",
            kind === "ui" ? "playwright/index.html" : "coverage/index.html",
          ]
            .filter((path) =>
              existsSync(resolve(report, "projects", project.name, path)),
            )
            .map(
              (path) =>
                `<a href="projects/${encodeURIComponent(project.name)}/${path}">${escape(path)}</a>`,
            )
            .join(" ")
        : "";
      return `<tr><td>${escape(project.name)}</td><td>${escape(project.reason)}</td><td>${links}</td></tr>`;
    })
    .join("");
  const index = resolve(report, "index.html");
  writeFileSync(
    index,
    readFileSync(index, "utf8").replace(
      "</html>",
      `<h2>Projects</h2><table>${rows}</table><pre>${escape(errors.join("\n"))}</pre></html>`,
    ),
  );
  save("result/projects.json", selection);
  process.exitCode = result.status ?? 1;
} else if (action === "unit") {
  const selection = json(file),
    results = [];
  let total = 0,
    passed = 0;
  const cucumber = resolve(dir, "report/tests/cucumber");
  mkdirSync(cucumber, { recursive: true });
  for (const project of selection.projects.filter(
    (project) => project.selected,
  )) {
    try {
      const projectDir = resolve(dir, "projects", project.name, "result");
      const features = json(resolve(projectDir, "unit.native.json"));
      const scenarios = features
        .flatMap((feature) => feature.elements ?? [])
        .filter((scenario) => scenario.type === "scenario");
      const successful = scenarios.filter(
        (scenario) =>
          scenario.steps?.length &&
          scenario.steps.every((step) => step.result?.status === "passed"),
      ).length;
      if (!scenarios.length)
        selection.errors.push(`${project.name}: empty Cucumber suite`);
      total += scenarios.length;
      passed += successful;
      cpSync(
        resolve(projectDir, "messages.ndjson"),
        resolve(cucumber, `${project.name}.ndjson`),
      );
      results.push({
        project: project.name,
        total: scenarios.length,
        passed: successful,
      });
    } catch (error) {
      selection.errors.push(`${project.name}: ${error.message}`);
    }
  }
  save("result/unit-test.json", {
    total,
    passed,
    failed: total - passed,
    projects: results,
    errors: selection.errors,
  });
  save("result/projects.json", selection);
  process.exitCode =
    selection.errors.length || total === 0 || passed !== total ? 1 : 0;
} else if (action === "inventory") {
  const selection = json(file),
    summary = json(resolve(dir, "result/unit-test.json"));
  const report = resolve(dir, "report/tests");
  const escape = (value) =>
    String(value).replace(
      /[&<>"']/g,
      (c) =>
        ({
          "&": "&amp;",
          "<": "&lt;",
          ">": "&gt;",
          '"': "&quot;",
          "'": "&#39;",
        })[c],
    );
  const rows = selection.projects
    .map((project) => {
      const counts = summary.projects.find(
        (item) => item.project === project.name,
      );
      let links = "";
      if (project.selected) {
        const from = resolve(dir, "projects", project.name),
          to = resolve(report, "projects", project.name);
        mkdirSync(to, { recursive: true });
        for (const path of ["runner.log", "result/unit.native.json"]) {
          if (existsSync(resolve(from, path))) {
            const name = path.split("/").at(-1);
            cpSync(resolve(from, path), resolve(to, name));
            links += `<a href="projects/${encodeURIComponent(project.name)}/${name}">${escape(name)}</a> `;
          }
        }
      }
      return `<tr><td>${escape(project.name)}</td><td>${escape(project.reason)}</td><td>${counts ? `${counts.passed}/${counts.total}` : "not run"}</td><td>${links}</td></tr>`;
    })
    .join("");
  writeFileSync(
    resolve(report, "projects.html"),
    `<!doctype html><html lang="en"><meta charset="utf-8"><title>Unit projects</title><h1>Unit projects</h1><table><tr><th>Project</th><th>Selection</th><th>Passed</th><th>Evidence</th></tr>${rows}</table><pre>${escape(selection.errors.join("\n"))}</pre></html>`,
  );
  const livingdoc = resolve(report, "livingdoc/index.html");
  if (existsSync(livingdoc))
    writeFileSync(
      livingdoc,
      readFileSync(livingdoc, "utf8").replace(
        "</body>",
        '<p><a href="../projects.html">Project selection and runner evidence</a></p></body>',
      ),
    );
}
