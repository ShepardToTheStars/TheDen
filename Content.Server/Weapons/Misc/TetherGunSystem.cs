// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.PowerCell;
using Content.Shared.Item.ItemToggle;
using Content.Shared.PowerCell;
using Content.Shared.Weapons.Misc;
using Robust.Shared.Physics.Components;

namespace Content.Server.Weapons.Misc;

public sealed class TetherGunSystem : SharedTetherGunSystem
{
    [Dependency] private readonly PowerCellSystem _cell = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TetherGunComponent, PowerCellSlotEmptyEvent>(OnGunEmpty);
        SubscribeLocalEvent<ForceGunComponent, PowerCellSlotEmptyEvent>(OnGunEmpty);
    }

    private void OnGunEmpty(EntityUid uid, BaseForceGunComponent component, ref PowerCellSlotEmptyEvent args)
    {
        StopTether(uid, component);
    }

    protected override bool CanTether(EntityUid uid, BaseForceGunComponent component, EntityUid target, EntityUid? user)
    {
        if (!base.CanTether(uid, component, target, user))
            return false;

        if (!_cell.HasDrawCharge(uid, user: user))
            return false;

        return true;
    }

    protected override void StartTether(EntityUid gunUid, BaseForceGunComponent component, EntityUid target, EntityUid? user,
        PhysicsComponent? targetPhysics = null, TransformComponent? targetXform = null)
    {
        base.StartTether(gunUid, component, target, user, targetPhysics, targetXform);
        _toggle.TryActivate(gunUid);
    }

    protected override void StopTether(EntityUid gunUid, BaseForceGunComponent component, bool land = true, bool transfer = false)
    {
        base.StopTether(gunUid, component, land, transfer);
        _toggle.TryDeactivate(gunUid);
    }
}
