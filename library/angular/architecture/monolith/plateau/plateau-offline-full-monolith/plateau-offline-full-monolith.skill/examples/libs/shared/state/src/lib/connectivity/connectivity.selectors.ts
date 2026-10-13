// Public selector surface for the connectivity slice. Feature code imports
// `selectIsOnline` from here (or the lib barrel) and never reads
// `navigator.onLine` directly.
export { selectIsOnline } from './connectivity.reducer';
