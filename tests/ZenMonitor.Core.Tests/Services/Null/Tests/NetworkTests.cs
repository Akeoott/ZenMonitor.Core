// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Xunit;

namespace ZenMonitor.Core.Tests.Services.Null.Tests;

[Trait("Platform", "Linux")] // Only to define runner, does not reflect reality XP
public class NetworkTests
{
    [Fact]
    public void GetAll_CheckThatEverythingIsNull()
    {
        Core.Services.NullNetwork network = new();
        network.Update();
        Assert.Equal("", network.GetNone());
    }
}
