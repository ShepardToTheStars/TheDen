// SPDX-FileCopyrightText: 2021 Paul <ritter.paul1+git@googlemail.com>
// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using SQLitePCL;

namespace Content.Server.Decals.Commands
{
    [AdminCommand(AdminFlags.Mapping)]
    public sealed class RemoveDecalCommand : IConsoleCommand
    {
        [Dependency] private readonly IEntityManager _entManager = default!;

        public string Command => "rmdecal";
        public string Description => "removes a decal";
        public string Help => $"{Command} <uid> <gridId>";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                shell.WriteError($"Unexpected number of arguments.\nExpected two: {Help}");
                return;
            }

            if (!uint.TryParse(args[0], out var uid))
            {
                shell.WriteError($"Failed parsing uid.");
                return;
            }

            if (!NetEntity.TryParse(args[1], out var rawGridIdNet) ||
                !_entManager.TryGetEntity(rawGridIdNet, out var rawGridId) ||
                !_entManager.HasComponent<MapGridComponent>(rawGridId))
            {
                shell.WriteError("Failed parsing gridId.");
                return;
            }

            var decalSystem = _entManager.System<DecalSystem>();
            if (decalSystem.RemoveDecal(rawGridId.Value, uid))
            {
                shell.WriteLine($"Successfully removed decal {uid}.");
                return;
            }

            shell.WriteError($"Failed trying to remove decal {uid}.");
        }
    }
}
