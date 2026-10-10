import { inject, Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { BaseHttpService, OfflineTransportError } from '@org/shared-http-core';
import { catchError, map, Observable, throwError } from 'rxjs';
import { AddOrderInput, Order } from './orders.model';
import { OrderDto } from './orders.dto';
import { dtoToModel, inputToDto } from './orders.mapper';
import { OrdersConflictError, OrdersTransportError } from './orders.errors';

@Injectable({ providedIn: 'root' })
export class OrdersClient {
  private readonly http = inject(BaseHttpService);

  list(): Observable<Order[]> {
    return this.http.get<OrderDto[]>('/orders').pipe(
      map((dtos) => dtos.map(dtoToModel)),
      catchError((e) => this.toDomain(e, 'list')),
    );
  }

  add(input: AddOrderInput): Observable<Order> {
    return this.http.post<OrderDto>('/orders', inputToDto(input)).pipe(
      map(dtoToModel),
      catchError((e) => this.toDomain(e, 'add', input.product)),
    );
  }

  private toDomain(e: unknown, operation: string, product = ''): Observable<never> {
    if (e instanceof HttpErrorResponse) {
      // status 0 == the request never reached the server (network-level
      // failure). Check this FIRST, before any status-code-specific handling,
      // and throw the shared OfflineTransportError so every feature's callers
      // — and the future sync queue — can catch it uniformly.
      if (e.status === 0) return throwError(() => new OfflineTransportError(operation, { cause: e }));
      if (e.status === 409) return throwError(() => new OrdersConflictError(product));
      return throwError(() => new OrdersTransportError(e.status));
    }
    return throwError(() => e);
  }
}
