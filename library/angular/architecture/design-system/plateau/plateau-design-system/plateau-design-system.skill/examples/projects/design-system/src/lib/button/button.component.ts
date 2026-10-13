import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatButton } from '@angular/material/button';

/**
 * Usage axes designed around how this application actually uses buttons —
 * deliberately NOT a 1:1 mirror of Angular Material's own `appearance` values.
 */
export type DsButtonVariant = 'solid' | 'outline' | 'text';
export type DsButtonTone = 'primary' | 'danger';

// Local literal type — never `MatButtonAppearance`, so no Material type reaches
// the generated .d.ts (even a `protected` field is emitted by ng-packagr).
type MatAppearance = 'filled' | 'outlined' | 'text';

const VARIANT_TO_MAT: Record<DsButtonVariant, MatAppearance> = {
  solid: 'filled',
  outline: 'outlined',
  text: 'text',
};

@Component({
  selector: 'ds-button',
  imports: [MatButton],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      [matButton]="matAppearance()"
      data-testid="ds-button"
      [class.ds-button--danger]="tone() === 'danger'"
      [disabled]="disabled()"
      (click)="pressed.emit()"
    >
      <ng-content>{{ label() }}</ng-content>
    </button>
  `,
  styles: `
    :host {
      display: inline-block;
    }
    .ds-button--danger {
      --mat-button-filled-container-color: var(--mat-sys-error);
      --mat-button-filled-label-text-color: var(--mat-sys-on-error);
      --mat-button-outlined-label-text-color: var(--mat-sys-error);
      --mat-button-text-label-text-color: var(--mat-sys-error);
    }
  `,
})
export class DsButtonComponent {
  /** Text label. Ignored when projected content is supplied. */
  readonly label = input('');
  readonly variant = input<DsButtonVariant>('solid');
  readonly tone = input<DsButtonTone>('primary');
  readonly disabled = input(false);

  /** Emitted on activation. Named for intent, not Material's `click`. */
  readonly pressed = output<void>();

  protected readonly matAppearance = computed(() => VARIANT_TO_MAT[this.variant()]);
}
