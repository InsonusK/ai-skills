import '@angular/compiler';
import 'fake-indexeddb/auto';
import '@analogjs/vitest-angular/setup-snapshots';
import { setupTestBed } from '@analogjs/vitest-angular/setup-testbed';

setupTestBed({ zoneless: false });

import '@testing-library/jest-dom/vitest';
