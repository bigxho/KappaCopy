using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KappaCopy.App;

public enum StartupAction
{
    Normal,
    Copy,
    Paste
}

public sealed record StartupRequest(
    StartupAction Action,
    string? Path)
{
    public static StartupRequest Parse(
        string[] args)
    {
        if (args.Length == 0)
        {
            return new StartupRequest(
                StartupAction.Normal,
                null);
        }

        if (args.Length >= 2 &&
            args[0].Equals(
                "--copy",
                StringComparison.OrdinalIgnoreCase))
        {
            return new StartupRequest(
                StartupAction.Copy,
                args[1]);
        }

        if (args.Length >= 2 &&
            args[0].Equals(
                "--paste",
                StringComparison.OrdinalIgnoreCase))
        {
            return new StartupRequest(
                StartupAction.Paste,
                args[1]);
        }

        return new StartupRequest(
            StartupAction.Normal,
            null);
    }
}