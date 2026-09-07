/*
 * Public API Surface of design-system
 *
 * No Angular Material selector, input, or type is re-exported here — Material is
 * an internal implementation detail of the ds-* components.
 */

export { DsButtonComponent } from './lib/button/button.component';
export type { DsButtonVariant, DsButtonTone } from './lib/button/button.component';

export { DsStatusChipComponent } from './lib/status-chip/status-chip.component';
export type { DsStatus } from './lib/status-chip/status-chip.component';

// Multi-tenant theming — the valid-tenant contract (styles ship as a package asset)
export { DS_TENANTS } from './lib/tenants';
export type { DsTenant } from './lib/tenants';
