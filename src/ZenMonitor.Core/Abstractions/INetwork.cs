// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Abstractions;

/// <summary>
/// Placeholder interface for network monitoring.
/// Implementation is pending and currently returns empty stubs.
/// </summary>
public interface INetwork
{
    /// <summary>Updates all cached network metrics.</summary>
    void Update();

    /// <summary>Returns a placeholder value. This will be redesigned with proper network metrics.</summary>
    string GetNone();
}
