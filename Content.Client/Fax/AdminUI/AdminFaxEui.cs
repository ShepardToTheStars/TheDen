// SPDX-FileCopyrightText: 2023 Morb <14136326+Morb0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Eui;
using Content.Shared.Eui;
using Content.Shared.Fax;
using JetBrains.Annotations;

namespace Content.Client.Fax.AdminUI;

[UsedImplicitly]
public sealed class AdminFaxEui : BaseEui
{
    private readonly AdminFaxWindow _window;

    public AdminFaxEui()
    {
        _window = new AdminFaxWindow();
        _window.OnClose += () => SendMessage(new AdminFaxEuiMsg.Close());
        _window.OnFollowFax += entity => SendMessage(new AdminFaxEuiMsg.Follow(entity));
        _window.OnMessageSend += args => SendMessage(new AdminFaxEuiMsg.Send(args.entity, args.title,
                    args.stampedBy, args.message, args.stampSprite, args.stampColor));
    }

    public override void Opened()
    {
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not AdminFaxEuiState cast)
            return;
        _window.PopulateFaxes(cast.Entries);
    }
}
