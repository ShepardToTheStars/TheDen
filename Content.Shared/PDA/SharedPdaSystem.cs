// SPDX-FileCopyrightText: 2022 Leon Friedrich
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 0x6273
// SPDX-FileCopyrightText: 2023 Nemanja
// SPDX-FileCopyrightText: 2023 TemporalOroboros
// SPDX-FileCopyrightText: 2023 Visne
// SPDX-FileCopyrightText: 2023 metalgearsloth
// SPDX-FileCopyrightText: 2024 VMSolidus
// SPDX-FileCopyrightText: 2025 Sir Warock
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: MIT

using Content.Shared.Access.Components;
using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;

namespace Content.Shared.PDA
{
    public abstract class SharedPdaSystem : EntitySystem
    {
        [Dependency] protected readonly ItemSlotsSystem ItemSlotsSystem = default!;
        [Dependency] protected readonly SharedAppearanceSystem Appearance = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<PdaComponent, ComponentInit>(OnComponentInit);
            SubscribeLocalEvent<PdaComponent, ComponentRemove>(OnComponentRemove);

            SubscribeLocalEvent<PdaComponent, EntInsertedIntoContainerMessage>(OnItemInserted);
            SubscribeLocalEvent<PdaComponent, EntRemovedFromContainerMessage>(OnItemRemoved);

            SubscribeLocalEvent<PdaComponent, GetAdditionalAccessEvent>(OnGetAdditionalAccess);
        }
        protected virtual void OnComponentInit(EntityUid uid, PdaComponent pda, ComponentInit args)
        {
            if (pda.IdCard != null)
                pda.IdSlot.StartingItem = pda.IdCard;

            ItemSlotsSystem.AddItemSlot(uid, PdaComponent.PdaIdSlotId, pda.IdSlot);
            ItemSlotsSystem.AddItemSlot(uid, PdaComponent.PdaPenSlotId, pda.PenSlot);
            ItemSlotsSystem.AddItemSlot(uid, PdaComponent.PdaPaiSlotId, pda.PaiSlot);
            ItemSlotsSystem.AddItemSlot(uid, PdaComponent.PdaPassportSlotId, pda.PassportSlot); // The Den


            UpdatePdaAppearance(uid, pda);
        }

        private void OnComponentRemove(EntityUid uid, PdaComponent pda, ComponentRemove args)
        {
            ItemSlotsSystem.RemoveItemSlot(uid, pda.IdSlot);
            ItemSlotsSystem.RemoveItemSlot(uid, pda.PenSlot);
            ItemSlotsSystem.RemoveItemSlot(uid, pda.PaiSlot);
        }

        protected virtual void OnItemInserted(EntityUid uid, PdaComponent pda, EntInsertedIntoContainerMessage args)
        {
            if (args.Container.ID == PdaComponent.PdaIdSlotId)
                pda.ContainedId = args.Entity;

            UpdatePdaAppearance(uid, pda);
        }

        protected virtual void OnItemRemoved(EntityUid uid, PdaComponent pda, EntRemovedFromContainerMessage args)
        {
            if (args.Container.ID == pda.IdSlot.ID)
                pda.ContainedId = null;

            UpdatePdaAppearance(uid, pda);
        }

        private void OnGetAdditionalAccess(EntityUid uid, PdaComponent component, ref GetAdditionalAccessEvent args)
        {
            if (component.ContainedId is { } id)
                args.Entities.Add(id);
        }

        private void UpdatePdaAppearance(EntityUid uid, PdaComponent pda)
        {
            Appearance.SetData(uid, PdaVisuals.IdCardInserted, pda.ContainedId != null);
        }
    }
}
