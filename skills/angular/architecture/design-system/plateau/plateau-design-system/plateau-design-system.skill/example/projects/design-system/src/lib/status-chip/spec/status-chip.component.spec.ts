import { render, screen } from '@testing-library/angular';
import { DsStatusChipComponent } from '../status-chip.component';

describe('DsStatusChipComponent', () => {
  describe('rendering', () => {
    it('shows the default label for a status', async () => {
      await render(DsStatusChipComponent, { inputs: { status: 'in-progress' } });

      expect(screen.getByRole('status')).toHaveTextContent('In progress');
    });

    it('lets the caller override the label', async () => {
      await render(DsStatusChipComponent, { inputs: { status: 'stop', label: 'Halted' } });

      expect(screen.getByRole('status')).toHaveTextContent('Halted');
    });

    it('applies a status-specific class so the --ds-color-status-* token resolves', async () => {
      await render(DsStatusChipComponent, { inputs: { status: 'start' } });

      expect(screen.getByTestId('ds-status-chip')).toHaveClass('ds-status-chip--start');
    });
  });
});
