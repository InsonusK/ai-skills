import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Presentational "N actions waiting to sync" indicator. The owning feature
 * feeds `count` from `MutationQueueService.pendingForFeature$(...)` (usually via
 * its feature Signal Store) — this component never touches the queue or the
 * store, keeping `shared-ui` boundary-clean.
 */
@Component({
  selector: 'ui-pending-sync-indicator',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (count() > 0) {
      <div role="status" class="pending-sync">
        {{ count() }} {{ count() === 1 ? 'action' : 'actions' }} waiting to sync
      </div>
    }
  `,
  styles: `
    .pending-sync {
      padding: 0.25rem 0.75rem;
      font-size: 0.8125rem;
      color: #92400e;
      background: #fef3c7;
      border-radius: 4px;
    }
  `,
})
export class PendingSyncIndicatorComponent {
  readonly count = input.required<number>();
}
