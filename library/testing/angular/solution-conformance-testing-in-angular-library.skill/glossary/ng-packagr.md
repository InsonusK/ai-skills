# ng-packagr

ng-packagr builds an Angular library into the Angular Package Format, so applications can consume its compiled code and type declarations through a public package entry point.

A library's `ng-package.json` names that entry point and output folder. Its TypeScript configuration uses partial Angular compilation and excludes tests. The CLI `@angular/build:ng-packagr` builder runs the tool; consumers install the resulting `dist/linkcheck` package, not the development workspace or demo application.

In this example, `projects/linkcheck` is the package and `projects/demo` is its browser host. The package proof builds the library, then inspects `npm pack --dry-run` output for leaked specs, steps and features.

Sources: [Angular library creation](https://angular.dev/tools/libraries/creating-libraries), [Angular Package Format](https://angular.dev/tools/libraries/angular-package-format).
