import { Component } from '@angular/core';
import { render, screen } from '@testing-library/angular';
import { provideMockStore } from '@ngrx/store/testing';
import { selectPermissions } from '@org/shared-state';
import { HasPermissionDirective } from './has-permission.directive';

@Component({
  standalone: true,
  imports: [HasPermissionDirective],
  template: `<button *hasPermission="'orders.delete'">Delete</button>`,
})
class Host {}

const renderWith = (permissions: string[]) =>
  render(Host, {
    providers: [provideMockStore({ selectors: [{ selector: selectPermissions, value: permissions }] })],
  });

describe('HasPermissionDirective', () => {
  it('renders the element when the user has the permission', async () => {
    await renderWith(['orders.delete', 'orders.view']);
    expect(screen.getByRole('button', { name: 'Delete' })).toBeInTheDocument();
  });

  it('does not render the element when the user lacks the permission', async () => {
    await renderWith(['orders.view']);
    expect(screen.queryByRole('button', { name: 'Delete' })).toBeNull();
  });
});
