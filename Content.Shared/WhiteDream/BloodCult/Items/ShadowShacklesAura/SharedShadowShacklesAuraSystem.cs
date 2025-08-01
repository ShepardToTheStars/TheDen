// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Shared.Containers;

namespace Content.Shared.WhiteDream.BloodCult.Items.ShadowShacklesAura;

public sealed class SharedShadowShacklesAuraSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffable = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowShacklesAuraComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<ShadowShacklesAuraComponent, MapInitEvent>(OnInit);
    }

    private void OnMeleeHit(EntityUid uid, ShadowShacklesAuraComponent component, MeleeHitEvent args)
    {
        if (!args.HitEntities.Any())
            return;

        component.Target = args.HitEntities.First();
        if (uid == component.Target || HasComp<BloodCultistComponent>(component.Target) || !HasComp<CuffableComponent>(component.Target))
            return;

        if (_cuffable.TryCuffing(args.User, component.Target, component.Shackles, distanceThreshold:component.DistanceThreshold))
            RaiseLocalEvent(uid, new SpeakOnAuraUseEvent(args.User));
    }

    private void OnInit(EntityUid uid, ShadowShacklesAuraComponent component, MapInitEvent args)
    {
        var container = _container.EnsureContainer<Container>(uid, "shackles");
        component.Shackles = Spawn(component.ShacklesProto, _transform.GetMapCoordinates(uid));
        _container.Insert(component.Shackles, container);
    }
}
