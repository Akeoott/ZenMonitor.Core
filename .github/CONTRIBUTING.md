# Contributing to ZenMonitor.Core

First off, thanks for taking the time to contribute!

When contributing to this repository, please first discuss the change you wish to make via an issue, discussion, email, or any other method with the owners or contributors of this repository before making a change and opening a pull request about it.

Please note we have a [Code of Conduct](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/CODE_OF_CONDUCT.md).<br>
Please follow it in all your interactions with the project.

---

## Table of Contents

- [Getting Started with Development](#getting-started-with-development)
- [Code Style & Conventions](#code-style--conventions)
- [Unit Tests & CI](#unit-tests--ci)
- [Commit Message Standards](#commit-message-standards)
- [Opening Issues](#opening-issues)
- [Making Pull Requests](#making-pull-requests)

---

## Getting Started with Development

<details>
<summary>Prerequisites and build instructions</summary>

> [!NOTE]
> The project structure for this library is still being defined.<br>
> This section will be expanded once the structure is settled.

### Prerequisites

- [.NET SDK 10.0.203](https://dotnet.microsoft.com/en-us/download/dotnet/10.0) (pinned in [`global.json`](https://github.com/Akeoott/ZenMonitor.Core/blob/main/global.json))
- A terminal and your editor of choice (VS Code, Rider, etc.)

### Build & Verify

```bash
dotnet restore
dotnet build

# Open the debug interface which provides you with all live values,
# gathered from your computer.
dotnet run --project tests/ZenMonitor.Core.Debug/ -- [logLevel]
# logLevel can be one of the following values:
# trace/debug/info/warning/error/critical


# Platform specific tests may fail,
# depending on what platform you're on.
dotnet test
```

Most structural details are in the [README.md](https://github.com/Akeoott/ZenMonitor.Core?tab=readme-ov-file) at the moment.

</details>

---

## Code Style & Conventions

<details>
<summary>Formatting, naming, and project conventions</summary>

### Editor Config

An [`.editorconfig`](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.editorconfig) file is at the repo root. Most editors pick it up automatically. If yours doesn't, configure it to match the project's indentation and style rules.

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Public classes, methods, properties | `PascalCase` | `CpuService`, `Update()` |
| Private fields | `_camelCase` | `_semaphore`, `_logger` |
| Method parameters, local variables | `camelCase` | `updateInterval` |
| Interfaces | `I` prefix + `PascalCase` | `ICpu`, `ISystem` |

### File-Scoped Namespaces

Use file-scoped namespaces (no braces):

```csharp
namespace ZenMonitor.Core.Interfaces;
```

### Code Quality

- No warnings on build. Run `dotnet build` before pushing.
- Keep interfaces focused — each interface should have a single responsibility.
- Use the `Update()` pattern: each service exposes a `void Update()` method that refreshes its internal snapshot.
- Pay attention to the static code analysis from [CodeFactor](https://www.codefactor.io/repository/github/akeoott/ZenMonitor.Core).
- Keep an eye on unit test coverage from [CodeCov](https://app.codecov.io/gh/Akeoott/ZenMonitor.Core) and make sure it does not drop significantly.

</details>

---

## Unit Tests & CI

<details>
<summary>Test framework, platform filters, coverage, and CI workflow</summary>

### Test Framework

The project uses **xUnit**. Tests are organized under `ZenMonitor.Core.Tests/`.

### Running Tests Locally

```bash
dotnet test # All tests

# All Linux specific tests
dotnet test --filter "Platform=Linux"

# All Windows specific tests
dotnet test --filter "Platform=Windows"
```

Platform specific tests may fail,
depending on what platform you're on.

Coverage configuration is in [`coverlet.runsettings`](https://github.com/Akeoott/ZenMonitor.Core/blob/main/coverlet.runsettings). Output is written to `./coverage/`.

### CI Workflow

The CI workflow (`.github/workflows/tests.yml`) runs on every push and pull request to `main`:

1. **Setup .NET** — installs SDK 10.0.203
2. **Restore** — `dotnet restore`
3. **Build** — `dotnet build --no-restore`
4. **Test** — runs tests with coverage
5. **Coverage** — uploads results to [Codecov](https://codecov.io/gh/Akeoott/ZenMonitor.Core)

Patch coverage is set to `informational: true` (see `.github/codecov.yml`), so a coverage drop won't block a PR — but aim to cover new code. Maintainers might tell you to add more Unit Tests.

> [!NOTE]
> In tests, we want deterministic data,<br>
> checking for exact values instead of simple null checks etc.<br>
> When writing tests, please also include non "happy path" tests.
> This means testing edge cases and not just intended flow.

</details>

---

## Commit Message Standards

<details>
<summary>Conventional Commits, scopes, and signed commits</summary>

### Conventional Commits

We follow [Conventional Commits](https://www.conventionalcommits.org/) for all commit messages. This keeps the history readable and enables automated changelog generation.

```
<type>(<scope>): <description>

[optional body]
[optional footer]
```

### Types

| Type | Usage |
|------|-------|
| `feat` | A new feature |
| `fix` | A bug fix |
| `chore` | Maintenance, tooling, config changes |
| `docs` | Documentation changes |
| `test` | Adding or updating tests |
| `refactor` | Code restructuring without feature or fix |
| `style` | Formatting, linting, code style (not CSS) |
| `ci` | CI/CD workflow changes |

### Scopes

Scopes indicate which part of the project the commit touches. Be as specific as makes sense:

| Scope | When to use |
|-------|-------------|
| `project` | Global changes (e.g. `style(project): apply editorconfig across all files`) |
| `interfaces` | Hardware abstraction interfaces |
| `services` | Platform service implementations |
| `models` | Data models and records |
| `api` | Public API surface |

#### Examples

```
feat(interfaces): add ICpu interface with temperature and frequency getters

chore(project): add launch configs
- add `.vscode/launch.json` with debug profiles for cli, gui, debug, and trace modes
```

### Signed Commits

All commits **must** be signed (GPG or SSH). Unsigned commits will not be accepted in pull requests.

#### Minimal Git Setup

If you haven't configured Git for signing yet:

```bash
# Set your identity (must match your commit signature)
git config --global user.name "Your Name"
git config --global user.email "your-email@example.com"

# Get your secret key id
gpg --list-secret-keys

# Tell Git which signing key to use
git config --global user.signingkey <key-id>

# Enable signing for all commits
git config --global commit.gpgsign true
```

> [!IMPORTANT]
> For full guides on generating GPG keys and linking them to GitHub, see:
> - [GitHub: Managing commit signature verification](https://docs.github.com/en/authentication/managing-commit-signature-verification)
> - [Git Tools — Signing Your Work](https://git-scm.com/book/en/v2/Git-Tools-Signing-Your-Work)

### Atomic Commits

Each commit should represent **one logical change**. If a commit includes a bug fix, a refactor, and a documentation update, split it into multiple commits.

</details>

---

## Opening Issues

<details>
<summary>Issue templates and security disclosures</summary>

### Issue Templates

We provide templates for most scenarios. Select the one that best fits your issue when creating it in the [issue tracker](https://github.com/Akeoott/ZenMonitor.Core/issues/new/choose).

| Template | Use when... |
|----------|-------------|
| [Bug Report](ISSUE_TEMPLATE/1-bug-report.md) | Something doesn't work as expected |
| [Failing Test](ISSUE_TEMPLATE/2-failing-test.md) | A test is failing or missing |
| [Docs Issue](ISSUE_TEMPLATE/3-docs-issue.md) | Documentation is wrong or missing |
| [Feature Request](ISSUE_TEMPLATE/4-feature-request.md) | You want a new capability |
| [Enhancement Request](ISSUE_TEMPLATE/5-enhancement-request.md) | You want to improve existing behaviour |
| [Security Report](ISSUE_TEMPLATE/6-security-report.md) | You found a vulnerability |
| [Question / Support](ISSUE_TEMPLATE/7-question-support.md) | You need help or clarification |

Read the template instructions carefully before submitting.

### Security Disclosures

If your report could disclose a vulnerability or sensitive information, **do not** open a public issue. Instead, email [akeoot@pm.me](mailto:akeoot@pm.me) with **"SECURITY"** in the subject line. See [SECURITY.md](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/SECURITY.md) for details.

</details>

---

## Making Pull Requests

<details>
<summary>PR checklist, branch naming, review process, and merge strategy</summary>

### Before You Open

- [ ] All tests pass (`dotnet test`)
- [ ] Build completes without warnings (`dotnet build`)
- [ ] New code includes tests (if applicable)
- [ ] Commits are signed
- [ ] Documentation is updated (if applicable)
- [ ] Branch is up to date with `main`

### Branch Naming

Use a prefix that matches the type of change, followed by a short descriptor:

```
feature/add-cpu-interface
fix/service-null-ref
docs/improve-contributing-guide
chore/update-dependencies
```

Prefixes: `feature/`, `fix/`, `docs/`, `chore/`, `refactor/`, `test/`

### Pull Request Process

1. Ensure your branch is up to date with the `origin/main`:
    ```bash
    # 1. Update your local main branch
    git checkout main
    git pull origin main

    # 2. Switch back to your feature branch
    git checkout your-feature-branch

    # 3. Merge or rebase main into your feature branch
    git merge main # Use when others are on your branch.
    git rebase main # Use when your the only one on your branch.
    ```
2. Open the PR against the `main` branch.
3. Fill out the [pull request template](https://github.com/Akeoott/ZenMonitor.Core/blob/main/.github/pull_request_template.md).
4. Ensure all CI checks pass.
5. Request a review from a project maintainer.

### Review Process

- Pull requests require approval from at least **2 project maintainers**.
- Repository **owners** may merge at their discretion after a thorough review. This ensures important changes aren't blocked when maintainers are unavailable.
- All reviews are strict — changes that don't meet the standards above will be asked to improve before merging.

### Merge Strategy

**Squash merge** is preferred. This keeps the commit history clean by collapsing multiple commits into one logical commit per PR. The squashed commit message should follow the Conventional Commits format.

Exceptions for squash merges are when there are changes that go beyond the scope of the PR.

</details>

---

***Thank you for contributing to ZenMonitor.Core!***