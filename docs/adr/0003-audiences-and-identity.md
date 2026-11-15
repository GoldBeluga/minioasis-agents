# ADR 0003: One API for staff and public audiences, with identity verified by the API

- **Status:** Accepted when the maintainer merges the pull request that adds this file
- **Date:** 2026-11-15
- **Deciders:** Maintainer

## Context

Minioasis began as an API for library staff. The maintainer wants it to serve two audiences: staff through an internal dashboard, and the public through a progressive web application. The staff MVP comes first, with a single staff member. The public application, including patron self-service, comes later.

The intended deployment is:

- the API on a private host, not reachable from the internet;
- the staff dashboard at its own hostname, behind a Cloudflare Tunnel protected by Cloudflare Access;
- the public site at a separate public hostname, without Cloudflare Access;
- each site reaching the API through its own server.

Koha remains the authority for library records and is used as an external data service.

## Decision

1. **One API serves every audience.** Every endpoint declares exactly one audience: anonymous, patron, or staff. Endpoints deny every caller unless they have an explicit policy.
2. **The API verifies identity itself.** It never trusts network location, loopback, a tunnel, a proxy, or claims from a frontend server. It accepts only cryptographically verified tokens.
3. **Frontends never forward requests wholesale.** A frontend server calls specific API endpoints, which makes it a backend-for-frontend, or proxies only an allowlisted route prefix.
4. **Staff MVP:** Cloudflare Access is the single source of staff identity. The API verifies the Access token (`Cf-Access-Jwt-Assertion`) for the dashboard's Access application on every staff request. Every verified staff identity has full permissions. Spec 002 defines the mechanics.
5. **Later:** Supabase adds per-staff sign-in alongside Cloudflare Access, to support several staff, and serves as the public identity provider. Each step needs its own approved spec.

## Alternatives considered

| Alternative | Why not |
| --- | --- |
| Trust requests because they arrive on loopback or the LAN | Every frontend server shares the same network position, so the API cannot tell them apart. A compromised public server, another process on the host, a device on the LAN, or a tunnel misconfiguration would get full Koha access. |
| Trust the frontend server's assertion of who the user is | A compromised frontend server could impersonate anyone, including staff. |
| Separate API host projects for staff and public now | No public endpoints exist yet. When the public spec is written, revisit this, mainly to give public traffic a lower-privilege Koha client. |
| Cloudflare Access for public users | Each public user would count as a paid Access user. Access would also interfere with offline use, installing the app, link sharing and search indexing, while adding no security, since anyone can pass a public policy. |
| Staff sign-in with Koha credentials | Koha's password validation requires the service account to hold broad patron-management permission, which weakens least privilege. |
| Supabase sign-in for staff in the MVP | One staff member does not justify the session and MFA work. Supabase is added when there are several staff. |

## Consequences

- Bypassing Cloudflare gains an attacker nothing at the API, because they cannot forge an Access token.
- The API holds the Koha service credentials, so it is the most valuable target. It must not be reachable from the internet or from untrusted LAN devices.
- A compromised dashboard server can replay the tokens of staff who are using it until they expire. The Access session length bounds that window.
- Authentication failures need a public response (401 and 403), which spec 002 must design.
- When Supabase is added for staff, its sign-in must be independent of the Access sign-in method, for example authenticator-app MFA. Otherwise both checks depend on one account, such as an email inbox that can reset passwords.
- The public specs must decide how a Supabase account is linked to a Koha patron. They must also decide where the server-only patron identifier is stored (never in user-editable metadata), and how anonymous and patron endpoints are rate limited.
