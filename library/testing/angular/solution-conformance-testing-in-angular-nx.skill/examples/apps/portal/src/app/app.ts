import { Component } from "@angular/core";
import { LinkcheckForm } from "@org/linkcheck";
import { portalHeading } from "../logic/heading";
@Component({
  selector: "app-root",
  imports: [LinkcheckForm],
  template: "<h1>{{ heading }}</h1><linkcheck-form />",
})
export class App {
  readonly heading = portalHeading(1);
}
