import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'ui-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="badge" [attr.data-status]="status()">{{ label() }}</span>`,
  styles: `.badge { padding: 2px 8px; border-radius: 4px; font-size: 12px; }`,
})
export class StatusBadgeComponent {
  status = input.required<'idle' | 'creating' | 'created' | 'error'>();
  label = input.required<string>();
}
