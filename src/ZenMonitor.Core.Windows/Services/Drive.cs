// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.Versioning;

using Microsoft.Extensions.Logging;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Services;

/// <summary>
/// Windows implementation of <see cref="IDrive"/> that reads Drive metrics
/// via native Win32 API calls through <see cref="IWindows"/>.
/// </summary>
[SupportedOSPlatform("windows")]
public class Drive(ILogger<Drive> logger, IServiceAbstraction helper) : IDrive
{
    private readonly ILogger<Drive> _logger = logger;
    private readonly IServiceAbstraction _helper = helper;
    private DriveInfoSnapshot _snapshot = new([]);

    /// <inheritdoc />
    public void Update() => _snapshot = FetchDriveInfo();

    /// <inheritdoc />
    public DriveMountInfo[] GetMountInfos() => _snapshot.MountInfos;

    private DriveInfoSnapshot FetchDriveInfo()
    {
        try
        {
            _logger.LogTrace("Fetching all Drive info...");
            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch Drive info");
            return new DriveInfoSnapshot([]);
        }
    }
}
