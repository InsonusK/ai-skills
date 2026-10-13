/*
 * Every tenant id the design system ships a palette for. Each entry has a matching
 * `styles/tenants/_<id>.scss` file and a `:root[data-tenant='<id>']` rule.
 * With no `data-tenant` attribute set, the base brand palette (styles/theme.scss) applies.
 *
 * A consuming application resolves the active tenant (from a claim / API / subdomain)
 * and sets `document.documentElement.dataset.tenant` to one of these — ideally
 * server-rendered into index.html to avoid a flash.
 */
export const DS_TENANTS = ['acme', 'globex'] as const;

export type DsTenant = (typeof DS_TENANTS)[number];
