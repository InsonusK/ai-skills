import { render, screen } from '@testing-library/angular';
import { PendingSyncIndicatorComponent } from './pending-sync-indicator.component';

describe('PendingSyncIndicatorComponent', () => {
  it('renders nothing when nothing is pending', async () => {
    await render(PendingSyncIndicatorComponent, { inputs: { count: 0 } });
    expect(screen.queryByRole('status')).toBeNull();
  });

  it('renders a singular message for one pending action', async () => {
    await render(PendingSyncIndicatorComponent, { inputs: { count: 1 } });
    expect(screen.getByRole('status')).toHaveTextContent('1 action waiting to sync');
  });

  it('renders a plural message for several pending actions', async () => {
    await render(PendingSyncIndicatorComponent, { inputs: { count: 3 } });
    expect(screen.getByRole('status')).toHaveTextContent('3 actions waiting to sync');
  });
});
