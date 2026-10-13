export default {
  paths: ["projects/linkcheck/src/**/features/**/*.feature"],
  requireModule: ["tsx/cjs"],
  require: ["projects/linkcheck/src/**/test/**/*.steps.ts"],
  tags: "not @status/todo and not @status/broken",
};
