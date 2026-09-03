import { render, screen } from '@testing-library/angular';
import { OfflineBannerComponent } from './offline-banner.component';

describe('OfflineBannerComponent', () => {
  it('is not rendered while online', async () => {
    await render(OfflineBannerComponent, { inputs: { isOnline: true } });
    expect(screen.queryByRole('status')).toBeNull();
  });

  it('shows the offline message while offline', async () => {
    await render(OfflineBannerComponent, { inputs: { isOnline: false } });
    expect(screen.getByRole('status')).toHaveTextContent(/offline/i);
  });
});
