import { Component } from '@angular/core';
import { DsStatusChipComponent } from 'design-system';

@Component({
  selector: 'ds-status-chip-preview',
  imports: [DsStatusChipComponent],
  template: `
    <main style="padding: 24px; display: grid; gap: 24px;">
      <section data-preview="default">
        <h2>Statuses</h2>
        <ds-status-chip status="start" />
        <ds-status-chip status="stop" />
        <ds-status-chip status="in-progress" />
      </section>

      <section data-preview="custom-label">
        <h2>Custom label</h2>
        <ds-status-chip status="in-progress" label="Deploying" />
      </section>
    </main>
  `,
})
export class DsStatusChipPreviewComponent {}
