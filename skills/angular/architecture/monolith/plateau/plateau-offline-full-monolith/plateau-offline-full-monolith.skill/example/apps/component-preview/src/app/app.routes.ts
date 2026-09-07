import { Route } from '@angular/router';
import { OrderFormPreview } from './previews/order-form.preview';

// One deep-linkable route per meaningfully distinct component state — the Playwright
// visual/a11y specs (in each component library's spec/) navigate here.
export const appRoutes: Route[] = [
  { path: 'order-form/idle', component: OrderFormPreview },
];
