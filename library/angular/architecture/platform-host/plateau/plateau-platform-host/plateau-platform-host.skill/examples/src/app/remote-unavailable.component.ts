import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-remote-unavailable',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<p role="alert">This section is temporarily unavailable. The rest of the platform still works.</p>`,
})
export class RemoteUnavailableComponent {}
