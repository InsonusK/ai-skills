---
name: connectivity-detection
description: How the application determines whether it is currently online, for UI purposes (offline banners, disabling actions) and as the future basis for triggering sync
problem: navigator.onLine only reflects whether a network interface is active, not whether the backend is actually reachable, which can misrepresent the application's real connectivity state
decision: Combine navigator.onLine events with a periodic lightweight health-check request, exposed as a global connectivity slice in shared-state
---

# Problem

The application needs a reliable, single source of truth for "are we online" — used to show an offline banner, decide whether to attempt a request at all, and (in the future "Синхронизация offline-данных" solution) trigger a retry of queued mutations. The browser's `navigator.onLine` property and its `online`/`offline` events only reflect whether a network interface is active (e.g. Wi-Fi connected), not whether the backend is actually reachable — a device can report `navigator.onLine === true` while sitting behind a captive portal or while the backend itself is down.

# Selected variant

**Selected variant:** [[#navigator.onLine events + periodic health-check]]

The application listens to `navigator.onLine`/`online`/`offline` events as an immediate, low-cost first signal, and additionally performs a lightweight periodic `HEAD` request to a backend health endpoint to confirm actual reachability. The combined result is exposed as a `connectivity` slice in `libs/shared/state` (global/cross-cutting state, per the "State management" solution's tiering), read by any feature that needs to react to connectivity (disabling a submit button, showing a banner).

# Searched variants

## navigator.onLine events + periodic health-check

### Description

`window.addEventListener('online'/'offline', ...)` provides an immediate signal on network interface changes. A periodic (e.g. every N seconds, backed off further when already known offline) `HEAD` request to a lightweight backend health endpoint confirms actual reachability, correcting for cases where the network interface is up but the backend is not reachable.

### Benefits

- Combines the immediacy of browser events (near-instant reaction to the network interface changing) with the accuracy of an actual reachability check
- Backing off the health-check frequency while already offline avoids hammering the network with pointless requests during a known outage
- A single, accurate `connectivity` signal is more trustworthy for any future logic (offline banner, disabling actions, and eventually triggering the sync queue in the future solution) than `navigator.onLine` alone

### Costs

- Introduces a small, recurring network request (the health-check) that would not otherwise exist
- Slightly more implementation complexity than reading a single browser property

## navigator.onLine events only

### Description

Rely solely on the browser's own `online`/`offline` events and the `navigator.onLine` property, with no additional reachability check.

### Benefits

- Zero additional network requests
- Simplest possible implementation — no polling logic to write or tune

### Costs

- Materially less accurate: a user can be shown as "online" while the backend itself is unreachable (captive portal, backend outage, restrictive firewall), leading to the application attempting requests that predictably fail, and delaying the offline banner from appearing on a set of real-world failure modes
