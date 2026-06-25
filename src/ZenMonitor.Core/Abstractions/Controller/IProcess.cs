// Copyright (c) Ame (Akeoot/Akeoott) <akeoot@pm.me>. Licensed under the LGPL-3.0 Licence.
// See the LICENSE file in the repository root for full license text.

using ZenMonitor.Core.Models;

namespace ZenMonitor.Core.Abstractions.Controller;

// TODO: add xml docs
public interface IProcess
{
    ProcessResult Run(string programName, params string[] arguments);

    ProcessResult Terminate(string processName);
    ProcessResult Terminate(int processId);

    ProcessResult Kill(string processName);
    ProcessResult Kill(int processId);
}
