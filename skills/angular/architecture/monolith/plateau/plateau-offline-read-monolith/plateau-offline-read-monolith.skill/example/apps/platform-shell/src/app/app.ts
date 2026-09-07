import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { Store } from '@ngrx/store';
import { OfflineBannerComponent } from '@org/shared-ui';
import { selectIsOnline } from '@org/shared-state';

@Component({
  imports: [RouterModule, OfflineBannerComponent],
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class App {
  protected readonly title = 'Orders';
  // The shell owns the store wiring; OfflineBannerComponent stays presentational.
  protected readonly isOnline = inject(Store).selectSignal(selectIsOnline);
}
