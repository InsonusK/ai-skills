import { TestBed } from '@angular/core/testing';
import { expect, it } from 'vitest';
import { App } from '../app';
it('should render zero when the component opens', async () => {
  const fixture = TestBed.createComponent(App);
  await fixture.whenStable();
  expect(fixture.nativeElement.querySelector('output').textContent).toBe('0');
});
it('should render one when the user clicks increment', async () => {
  const fixture = TestBed.createComponent(App);
  await fixture.whenStable();
  fixture.nativeElement.querySelector('button').click();
  await fixture.whenStable();
  expect(fixture.nativeElement.querySelector('output').textContent).toBe('1');
});
