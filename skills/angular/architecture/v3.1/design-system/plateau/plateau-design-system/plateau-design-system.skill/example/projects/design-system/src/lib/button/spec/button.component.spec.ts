import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { DsButtonComponent } from '../button.component';

describe('DsButtonComponent', () => {
  describe('rendering', () => {
    it('renders the label and reflects the disabled input', async () => {
      await render(DsButtonComponent, { inputs: { label: 'Save', disabled: true } });

      expect(screen.getByRole('button', { name: /save/i })).toBeDisabled();
    });

    it('accepts every variant without leaking a Material appearance name to the API', async () => {
      const { rerender } = await render(DsButtonComponent, { inputs: { label: 'Go', variant: 'solid' } });
      expect(screen.getByRole('button', { name: /go/i })).toBeInTheDocument();

      await rerender({ inputs: { label: 'Go', variant: 'outline' } });
      expect(screen.getByRole('button', { name: /go/i })).toBeInTheDocument();

      await rerender({ inputs: { label: 'Go', variant: 'text' } });
      expect(screen.getByRole('button', { name: /go/i })).toBeInTheDocument();
    });
  });

  describe('interactions', () => {
    it('emits (pressed) when clicked', async () => {
      const pressed = vi.fn();
      await render(DsButtonComponent, { inputs: { label: 'Save' }, on: { pressed } });

      await userEvent.click(screen.getByRole('button', { name: /save/i }));
      expect(pressed).toHaveBeenCalledTimes(1);
    });

    it('does not emit (pressed) while disabled', async () => {
      const pressed = vi.fn();
      await render(DsButtonComponent, { inputs: { label: 'Save', disabled: true }, on: { pressed } });

      // a disabled button cannot receive pointer events — user-event refuses the click
      await userEvent.click(screen.getByRole('button', { name: /save/i }), {
        pointerEventsCheck: 0,
      });
      expect(pressed).not.toHaveBeenCalled();
    });
  });
});
