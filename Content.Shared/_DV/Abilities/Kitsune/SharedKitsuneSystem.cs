// SPDX-FileCopyrightText: 2025 M3739 <47579354+M3739@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tabitha <64847293+KyuPolaris@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._Shitmed.Humanoid.Events;
using Content.Shared.Actions;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Popups;

namespace Content.Shared._DV.Abilities.Kitsune;

public abstract class SharedKitsuneSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _light = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KitsuneComponent, CreateFoxfireActionEvent>(OnCreateFoxfire);
        SubscribeLocalEvent<FoxfireComponent, ComponentShutdown>(OnFoxfireShutdown);
        SubscribeLocalEvent<KitsuneComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<KitsuneComponent, ProfileLoadFinishedEvent>(OnProfileLoadFinished);
    }

    private void OnProfileLoadFinished(Entity<KitsuneComponent> ent, ref ProfileLoadFinishedEvent args)
    {
        // Eye color is stored on component to be used for fox fire/fox form color.
        if (TryComp<HumanoidAppearanceComponent>(ent, out var humanComp))
        {
            ent.Comp.Color = humanComp.EyeColor;

            var lightColor = ent.Comp.Color.Value;
            var max = MathF.Max(lightColor.R, MathF.Max(lightColor.G, lightColor.B));
            // Don't let it divide by 0
            if (max == 0)
            {
                lightColor = new Color(1, 1, 1, lightColor.A);
            }
            else
            {
                var factor = 1 / max;
                lightColor.R *= factor;
                lightColor.G *= factor;
                lightColor.B *= factor;
            }
            ent.Comp.ColorLight = lightColor;
        }
    }

    private void OnMapInit(Entity<KitsuneComponent> ent, ref MapInitEvent args)
    {
        // Kitsune Fox form should not have action to transform into fox form.
        if (!HasComp<KitsuneFoxComponent>(ent))
            _actions.AddAction(ent, ref ent.Comp.KitsuneActionEntity, ent.Comp.KitsuneAction);
        ent.Comp.FoxfireAction = _actions.AddAction(ent, ent.Comp.FoxfireActionId);
    }

    private void OnCreateFoxfire(Entity<KitsuneComponent> ent, ref CreateFoxfireActionEvent args)
    {
        // Kitsune fox can make fox fires from their mouth otherwise they need hands.
        if ((!TryComp<HandsComponent>(ent, out var hands) || hands.Count < 1) && !HasComp<KitsuneFoxComponent>(ent))
        {
            _popup.PopupEntity(Loc.GetString("fox-no-hands"), ent, ent);
            return;
        }
        // Floof - M3739 - #1030 - Do not allow additional fox fires if they have no charges. They will have to wait.
        if (_actions.GetCharges(ent.Comp.FoxfireAction) <= 0)
        {
            _popup.PopupEntity(Loc.GetString("fox-no-charges"), ent, ent);
            return;
        }
        // Floof - M3739 - #1030 - This... is probably the least intrusive solution to the infinite foxfire problem.
        // Ensure that the number of active fox fires does not exceed max charges.
        if (!TryComp<InstantActionComponent>(ent.Comp.FoxfireAction, out var instantActionComp))
            return;
        if (ent.Comp.ActiveFoxFires.Count >= instantActionComp.MaxCharges)
        {
            QueueDel(ent.Comp.ActiveFoxFires[0]);
            ent.Comp.ActiveFoxFires.RemoveAt(0);
        }

        var fireEnt = Spawn(ent.Comp.FoxfirePrototype, Transform(ent).Coordinates);
        var fireComp = EnsureComp<FoxfireComponent>(fireEnt);
        fireComp.Kitsune = ent;
        ent.Comp.ActiveFoxFires.Add(fireEnt);
        _actions.RemoveCharges(ent.Comp.FoxfireAction, 1); // Floof - M3739 - #1030
        Dirty(fireEnt, fireComp);
        Dirty(ent);

        _light.SetColor(fireEnt, ent.Comp.ColorLight ?? Color.Purple);
    }

    private void OnFoxfireShutdown(Entity<FoxfireComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Kitsune is not { } kitsune || !TryComp<KitsuneComponent>(kitsune, out var kitsuneComp))
            return;

        // Stop tracking the removed fox fire
        kitsuneComp.ActiveFoxFires.Remove(ent);

        // Refund the fox fire charge
        _actions.AddCharges(kitsuneComp.FoxfireAction, 1);

        // If charges exceeds the maximum then set charges to max
        var foxfireAction = kitsuneComp.FoxfireAction;
        if (!TryComp<InstantActionComponent>(foxfireAction, out var instantActionComp))
            return;
        if (_actions.GetCharges(foxfireAction) > instantActionComp.MaxCharges)
            _actions.SetCharges(foxfireAction, instantActionComp.MaxCharges);

        Dirty(kitsune, kitsuneComp);
    }
}

public sealed partial class MorphIntoKitsune : InstantActionEvent;
