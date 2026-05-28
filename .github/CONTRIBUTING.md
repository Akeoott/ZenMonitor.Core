# Contributing to ZenMonitor.Core

First off, thanks for taking the time to contribute.

When contributing to this repository, please first discuss the change you wish to make via an issue, discussion, email, or any other method with the owners or contributors before making a change and opening a pull request.

Please note we have a [Code of Conduct](https://github.com/Akeoott/ZenMonitor.Core?tab=coc-ov-file). Follow it in all your interactions with the project.

---

## Getting Started with Development

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version specified in `global.json`)
- A terminal and your editor of choice (VS Code, Rider, Visual Studio, etc.)

### Build & Verify

```bash
dotnet restore
dotnet build
dotnet test
```

Run the test suite before pushing any changes.

---

## Code Style & Conventions

### EditorConfig

An `.editorconfig` file is at the repo root. Most editors pick it up automatically. If yours does not, configure it to match the project's indentation and style rules.

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
namespace Example.NameSpace;
```

### Code Quality

- No warnings on build. Run `dotnet build` before pushing.
- Keep interfaces focused – each interface should have a single responsibility.
- Follow existing patterns in the codebase.
- Pay attention to static code analysis and code coverage if configured.

---

## Unit Tests

- The project uses **xUnit**.
- Write tests for new code, including edge cases (non‑happy path).
- Run all tests locally with `dotnet test`.
- If your project defines test categories (e.g., `Platform=Linux`), you can filter them:
  ```bash
  dotnet test --filter "Category=YourCategory"
  ```
- Keep or improve test coverage – avoid significant drops.

---

## Commit Message Standards

### Conventional Commits

We follow [Conventional Commits](https://www.conventionalcommits.org/). This keeps history readable and enables automated changelog generation.

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
| `perf` | Performance improvement without feature or fix |
| `style` | Formatting, linting, code style (not CSS) |
| `ci` | CI/CD workflow changes |

### Scopes

Use a scope that indicates which part of the project the commit touches. Be as specific as makes sense:

| Scope | When to use |
|-------|-------------|
| `core` | Core abstractions or shared logic |
| `api` | Public API surface |
| `services` | Platform or service implementations |
| `models` | Data models and records |
| `readme` | changes in readme |

Examples:

```
feat(api): add IResource interface with usage getters
chore(ci): update dotnet version in workflow
docs(readme): improve quick start guide
```

### Signed Commits

All commits **must** be signed (GPG or SSH). Unsigned commits will not be accepted.

#### Minimal Git Setup

If you have not configured signing yet:

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

---

## Opening Issues

### Issue Templates

We provide templates for most scenarios. Select the one that best fits your issue when creating it in the issue tracker.

| Template | Use when... |
|----------|-------------|
| Bug Report | Something does not work as expected |
| Failing Test | A test is failing or missing |
| Docs Issue | Documentation is wrong or missing |
| Feature Request | You want a new capability |
| Enhancement Request | You want to improve existing behavior |
| Security Report | You found a vulnerability |
| Question / Support | You need help or clarification |

Read the template instructions carefully before submitting.

### Security Disclosures

If your report could disclose a vulnerability or sensitive information, **do not** open a public issue. Instead, email the project maintainers at [akeoot@pm.me](mailto:akeoot@pm.me) with **"SECURITY"** in the subject line. See `SECURITY.md` if present.

---

## Making Pull Requests

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
feature/add-resource-interface
fix/service-null-ref
docs/improve-contributing-guide
chore/update-dependencies
```

Prefixes: `feature/`, `fix/`, `docs/`, `chore/`, `refactor/`, `test/`

### Pull Request Process

1. Ensure your branch is up to date with `origin/main`:
    ```bash
    git checkout main
    git pull origin main
    git checkout your-feature-branch
    git merge main   # or git rebase main
    ```
2. Open the PR against the `main` branch.
3. Fill out the pull request template.
4. Ensure all CI checks pass.
5. Request a review from a project maintainer.

### Review Process

- Pull requests require approval from project maintainers.
- All reviews are strict – changes that do not meet the standards above will be asked to improve before merging.

### Merge Strategy

**Squash merge** is preferred.<br>
This keeps the commit history clean by collapsing multiple commits into one logical commit per PR. The squashed commit message should follow the Conventional Commits format.

---

**Thank you for contributing to ZenMonitor.Core!**