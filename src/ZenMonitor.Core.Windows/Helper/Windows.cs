// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;

using ZenMonitor.Core.Interfaces;
using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Windows.Helper;

/// <summary>
/// Provides system-level helper operations that are abstracted for testability.
/// </summary>
[ExcludeFromCodeCoverage]
[SupportedOSPlatform("windows")]
public class Windows : IWindows
{

}
