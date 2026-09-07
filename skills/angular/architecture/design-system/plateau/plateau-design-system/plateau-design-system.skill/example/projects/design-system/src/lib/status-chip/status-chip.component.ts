import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/**
 * Workflow status is a domain concept Angular Material does not model, so this
 * component is built fully custom and consumes the `--ds-color-status-*` tokens.
 */
export type DsStatus = 'start' | 'stop' | 'in-progress';

const STATUS_LABEL: Record<DsStatus, string> = {
  start: 'Running',
  stop: 'Stopped',
  'in-progress': 'In progress',
};

@Component({
  selector: 'ds-status-chip',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="ds-status-chip" [class]="'ds-status-chip--' + status()" data-testid="ds-status-chip" role="status">
      <span class="ds-status-chip__dot" aria-hidden="true"></span>
      {{ resolvedLabel() }}
    </span>
  `,
  styles: `
    :host {
      display: inline-block;
    }
    .ds-status-chip {
      display: inline-flex;
      align-items: center;
      gap: var(--ds-spacing-xs);
      padding: var(--ds-spacing-xs) var(--ds-spacing-sm);
      border-radius: var(--ds-radius-lg);
      /* typography from Material's own system tokens — never re-aliased */
      font: var(--mat-sys-label-large);
      color: var(--ds-color-on-status);
    }
    .ds-status-chip__dot {
      width: 8px;
      height: 8px;
      border-radius: var(--ds-radius-sm);
      background: currentColor;
      opacity: 0.9;
    }
    .ds-status-chip--start {
      background: var(--ds-color-status-start);
    }
    .ds-status-chip--stop {
      background: var(--ds-color-status-stop);
    }
    .ds-status-chip--in-progress {
      background: var(--ds-color-status-in-progress);
    }
  `,
})
export class DsStatusChipComponent {
  readonly status = input.required<DsStatus>();
  /** Overrides the default label for the status. */
  readonly label = input('');

  protected readonly resolvedLabel = computed(() => this.label() || STATUS_LABEL[this.status()]);
}
