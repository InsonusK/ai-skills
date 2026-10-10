import { Component } from "@angular/core";
import { bootstrapApplication } from "@angular/platform-browser";
import { LinkcheckForm } from "linkcheck";
@Component({
  selector: "app-root",
  imports: [LinkcheckForm],
  template: "<linkcheck-form />",
})
class Demo {}
bootstrapApplication(Demo).catch(console.error);
