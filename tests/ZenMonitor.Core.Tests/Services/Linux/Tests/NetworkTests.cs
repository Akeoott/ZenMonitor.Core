// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Linux.Services;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class NetworkTests
{
    private readonly Mock<ILogger<Network>> _mockLogger;

    public NetworkTests()
    {
        _mockLogger = new Mock<ILogger<Network>>();
    }

    private Network CreateNetwork() => new(_mockLogger.Object);
}
