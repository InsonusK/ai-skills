import { Component } from '@angular/core';
import { of } from 'rxjs';
import { provideState, provideStore } from '@ngrx/store';
import { OrderFormComponent, OrdersStore } from '@org/orders-feature';
import { OrdersFacade } from '@org/orders-data-access';
import { MutationQueueService } from '@org/shared-offline-sync';
import { authFeature } from '@org/shared-state';

// Static, hardcoded data only — no real Facade, HTTP, or backend-wired store.
const stubFacade = { list: async () => [], addOrder: async () => { throw new Error('preview'); } };
const stubQueue = { pendingForFeature$: () => of([]), enqueue: async () => undefined };

@Component({
  selector: 'preview-order-form',
  standalone: true,
  imports: [OrderFormComponent],
  providers: [
    OrdersStore,
    { provide: OrdersFacade, useValue: stubFacade },
    { provide: MutationQueueService, useValue: stubQueue },
    // real auth slice, default state (permissions: []) — *hasPermission hides the button
    provideStore(),
    provideState(authFeature),
  ],
  template: '<orders-order-form />',
})
export class OrderFormPreview {}
