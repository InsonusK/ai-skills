export default {
  paths: ["src/**/features/**/*.feature"],
  requireModule: ["tsx/cjs"],
  require: ["src/**/test/**/*.steps.ts"],
  tags: "not @status/todo and not @status/broken",
};
