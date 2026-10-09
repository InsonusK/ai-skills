import { TestBed } from '@angular/core/testing';
import { expect, it } from 'vitest';
import { App } from '../app';

async function open() {
  const fixture = TestBed.createComponent(App);
  await fixture.whenStable();
  return fixture;
}

async function submit(fixture: Awaited<ReturnType<typeof open>>, value: string) {
  const input = fixture.nativeElement.querySelector('input') as HTMLInputElement;
  input.value = value;
  input.dispatchEvent(new Event('input', { bubbles: true }));
  fixture.nativeElement.querySelector('form').dispatchEvent(new Event('submit', { bubbles: true, cancelable: true }));
  await fixture.whenStable();
}

it('should show the form when the page opens', async () => {
  const fixture = await open();
  expect(fixture.nativeElement.querySelector('label').textContent).toBe('URL');
  expect(fixture.nativeElement.querySelector('button').textContent).toBe('Check URL');
  expect(fixture.nativeElement.querySelector('section')).toBeNull();
});
it('should render the normalized URL when the form is submitted', async () => {
  const fixture = await open();
  await submit(fixture, '  HTTPS://EXAMPLE.COM/path  ');
  expect(fixture.nativeElement.querySelector('output').textContent).toBe('https://example.com/path');
  expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
});
it('should show the domain error when an unsupported scheme is submitted', async () => {
  const fixture = await open();
  await submit(fixture, 'ftp://example.com');
  expect(fixture.nativeElement.querySelector('[role="alert"]').textContent).toContain('UNSUPPORTED_SCHEME');
  expect(fixture.nativeElement.querySelector('output')).toBeNull();
});
it('should replace the error when a valid URL is resubmitted', async () => {
  const fixture = await open();
  await submit(fixture, 'ftp://example.com');
  await submit(fixture, 'https://example.com');
  expect(fixture.nativeElement.querySelector('[role="alert"]')).toBeNull();
  expect(fixture.nativeElement.querySelector('output').textContent).toBe('https://example.com');
});
