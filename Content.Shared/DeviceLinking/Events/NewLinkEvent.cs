// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.DeviceLinking.Events;

public sealed class NewLinkEvent : EntityEventArgs
{
    public readonly EntityUid Source;
    public readonly EntityUid Sink;
    public readonly EntityUid? User;
    public readonly string SourcePort;
    public readonly string SinkPort;

    public NewLinkEvent(EntityUid? user, EntityUid source, string sourcePort, EntityUid sink, string sinkPort)
    {
        User = user;
        Source = source;
        SourcePort = sourcePort;
        Sink = sink;
        SinkPort = sinkPort;
    }
}
