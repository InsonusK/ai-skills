export default {
  paths: ["features/**/*.feature"],
  requireModule: ["tsx/cjs"],
  require: ["features/step-definitions/**/*.ts"],
  tags: "not @status/todo and not @status/broken",
};
