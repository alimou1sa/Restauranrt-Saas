cd frontend# RestaurantSaaS frontend (milestone 1)

React 18 + TypeScript + Vite + React Router + Axios. Login -> organization selection -> dashboard shell.

## Run

1. **Apply the backend patch first**: `../backend-patch/BACKEND_PATCH.md` (adds `GET /api/auth/me`). Restart the API.
2. Start the API (`https` profile -> `https://localhost:7107`).
3. In this folder:
   ```bash
   cp .env.example .env     # optional; defaults work with the https profile
   npm install
   npm run dev              # http://localhost:5173
   ```
   `npm run build` type-checks and builds.

In dev, Vite proxies `/api` to the backend, so the backend CORS list (which only allows the API's own origins)
does not need to change. For a deployed build set `VITE_API_BASE_URL` to the full API URL and add the frontend
origin to the backend CORS policy.

## Structure

```
src/
  api/        axios instance (client.ts) + one file per backend area; no axios use outside this folder
  auth/       AuthContext, token storage, single-flight refresh, permission codes, <Can/>
  config/     env, sidebar navigation definition
  components/ Icon, Toast, Spinner/ErrorState/EmptyState
  features/   dashboard/ (data hook, StatCard, RecentOrders)
  hooks/      useAsync (loading/error/data)
  layouts/    AppLayout, Sidebar, Topbar
  pages/      Login, SelectOrganization, Dashboard, NotFound
  router/     routes + guards (RequireAuth, PublicOnly, RequirePreAuth, RequirePermission)
  types/      API DTO interfaces
  utils/      errors, format (UTC-safe dates), jwt (exp only)
```

## Auth flow (as implemented by the backend)

1. `POST /api/auth/login` -> `{ token, tokenType: "org_selection", organizations[] }`. The token is a **PreAuth token** (5 min). It is kept in
   `sessionStorage`, only sent to `GET /api/auth` and `POST /api/auth/select-organization`, and never used as an access token.
2. `POST /api/auth/select-organization { organizationId }` -> `{ token, refreshToken, ... }` (access token 15 min, refresh 30 days). One membership per
   organization, so the organization id is enough; the branch comes from the membership (`branchId` null = all branches).
3. `GET /api/auth/me` (**backend patch**) -> profile + organization + branch + effective permission codes.
4. 401 on an access-token call -> one shared `POST /api/auth/refresh` (the backend rotates refresh tokens and revokes everything on reuse, so
   concurrent refreshes are never allowed) -> original request retried once. Refresh rejected -> session cleared, user sent to login.
5. Logout -> `POST /api/auth/logout { refreshToken }` (best effort), then local state is always cleared.

## Backend gaps

| # | Gap | Impact / current handling |
|---|-----|---------------------------|
| 1 | No endpoint exposes the user's permissions (JWT has none) | **Needs `GET /api/auth/me`** - patch provided |
| 2 | No dashboard/analytics endpoints | Orders are aggregated in the browser from `GET /api/Orders` (no paging/date filter) - fine for small data, replace with a summary endpoint |
| 3 | Products/inventory only exist per branch | Organization-wide users: one request per active branch (+ `GET /api/Branches`, needs `branch.read`) |
| 4 | No currency field anywhere | Amounts shown as plain numbers |
| 5 | CORS allow-list only has the API's own origins | Worked around with the Vite proxy; add the frontend origin before deploying |
| 6 | Auth hardening outside this milestone | `GET /api/users`, `GET /api/users/{id}`, `GET /api/organizations` have no permission check; `Plans` and `Permissions` write routes have none either; `ChangeMyPassword`/`DeleteOrganization` too |
| 7 | Refresh token is returned in JSON | It lives in `localStorage` (XSS-exposed). An httpOnly cookie would need a backend change |

## Assumptions

- "Today" = the browser's local day; timestamps are UTC (the API omits the `Z`, handled in `utils/format.ts`).
- "Today's orders" excludes Canceled; "Today's revenue" = sum of `totalAmount` of Completed orders created today; "Pending" = status `Pending`.
- A user with a single organization skips the selection screen.
- No mock data is used: every dashboard number comes from a real endpoint.

## Known limitations

- Two browser tabs refreshing at the very same moment could trip the backend's refresh-reuse protection (no cross-tab lock yet).
- Sidebar entries for Orders, Menu, Products, Inventory, Branches, Users, Roles and Settings are permission-filtered placeholders marked "Soon".
- Not built/type-checked in the authoring environment (no npm registry access) - run `npm install && npm run build` first.

## Next milestone suggestion

Orders (list with filters, detail, status updates via `PATCH /api/Orders/orders/{id}/status`), because it's the core daily workflow and
exercises the permission layer (`order.read` vs `order.manage`) and the branch-scoped API. Raise the backend summary endpoint (gap 2) at the same time.
