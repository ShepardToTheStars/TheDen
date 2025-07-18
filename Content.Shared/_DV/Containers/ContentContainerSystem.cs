// SPDX-FileCopyrightText: 2025 M3739 <47579354+M3739@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Shared._DV.Polymorph;
//using Content.Shared.Intellicard; | Floof - M3739 - #937 - DV 3539. Uncomment as soon as Station AI hits the field.
using Content.Shared.Mind.Components;
using Robust.Shared.Containers;

namespace Content.Shared._DV.Containers;

public sealed class ContentContainerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private List<EntityUid> _found = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainerManagerComponent, BeforePolymorphedEvent>(OnBeforePolymorphed);
    }

    private void OnBeforePolymorphed(Entity<ContainerManagerComponent> ent, ref BeforePolymorphedEvent args)
    {
        // Recursively drop all entities with MindContainerComponent in all containers on the entity
        // This prevents players from being sent to null-space when carried by a polymorphing entity
        var stack = new Stack<EntityUid>();
        _found.Clear();
        stack.Push(ent);

        while (stack.Count > 0)
        {
            var currentUid = stack.Pop();

            if (!HasComp<ContainerManagerComponent>(currentUid))
                continue;

            foreach (var container in _container.GetAllContainers(currentUid))
            {
                foreach (var entity in container.ContainedEntities)
                {
                    if (HasComp<MindContainerComponent>(entity))
                    {
                        _found.Add(entity);
                        continue;
                    }

                    // if (HasComp<IntellicardComponent>(entity)) | Floof - M3739 - #937 - DV 3539. Uncomment as soon as Station AI hits the field.
                    // {
                    //     // Dump out the WHOLE intellicard rather than just the entities contained inside.
                    //     _found.Add(entity);
                    //     continue;
                    // }

                    if (stack.Count < 1000) // Unlikely to have over 1000 nested entities unless there is an infinite loop.
                        stack.Push(entity); // Process this entity's containers next
                }
            }
        }

        foreach (var entity in _found)
        {
            _transform.AttachToGridOrMap(entity);
        }
    }
}
