import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';
import { Store } from '@ngrx/store';
import { selectPermissions } from '@org/shared-state';

/**
 * `<x *hasPermission="'orders.delete'">` — shows/hides UI by permission string
 * (never a role name). A UI convenience only — the backend remains the real
 * authorization boundary.
 *
 * The `hasPermission` selector (no `auth` prefix) is the deliberate, readable
 * convention mandated by solution-authentication — it reads as an `*ngIf`-style
 * structural directive at the call site.
 */
// eslint-disable-next-line @angular-eslint/directive-selector
@Directive({ selector: '[hasPermission]', standalone: true })
export class HasPermissionDirective {
  readonly hasPermission = input.required<string>();
  private readonly permissions = inject(Store).selectSignal(selectPermissions);
  private readonly templateRef = inject(TemplateRef);
  private readonly viewContainer = inject(ViewContainerRef);
  private rendered = false;

  constructor() {
    effect(() => {
      const allowed = this.permissions().includes(this.hasPermission());
      if (allowed && !this.rendered) {
        this.viewContainer.createEmbeddedView(this.templateRef);
        this.rendered = true;
      } else if (!allowed && this.rendered) {
        this.viewContainer.clear();
        this.rendered = false;
      }
    });
  }
}
