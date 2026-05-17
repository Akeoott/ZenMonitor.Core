# Security Policy

We take the security of ZenMonitor seriously. This document outlines how to report security issues, what to expect, and which versions receive security patches.

---

## About Security

ZenMonitor is a system monitor that can run with elevated privileges (root on Linux, admin on Windows). The following areas are considered in scope for security reports:

- **Privilege escalation** — any way the app could be used to gain or leak elevated privileges
- **Command injection** — vulnerabilities in parsing `/proc`/`sys` files or configuration inputs that could lead to arbitrary code execution
- **Data exposure** — system telemetry or logs unintentionally leaking sensitive user data
- **Dependency vulnerabilities** — CVEs in dependencies such as System.IO.Abstractions or the .NET runtime itself

If you are unsure whether something qualifies, stay on the side of caution and report it.

---

## Supported Versions

Security patches are applied to the `main` branch. Releases are snapshots of `main` at a specific point in time and may receive a patch release if the severity warrants it.

| Version / Branch | Supported |
|------------------|-----------|
| `main` | ✅ Receives security patches |
| Tagged releases | 🔄 Patched via a new release from `main` |
| Older releases | ❌ No longer supported |

---

## Reporting a Security Issue

We appreciate your efforts to disclose findings responsibly. Please choose the appropriate channel below.

### Non-Sensitive Issues (Public)

If the issue does **not** expose sensitive data or reveal exploitable details, open a public issue using the [Security Report template](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/ISSUE_TEMPLATE/6-security-report.md). Follow the template instructions carefully.

### Sensitive Issues (Private)

If your report could disclose vulnerabilities, sensitive data, or enough detail for someone to reproduce an exploit, **do not** open a public issue. Instead:

1. Send an email to **akeoot@pm.me**
2. Include **"SECURITY"** in the subject line
3. Provide as much detail as possible: steps to reproduce, affected versions, potential impact, and any proof-of-concept code (minimised)

---

## Response Commitment

We will do our best to respond quickly and keep you informed:

1. **Acknowledgement** — we will confirm receipt within 48–72 hours
2. **Assessment** — we will triage the issue and determine severity (CVSS score) within 5 business days
3. **Fix timeline** — after assessment, we will communicate an expected timeline. This depends on severity:
   - **Critical / High** (CVSS 7.0–10.0) → prioritised immediately, patch release may follow
   - **Medium / Low** (CVSS 0.1–6.9) → addressed in the next regular release cycle

---

## Disclosure Policy

We follow a coordinated disclosure process:

- We will work with the reporter to agree on a public disclosure date *after* a fix has been released
- Reporters may be credited in release notes and GitHub Security Advisories (with consent)
- Anonymous reporting is respected — if you prefer not to be credited, just let us know

---

*Thank you for helping keep ZenMonitor and its users safe.*