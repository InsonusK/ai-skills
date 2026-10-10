import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';
import { SESSION_CONTRACT } from '@platform/contracts';

/**
 * `*hasPermission="'reports.export'"` — shows/hides by a permission string read
 * from the host's SessionContract. A UX affordance only; the real check is server-side.
 */
@Directive({ selector: '[hasPermission]' })
export class HasPermissionDirective {
  // eslint-disable-next-line @angular-eslint/directive-selector -- the *hasPermission convention is mandated by the solution
  readonly hasPermission = input.required<string>();

  private readonly session = inject(SESSION_CONTRACT);
  private readonly tpl = inject(TemplateRef<unknown>);
  private readonly vcr = inject(ViewContainerRef);

  constructor() {
    effect(() => {
      const allowed = this.session.isAuthenticated() && this.session.permissions().includes(this.hasPermission());
      this.vcr.clear();
      if (allowed) this.vcr.createEmbeddedView(this.tpl);
    });
  }
}
