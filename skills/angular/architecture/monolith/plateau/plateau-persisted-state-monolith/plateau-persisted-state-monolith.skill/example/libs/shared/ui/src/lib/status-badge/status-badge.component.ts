import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'ui-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [attr.data-status]="status()">{{ label() }}</span>`,
  styles: `.badge { padding: 2px 8px; border-radius: 4px; font-size: 12px; }`,
})
export class StatusBadgeComponent {
  /** Presentational only — reflected on `data-status` for styling. Any status vocabulary
   *  (a feature's sync status, a workflow state, …) is valid. */
  status = input.required<string>();
  label = input.required<string>();
}
