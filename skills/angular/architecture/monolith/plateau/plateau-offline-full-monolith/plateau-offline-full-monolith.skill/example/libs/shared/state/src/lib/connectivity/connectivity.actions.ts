import { createActionGroup, emptyProps } from '@ngrx/store';

export const ConnectivityActions = createActionGroup({
  source: 'Connectivity',
  events: {
    'Browser Reported Online': emptyProps(),
    'Browser Reported Offline': emptyProps(),
    'Health Check Succeeded': emptyProps(),
    'Health Check Failed': emptyProps(),
  },
});
