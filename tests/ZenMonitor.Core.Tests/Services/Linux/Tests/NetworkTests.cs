// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.IO.Abstractions.TestingHelpers;
using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Linux.Services;

namespace ZenMonitor.Core.Tests.Services.Linux.Tests;

[Trait("Platform", "Linux")]
[SupportedOSPlatform("linux")]
public class NetworkTests
{
    private readonly Mock<ILogger<Network>> _mockLogger;
    private readonly MockFileSystem _mockFileSystem;
    private readonly Mock<IServiceAbstraction> _mockHelper;

    public NetworkTests()
    {
        _mockLogger = new Mock<ILogger<Network>>();
        _mockFileSystem = new MockFileSystem();
        _mockHelper = new Mock<IServiceAbstraction>();
    }

    private Network CreateNetwork() => new(_mockLogger.Object, _mockFileSystem, _mockHelper.Object);

    // TODO: add unit tests
}
