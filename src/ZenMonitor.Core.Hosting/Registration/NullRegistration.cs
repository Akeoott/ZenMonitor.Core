// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using Microsoft.Extensions.DependencyInjection;

using ZenMonitor.Core.Abstractions;
using ZenMonitor.Core.Services;

namespace ZenMonitor.Core.Hosting.Registration;

/// <summary>
/// Registers Null (no-op) ZenMonitor services as a fallback
/// for unsupported platforms or failed detection.
/// </summary>
internal static class NullRegistration
{
    internal static void Register(IServiceCollection services)
    {
        services.AddSingleton<ICpu, NullCpu>();
        services.AddSingleton<IDrive, NullDrive>();
        services.AddSingleton<IGpu, NullGpu>();
        services.AddSingleton<IMemory, NullMemory>();
        services.AddSingleton<INetwork, NullNetwork>();
        services.AddSingleton<ISystem, NullSystem>();
    }
}
