import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * Presentational offline indicator. Mounted once at the shell, which feeds it
 * `isOnline` from the shared `connectivity` slice (`selectIsOnline`) — the
 * component itself never touches the store or `navigator.onLine`, so it stays
 * a pure, boundary-clean `shared-ui` component.
 */
@Component({
  selector: 'ui-offline-banner',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (!isOnline()) {
      <div role="status" class="offline-banner">
        You're offline. Showing the latest available data.
      </div>
    }
  `,
  styles: `
    .offline-banner {
      padding: 0.5rem 1rem;
      background: #b45309;
      color: #fff;
      font-size: 0.875rem;
      text-align: center;
    }
  `,
})
export class OfflineBannerComponent {
  readonly isOnline = input.required<boolean>();
}
