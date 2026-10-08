# Counter illustration

[App](app.ts), [component specs](test/counter.component.spec.ts) and [browser specs](test/counter.ui.spec.ts) exercise the same real component. The browser spec includes a separate visual assertion. These source files illustrate test authoring; they are not a standalone application or a replacement for the inherited TypeScript domain showcase.

To run them, use an Angular CLI application configured by this skill's Implementation files, place these sources under its application source root, bootstrap `App`, and serve that application with the configured UI server command. Install the locked dependencies and matching Chromium with its system libraries. Create and inspect the visual baseline through the separate review procedure before expecting a green normal UI run.

[Verification](../verification.md) records actual positive and negative executions of these files and the delivered assets.
