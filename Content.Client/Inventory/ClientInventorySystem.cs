// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Michael Phillips <1194692+MeltedPixel@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 ike709 <ike709@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 AJCM-git <60196617+ajcm-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <linebarrelerenthusiast@gmail.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client.Clothing;
using Content.Client.Examine;
using Content.Client.Verbs.UI;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Storage;
using JetBrains.Annotations;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Client.Inventory
{
    [UsedImplicitly]
    public sealed class ClientInventorySystem : InventorySystem
    {
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IUserInterfaceManager _ui = default!;

        [Dependency] private readonly ClientClothingSystem _clothingVisualsSystem = default!;
        [Dependency] private readonly ExamineSystem _examine = default!;

        public Action<SlotData>? EntitySlotUpdate = null;
        public Action<SlotData>? OnSlotAdded = null;
        public Action<SlotData>? OnSlotRemoved = null;
        public Action<EntityUid, InventorySlotsComponent>? OnLinkInventorySlots = null;
        public Action? OnUnlinkInventory = null;
        public Action<SlotSpriteUpdate>? OnSpriteUpdate = null;

        private readonly Queue<(InventorySlotsComponent comp, EntityEventArgs args)> _equipEventsQueue = new();

        public override void Initialize()
        {
            UpdatesOutsidePrediction = true;
            base.Initialize();

            SubscribeLocalEvent<InventorySlotsComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<InventorySlotsComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

            SubscribeLocalEvent<InventoryComponent, ComponentShutdown>(OnShutdown);

            SubscribeLocalEvent<InventorySlotsComponent, DidEquipEvent>((_, comp, args) =>
                _equipEventsQueue.Enqueue((comp, args)));
            SubscribeLocalEvent<InventorySlotsComponent, DidUnequipEvent>((_, comp, args) =>
                _equipEventsQueue.Enqueue((comp, args)));
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            while (_equipEventsQueue.TryDequeue(out var tuple))
            {
                var (component, args) = tuple;

                switch (args)
                {
                    case DidEquipEvent equipped:
                        OnDidEquip(component, equipped);
                        break;
                    case DidUnequipEvent unequipped:
                        OnDidUnequip(component, unequipped);
                        break;
                    default:
                        throw new InvalidOperationException($"Received queued event of unknown type: {args.GetType()}");
                }
            }
        }

        private void OnDidUnequip(InventorySlotsComponent component, DidUnequipEvent args)
        {
            UpdateSlot(args.Equipee, component, args.Slot);
            if (args.Equipee != _playerManager.LocalEntity)
                return;
            var update = new SlotSpriteUpdate(null, args.SlotGroup, args.Slot, false);
            OnSpriteUpdate?.Invoke(update);
        }

        private void OnDidEquip(InventorySlotsComponent component, DidEquipEvent args)
        {
            UpdateSlot(args.Equipee, component, args.Slot);
            if (args.Equipee != _playerManager.LocalEntity)
                return;
            var update = new SlotSpriteUpdate(args.Equipment, args.SlotGroup, args.Slot,
                HasComp<StorageComponent>(args.Equipment));
            OnSpriteUpdate?.Invoke(update);
        }

        private void OnShutdown(EntityUid uid, InventoryComponent component, ComponentShutdown args)
        {
            if (uid == _playerManager.LocalEntity)
                OnUnlinkInventory?.Invoke();
        }

        private void OnPlayerDetached(EntityUid uid, InventorySlotsComponent component, LocalPlayerDetachedEvent args)
        {
            OnUnlinkInventory?.Invoke();
        }

        private void OnPlayerAttached(EntityUid uid, InventorySlotsComponent component, LocalPlayerAttachedEvent args)
        {
            if (TryGetSlots(uid, out var definitions))
            {
                foreach (var definition in definitions)
                {
                    if (!TryGetSlotContainer(uid, definition.Name, out var container, out _))
                        continue;

                    if (!component.SlotData.TryGetValue(definition.Name, out var data))
                    {
                        data = new SlotData(definition);
                        component.SlotData[definition.Name] = data;
                    }

                    data.Container = container;
                }
            }

            OnLinkInventorySlots?.Invoke(uid, component);
        }

        public override void Shutdown()
        {
            CommandBinds.Unregister<ClientInventorySystem>();
            base.Shutdown();
        }

        protected override void OnInit(EntityUid uid, InventoryComponent component, ComponentInit args)
        {
            base.OnInit(uid, component, args);
            _clothingVisualsSystem.InitClothing(uid, component);

            if (!TryComp(uid, out InventorySlotsComponent? inventorySlots))
                return;

            foreach (var slot in component.Slots)
            {
                TryAddSlotDef(uid, inventorySlots, slot);
            }
        }

        public void ReloadInventory(InventorySlotsComponent? component = null)
        {
            var player = _playerManager.LocalEntity;
            if (player == null || !Resolve(player.Value, ref component, false))
            {
                return;
            }

            OnUnlinkInventory?.Invoke();
            OnLinkInventorySlots?.Invoke(player.Value, component);
        }

        public void SetSlotHighlight(EntityUid owner, InventorySlotsComponent component, string slotName, bool state)
        {
            var oldData = component.SlotData[slotName];
            var newData = component.SlotData[slotName] = new SlotData(oldData, state);
            if (owner == _playerManager.LocalEntity)
                EntitySlotUpdate?.Invoke(newData);
        }

        public void UpdateSlot(EntityUid owner, InventorySlotsComponent component, string slotName,
            bool? blocked = null, bool? highlight = null)
        {
            var oldData = component.SlotData[slotName];
            var newHighlight = oldData.Highlighted;
            var newBlocked = oldData.Blocked;

            if (blocked != null)
                newBlocked = blocked.Value;

            if (highlight != null)
                newHighlight = highlight.Value;

            var newData = component.SlotData[slotName] =
                new SlotData(component.SlotData[slotName], newHighlight, newBlocked);
            if (owner == _playerManager.LocalEntity)
                EntitySlotUpdate?.Invoke(newData);
        }

        public bool TryAddSlotDef(EntityUid owner, InventorySlotsComponent component, SlotDefinition newSlotDef)
        {
            SlotData newSlotData = newSlotDef; //convert to slotData
            if (!component.SlotData.TryAdd(newSlotDef.Name, newSlotData))
                return false;

            if (owner == _playerManager.LocalEntity)
                OnSlotAdded?.Invoke(newSlotData);
            return true;
        }

        public void UIInventoryActivate(string slot)
        {
            EntityManager.RaisePredictiveEvent(new UseSlotNetworkMessage(slot));
        }

        public void UIInventoryStorageActivate(string slot)
        {
            EntityManager.RaisePredictiveEvent(new OpenSlotStorageNetworkMessage(slot));
        }

        public void UIInventoryExamine(string slot, EntityUid uid)
        {
            if (!TryGetSlotEntity(uid, slot, out var item))
                return;

            _examine.DoExamine(item.Value);
        }

        public void UIInventoryOpenContextMenu(string slot, EntityUid uid)
        {
            if (!TryGetSlotEntity(uid, slot, out var item))
                return;

            _ui.GetUIController<VerbMenuUIController>().OpenVerbMenu(item.Value);
        }

        public void UIInventoryActivateItem(string slot, EntityUid uid)
        {
            if (!TryGetSlotEntity(uid, slot, out var item))
                return;

            EntityManager.RaisePredictiveEvent(
                new InteractInventorySlotEvent(GetNetEntity(item.Value), altInteract: false));
        }

        public void UIInventoryAltActivateItem(string slot, EntityUid uid)
        {
            if (!TryGetSlotEntity(uid, slot, out var item))
                return;

            EntityManager.RaisePredictiveEvent(new InteractInventorySlotEvent(GetNetEntity(item.Value), altInteract: true));
        }

        protected override void UpdateInventoryTemplate(Entity<InventoryComponent> ent)
        {
            base.UpdateInventoryTemplate(ent);

            if (TryComp(ent, out InventorySlotsComponent? inventorySlots))
            {
                foreach (var slot in ent.Comp.Slots)
                {
                    if (inventorySlots.SlotData.TryGetValue(slot.Name, out var slotData))
                        slotData.SlotDef = slot;
                }
            }
        }

        public sealed class SlotData
        {
            [ViewVariables] // Shitmed Change - Mostly for debugging.
            public SlotDefinition SlotDef;
            public EntityUid? HeldEntity => Container?.ContainedEntity;
            public bool Blocked;
            public bool Highlighted;

            [ViewVariables]
            public ContainerSlot? Container;
            public bool HasSlotGroup => SlotDef.SlotGroup != "Default";
            public Vector2i ButtonOffset => SlotDef.UIWindowPosition;
            public string SlotName => SlotDef.Name;
            public bool ShowInWindow => SlotDef.ShowInWindow;
            public string SlotGroup => SlotDef.SlotGroup;
            public string SlotDisplayName => SlotDef.DisplayName;
            public string TextureName => "Slots/" + SlotDef.TextureName;
            public string FullTextureName => SlotDef.FullTextureName;

            public SlotData(SlotDefinition slotDef, ContainerSlot? container = null, bool highlighted = false,
                bool blocked = false)
            {
                SlotDef = slotDef;
                Highlighted = highlighted;
                Blocked = blocked;
                Container = container;
            }

            public SlotData(SlotData oldData, bool highlighted = false, bool blocked = false)
            {
                SlotDef = oldData.SlotDef;
                Highlighted = highlighted;
                Container = oldData.Container;
                Blocked = blocked;
            }

            public static implicit operator SlotData(SlotDefinition s)
            {
                return new SlotData(s);
            }

            public static implicit operator SlotDefinition(SlotData s)
            {
                return s.SlotDef;
            }
        }

        public readonly record struct SlotSpriteUpdate(
            EntityUid? Entity,
            string Group,
            string Name,
            bool ShowStorage
        );
    }
}
