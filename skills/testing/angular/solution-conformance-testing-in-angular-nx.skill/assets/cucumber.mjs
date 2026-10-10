const root = process.env["NX_TEST_PROJECT_ROOT"];
export default {
  paths: root
    ? [`${root}/src/**/features/**/*.feature`]
    : ["apps/**/features/**/*.feature", "libs/**/features/**/*.feature"],
  requireModule: ["tsx/cjs"],
  require: ["apps/**/test/**/*.steps.ts", "libs/**/test/**/*.steps.ts"],
  tags: "not @status/todo and not @status/broken",
};
