---
name: "Failing Test"
about: "Report failing tests or CI jobs."
title: "[Test] "
labels: "Type: Test"

---

<!--
Hi there!

To expedite issue processing, please search open and closed issues before submitting a new one.
-->

# **Failing Test**

## **Test file location**
<!-- Which test file(s) are failing? Include the full path if possible, e.g. ZenMonitor.Tests/Services/Linux/CpuServiceTests.cs -->

[replace with test file path(s)]

---

## **Platform filter**
<!-- Tests are annotated with [Trait("Platform", "...")]. Select the affected platform(s). -->

- [ ] **Linux** — `--filter "Platform=Linux"`
- [ ] **Windows** — `--filter "Platform=Windows"`
- [ ] **Both** — fails on all platforms

---

## **Failure description**
<!-- Describe what is failing and why. Include the error message or stack trace if available. -->

[replace with description]

---

## **Have you checked recent CI runs?**
<!-- Sometimes a test fails due to infrastructure issues rather than a code change. -->

- [ ] Yes, CI runs are passing on `main`
- [ ] No, CI is also failing on `main`

---

## **Screenshots / Logs**
<!-- If applicable, add logs, screenshots or videos to help explain the problem. -->

---

## **Additional context**
<!-- Add any other context about the problem here. -->

[replace with additional context]