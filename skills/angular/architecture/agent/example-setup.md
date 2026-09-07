# Stage 4 example — environment + workspace setup

The dotnet v3.1 examples had `.NET 10 SDK` pre-installed; this container has **no Node**. It is installed manually (same pattern):

```bash
# Node (LTS) — nodejs.org + npm registry are reachable from this container
cd /tmp && curl -sSL "https://nodejs.org/dist/$(curl -sS https://nodejs.org/dist/index.json | \
  python3 -c "import sys,json;print([r['version'] for r in json.load(sys.stdin) if r.get('lts')][0])")/node-*-linux-x64.tar.xz" -o node.tar.xz
mkdir -p ~/.local/node && tar -xJf node.tar.xz -C ~/.local/node --strip-components=1
echo 'export PATH="$HOME/.local/node/bin:$PATH"' >> ~/.profile   # + ~/.bashrc
```

Installed: Node v24.20.0, npm 11.19.0. Angular **22.1.4** is `latest` on npm (Signal Forms available). Nx **23.2.0**. NgRx **22.0.0** (peer `@angular/core@^22`).

## `plateau-online-monolith` example — scaffold state (in `/tmp/ng-ex/online-monolith`, not yet committed)

```bash
npx create-nx-workspace@latest online-monolith --preset=angular-monorepo --appName=platform-shell \
  --style=scss --bundler=esbuild --unitTestRunner=vitest --e2eTestRunner=playwright --ssr=false \
  --nxCloud=skip --no-interactive --routing
# the legacy 'angular-monorepo' preset maps to nrwl/angular-template — it ships an SSR shop + api app
rm -rf apps/api packages                          # drop the api node app
npx nx g @nx/workspace:move --project=shop --destination=apps/platform-shell --newProjectName=platform-shell
npx nx g @nx/workspace:move --project=shop-e2e --destination=apps/platform-shell-e2e --newProjectName=platform-shell-e2e
rm apps/platform-shell/src/{main.server.ts,server.ts,app/app.config.server.ts,app/app.routes.server.ts}
# libs (vitest-analog runner — vitest-angular needs buildable/publishable):
for L in shared-util:libs/shared/util shared-ui:libs/shared/ui shared-logging:libs/shared/logging \
         shared-state:libs/shared/state shared-http-core:libs/shared/http-core \
         orders-feature:libs/orders/feature orders-data-access:libs/orders/data-access; do
  IFS=: read N D <<< "$L"; npx nx g @nx/angular:library --name=$N --directory=$D --unitTestRunner=vitest-analog --style=scss --no-interactive; done
npm i @ngrx/store@^22 @ngrx/effects@^22 @ngrx/signals@^22
npm i -D @testing-library/angular @testing-library/user-event @axe-core/playwright
```

**Path aliases** are flat: `@org/shared-util`, `@org/orders-feature`, … (`tsconfig.base.json` still carries stale `@org/shop/*`, `@org/api/products`, `@org/models` — clean them).

## Remaining to green

- Rewrite `apps/platform-shell/src/app/app.config.ts` (drop hydration; add `provideStore`, `provideEffects`, `provideHttpClient`, the `LOG_SINKS` provider), `app.routes.ts` (mount `orders`), `app.ts`/`app.html` (router-outlet shell).
- `apps/platform-shell/project.json` — remove the SSR `server`/`ssr`/`outputMode` build options and the `serve-ssr` target.
- Write the lib code per the `structure/` skills: `LoggerService`+`ConsoleLogSink`+`LOG_SINKS`; `store.config.ts` (empty root store); `BaseHttpService`; `orders` data-access (mapper/errors/client/facade) + feature (routes/store/list+form components).
- Specs: `orders.client.spec.ts` (`HttpTestingController`), `orders.facade.spec.ts` + `orders.store.spec.ts` (TestBed, fake the layer beneath), `order-form.component.spec.ts` (Testing Library).
- `apps/component-preview` (`nx g @nx/angular:app`), one preview route; the 4 visual/style/a11y specs + `read-visual-style-properties.ts` under the component's `spec/`.
- `npx nx run-many -t test` (Vitest, jsdom — no browser needed) → green.
- `npx playwright install chromium` then `npx nx e2e platform-shell-e2e` + the visual specs → green.

Once green: copy the workspace (minus `node_modules`, `.nx`, `dist`) into `skills/angular/architecture/v3.1/monolith/plateau/plateau-online-monolith/plateau-online-monolith.skill/example/`.
