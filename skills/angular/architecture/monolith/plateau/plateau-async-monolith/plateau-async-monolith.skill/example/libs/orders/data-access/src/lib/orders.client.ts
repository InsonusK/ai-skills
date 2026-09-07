import { inject, Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { BaseHttpService } from '@org/shared-http-core';
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
      catchError((e) => this.toDomain(e)),
    );
  }

  add(input: AddOrderInput): Observable<Order> {
    return this.http.post<OrderDto>('/orders', inputToDto(input)).pipe(
      map(dtoToModel),
      catchError((e) => this.toDomain(e, input.product)),
    );
  }

  private toDomain(e: unknown, product = ''): Observable<never> {
    if (e instanceof HttpErrorResponse) {
      if (e.status === 409) return throwError(() => new OrdersConflictError(product));
      return throwError(() => new OrdersTransportError(e.status));
    }
    return throwError(() => e);
  }
}
