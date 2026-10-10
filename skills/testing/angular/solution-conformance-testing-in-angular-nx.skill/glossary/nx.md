# Nx

Nx is a workspace task runner that keeps a graph of projects and their dependencies. It exists so a repository can run a named target across its applications and libraries and select projects affected by a change.

A project's `project.json` defines targets and configuration. `nx.json` defines workspace defaults and plugins. Dependency edges come from installed plugins or explicit declarations; `nx affected` uses changed files and those edges to select changed projects and their dependants. In this example, changing the formatter selects the formatter and the portal application.

The testing refinement bypasses Nx task caches: its reports prove what ran now. Project targets produce isolated native evidence, and a kind brings that evidence into the one repository report.

Sources: [Nx Angular integration](https://nx.dev/docs/technologies/angular/introduction), [Nx command reference](https://nx.dev/docs/reference/nx-commands), [Nx project configuration](https://nx.dev/docs/reference/project-configuration).
