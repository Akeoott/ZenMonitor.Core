// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

namespace ZenMonitor.Core.Abstractions;

/// <summary>
/// Aggregates all system controller interfaces into a single entry point.
/// Each sub-interface is exposed as a property so consumers (and DI) can
/// access individual controllers or the whole system at once.
/// </summary>
public interface ISystemController
{
}
