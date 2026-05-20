// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Linux.Services;

/// <summary>
/// Linux implementation of <see cref="INetwork"/>.
/// Currently a placeholder — network metrics are not yet implemented.
/// </summary>
[SupportedOSPlatform("linux")]
public class Network(ILogger<Network> logger) : INetwork
{
    private readonly ILogger<Network> _logger = logger;
    private readonly NetworkInfoSnapshot _snapshot = new("");

    /// <inheritdoc />
    public void Update() => _logger.LogWarning("Network is not implemented yet. Returning empty snapshot...");

    /// <inheritdoc />
    public string GetNone() => _snapshot.None;
}
