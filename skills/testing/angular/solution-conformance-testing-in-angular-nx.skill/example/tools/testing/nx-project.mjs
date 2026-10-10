import { readFileSync, mkdirSync, writeFileSync } from "node:fs";
import { resolve } from "node:path";
import { spawnSync } from "node:child_process";
const [kind, name] = process.argv.slice(2);
const parent = process.env.TEST_KIND_DIR;
if (!parent || !["unit", "components", "ui"].includes(kind))
  throw new Error("Run through make");
const selection = JSON.parse(
  readFileSync(resolve(parent, "result/projects.json"), "utf8"),
);
const project = selection.projects.find(
  (project) => project.name === name && project.selected,
);
if (!project) throw new Error(`Unexpected project: ${name}`);
const dir = resolve(parent, "projects", name);
for (const folder of ["result", "badges", "report"])
  mkdirSync(resolve(dir, folder), { recursive: true });
const env = {
  ...process.env,
  TEST_KIND_DIR: dir,
  NX_TEST_PROJECT_ROOT: project.root,
  NX_WORKSPACE_DATA_DIRECTORY: resolve(dir, "cache/nx-data"),
  NX_CACHE_DIRECTORY: resolve(dir, "cache/nx-tasks"),
};
// A stopped browser host must not leave task-invocation state for the next project's host.
const bin = (name) => resolve("node_modules/.bin", name);
let command, args;
if (kind === "unit") {
  command = bin("cucumber-js");
  args = [
    "--format",
    "progress",
    "--format",
    `json:${dir}/result/unit.native.json`,
    "--format",
    `message:${dir}/result/messages.ndjson`,
  ];
} else if (kind === "components") {
  command = bin("nx");
  args = [
    "run",
    `${name}:component-native`,
    "--skip-nx-cache",
    "--skipRemoteCache",
    `--outputFile=${dir}/result/components.native.json`,
  ];
} else {
  // Reserve a distinct loopback port for this project's host run.
  const { createServer } = await import("node:net");
  env.UI_TEST_PORT = await new Promise((done, reject) => {
    const server = createServer();
    server.on("error", reject);
    server.listen(0, "127.0.0.1", () => {
      const port = server.address().port;
      server.close(() => done(String(port)));
    });
  });
  env.NX_UI_HOST = project.uiHost;
  command = bin("playwright");
  args = ["test", "--config=playwright.ui.config.ts"];
}
const result = spawnSync(command, args, {
  env,
  encoding: "utf8",
  maxBuffer: 32 * 1024 * 1024,
});
const log = [result.stdout, result.stderr, result.error?.message]
  .filter(Boolean)
  .join("\n");
writeFileSync(resolve(dir, "runner.log"), log);
process.stdout.write(log);
writeFileSync(
  resolve(dir, "result/exit-code.json"),
  JSON.stringify({ project: name, code: result.status ?? 1 }),
);
process.exitCode = result.status ?? 1;
