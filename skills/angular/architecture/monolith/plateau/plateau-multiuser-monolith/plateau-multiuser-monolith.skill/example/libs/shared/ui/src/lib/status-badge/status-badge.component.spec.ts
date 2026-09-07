import { render, screen } from '@testing-library/angular';
import { StatusBadgeComponent } from './status-badge.component';

describe('StatusBadgeComponent', () => {
  it('renders the label and reflects the status on a data attribute', async () => {
    await render(StatusBadgeComponent, { inputs: { status: 'creating', label: 'Creating…' } });
    const el = screen.getByText('Creating…');
    expect(el).toHaveAttribute('data-status', 'creating');
  });
});
