import { Component } from '@angular/core';
// Imports the published package surface — the preview doubles as a check that the
// public API is enough to use the component with no Material knowledge.
import { DsButtonComponent } from 'design-system';

/**
 * Preview page for DsButtonComponent — one clearly delineated section per
 * meaningfully distinct state. Imported by projects/demo and used as the
 * navigation target for the visual / style-snapshot / a11y specs.
 */
@Component({
  selector: 'ds-button-preview',
  imports: [DsButtonComponent],
  template: `
    <main style="padding: 24px; display: grid; gap: 24px;">
      <section data-preview="default">
        <h2>Default</h2>
        <ds-button label="Save changes" />
      </section>

      <section data-preview="variants">
        <h2>Variants</h2>
        <ds-button label="Solid" variant="solid" />
        <ds-button label="Outline" variant="outline" />
        <ds-button label="Text" variant="text" />
      </section>

      <section data-preview="danger">
        <h2>Danger tone</h2>
        <ds-button label="Delete" tone="danger" />
      </section>

      <section data-preview="disabled">
        <h2>Disabled</h2>
        <ds-button label="Save changes" [disabled]="true" />
      </section>
    </main>
  `,
})
export class DsButtonPreviewComponent {}
