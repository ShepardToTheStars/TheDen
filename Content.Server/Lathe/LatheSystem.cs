// SPDX-FileCopyrightText: 2020 Víctor Aguilera Puerto
// SPDX-FileCopyrightText: 2020 chairbender
// SPDX-FileCopyrightText: 2021 Acruid
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto
// SPDX-FileCopyrightText: 2022 0x6273
// SPDX-FileCopyrightText: 2022 Chief-Engineer
// SPDX-FileCopyrightText: 2022 Chris V
// SPDX-FileCopyrightText: 2022 Paul Ritter
// SPDX-FileCopyrightText: 2022 Rane
// SPDX-FileCopyrightText: 2022 Visne
// SPDX-FileCopyrightText: 2022 keronshb
// SPDX-FileCopyrightText: 2023 Debug
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2023 Hannah Giovanna Dawson
// SPDX-FileCopyrightText: 2023 Moony
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers
// SPDX-FileCopyrightText: 2023 TemporalOroboros
// SPDX-FileCopyrightText: 2023 chromiumboy
// SPDX-FileCopyrightText: 2023 ubis1
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT
// SPDX-FileCopyrightText: 2024 Ed
// SPDX-FileCopyrightText: 2024 Ilya246
// SPDX-FileCopyrightText: 2024 Nemanja
// SPDX-FileCopyrightText: 2024 metalgearsloth
// SPDX-FileCopyrightText: 2025 Blitz
// SPDX-FileCopyrightText: 2025 EctoplasmIsGood
// SPDX-FileCopyrightText: 2025 Leon Friedrich
// SPDX-FileCopyrightText: 2025 Tayrtahn
// SPDX-FileCopyrightText: 2025 deltanedas
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Lathe.Components;
using Content.Server.Materials;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Stack;
using Content.Shared.Atmos;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.UserInterface;
using Content.Shared.Database;
using Content.Shared.Emag.Components;
using Content.Shared.Examine;
using Content.Shared.Lathe;
using Content.Shared.Lathe.Prototypes;
using Content.Shared.Localizations;
using Content.Shared.Materials;
using Content.Shared.Power;
using Content.Shared.ReagentSpeed;
using Content.Shared.Research.Components;
using Content.Shared.Research.Prototypes;
using JetBrains.Annotations;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Server.Chat.Systems;
using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Systems;
using Content.Shared.Chat;

namespace Content.Server.Lathe
{
    [UsedImplicitly]
    public sealed class LatheSystem : SharedLatheSystem
    {
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IPrototypeManager _proto = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger = default!;
        [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly ContainerSystem _container = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSys = default!;
        [Dependency] private readonly MaterialStorageSystem _materialStorage = default!;
        [Dependency] private readonly PopupSystem _popup = default!;
        [Dependency] private readonly PuddleSystem _puddle = default!;
        [Dependency] private readonly ReagentSpeedSystem _reagentSpeed = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
        [Dependency] private readonly StackSystem _stack = default!;
        [Dependency] private readonly TransformSystem _transform = default!;
        [Dependency] private readonly RadioSystem _radio = default!;
        [Dependency] private readonly StationSystem _station = default!;

        /// <summary>
        /// Per-tick cache
        /// </summary>
        private readonly List<GasMixture> _environments = new();

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<LatheComponent, GetMaterialWhitelistEvent>(OnGetWhitelist);
            SubscribeLocalEvent<LatheComponent, MapInitEvent>(OnMapInit);
            SubscribeLocalEvent<LatheComponent, PowerChangedEvent>(OnPowerChanged);
            SubscribeLocalEvent<LatheComponent, TechnologyDatabaseModifiedEvent>(OnDatabaseModified);
            SubscribeLocalEvent<LatheAnnouncingComponent, TechnologyDatabaseModifiedEvent>(OnAnnouncingDatabaseModified);
            SubscribeLocalEvent<LatheComponent, ResearchRegistrationChangedEvent>(OnResearchRegistrationChanged);

            SubscribeLocalEvent<LatheComponent, LatheQueueRecipeMessage>(OnLatheQueueRecipeMessage);
            SubscribeLocalEvent<LatheComponent, LatheSyncRequestMessage>(OnLatheSyncRequestMessage);

            SubscribeLocalEvent<LatheComponent, BeforeActivatableUIOpenEvent>((u, c, _) => UpdateUserInterfaceState(u, c));
            SubscribeLocalEvent<LatheComponent, MaterialAmountChangedEvent>(OnMaterialAmountChanged);
            SubscribeLocalEvent<TechnologyDatabaseComponent, LatheGetRecipesEvent>(OnGetRecipes);
            SubscribeLocalEvent<EmagLatheRecipesComponent, LatheGetRecipesEvent>(GetEmagLatheRecipes);
            SubscribeLocalEvent<LatheHeatProducingComponent, LatheStartPrintingEvent>(OnHeatStartPrinting);
        }
        public override void Update(float frameTime)
        {
            var query = EntityQueryEnumerator<LatheProducingComponent, LatheComponent>();
            while (query.MoveNext(out var uid, out var comp, out var lathe))
            {
                if (lathe.CurrentRecipe == null)
                    continue;

                if (_timing.CurTime - comp.StartTime >= comp.ProductionLength)
                    FinishProducing(uid, lathe);
            }

            var heatQuery = EntityQueryEnumerator<LatheHeatProducingComponent, LatheProducingComponent, TransformComponent>();
            while (heatQuery.MoveNext(out var uid, out var heatComp, out _, out var xform))
            {
                if (_timing.CurTime < heatComp.NextSecond)
                    continue;
                heatComp.NextSecond += TimeSpan.FromSeconds(1);

                var position = _transform.GetGridTilePositionOrDefault((uid, xform));
                _environments.Clear();

                if (_atmosphere.GetTileMixture(xform.GridUid, xform.MapUid, position, true) is { } tileMix)
                    _environments.Add(tileMix);

                if (xform.GridUid != null)
                {
                    var enumerator = _atmosphere.GetAdjacentTileMixtures(xform.GridUid.Value, position, false, true);
                    while (enumerator.MoveNext(out var mix))
                    {
                        _environments.Add(mix);
                    }
                }

                if (_environments.Count > 0)
                {
                    var heatPerTile = heatComp.EnergyPerSecond / _environments.Count;
                    foreach (var env in _environments)
                    {
                        _atmosphere.AddHeat(env, heatPerTile);
                    }
                }
            }
        }

        private void OnGetWhitelist(EntityUid uid, LatheComponent component, ref GetMaterialWhitelistEvent args)
        {
            if (args.Storage != uid)
                return;
            var materialWhitelist = new List<ProtoId<MaterialPrototype>>();
            var recipes = GetAvailableRecipes(uid, component, true);
            foreach (var id in recipes)
            {
                if (!_proto.TryIndex(id, out var proto))
                    continue;
                foreach (var (mat, _) in proto.Materials)
                {
                    if (!materialWhitelist.Contains(mat))
                    {
                        materialWhitelist.Add(mat);
                    }
                }
            }

            var combined = args.Whitelist.Union(materialWhitelist).ToList();
            args.Whitelist = combined;
        }

        [PublicAPI]
        public bool TryGetAvailableRecipes(EntityUid uid, [NotNullWhen(true)] out List<ProtoId<LatheRecipePrototype>>? recipes, [NotNullWhen(true)] LatheComponent? component = null, bool getUnavailable = false)
        {
            if (!Resolve(uid, ref component))
            {
                recipes = null;
                return false;
            }

            recipes = GetAvailableRecipes(uid, component, getUnavailable);
            return true;
        }

        public List<ProtoId<LatheRecipePrototype>> GetAvailableRecipes(EntityUid uid, LatheComponent component, bool getUnavailable = false)
        {
            var ev = new LatheGetRecipesEvent((uid, component), getUnavailable);
            AddRecipesFromPacks(ev.Recipes, component.StaticPacks);
            RaiseLocalEvent(uid, ev);
            return ev.Recipes.ToList();
        }

        public bool TryAddToQueue(EntityUid uid, LatheRecipePrototype recipe, LatheComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return false;

            if (!CanProduce(uid, recipe, 1, component))
                return false;

            foreach (var (mat, amount) in recipe.Materials)
            {
                var adjustedAmount = recipe.ApplyMaterialDiscount
                    ? (int) (-amount * component.MaterialUseMultiplier)
                    : -amount;

                _materialStorage.TryChangeMaterialAmount(uid, mat, adjustedAmount);
            }
            component.Queue.Enqueue(recipe);

            return true;
        }

        public bool TryStartProducing(EntityUid uid, LatheComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return false;
            if (component.CurrentRecipe != null || component.Queue.Count <= 0 || !this.IsPowered(uid, EntityManager))
                return false;

            var recipeProto = component.Queue.Dequeue();
            var recipe = _proto.Index(recipeProto);

            var time = _reagentSpeed.ApplySpeed(uid, recipe.CompleteTime) * component.TimeMultiplier;

            var lathe = EnsureComp<LatheProducingComponent>(uid);
            lathe.StartTime = _timing.CurTime;
            lathe.ProductionLength = time;
            component.CurrentRecipe = recipe;

            var ev = new LatheStartPrintingEvent(recipe);
            RaiseLocalEvent(uid, ref ev);

            _audio.PlayPvs(component.ProducingSound, uid);
            UpdateRunningAppearance(uid, true);
            UpdateUserInterfaceState(uid, component);

            if (time == TimeSpan.Zero)
            {
                FinishProducing(uid, component, lathe);
            }
            return true;
        }

        public void FinishProducing(EntityUid uid, LatheComponent? comp = null, LatheProducingComponent? prodComp = null)
        {
            if (!Resolve(uid, ref comp, ref prodComp, false))
                return;

            if (comp.CurrentRecipe != null)
            {
                var currentRecipe = _proto.Index(comp.CurrentRecipe.Value);
                if (currentRecipe.Result is { } resultProto)
                {
                    var result = Spawn(resultProto, Transform(uid).Coordinates);
                    _stack.TryMergeToContacts(result);
                }

                if (currentRecipe.ResultReagents is { } resultReagents &&
                    comp.ReagentOutputSlotId is { } slotId)
                {
                    var toAdd = new Solution(
                        resultReagents.Select(p => new ReagentQuantity(p.Key.Id, p.Value, null)));

                    // dispense it in the container if we have it and dump it if we don't
                    if (_container.TryGetContainer(uid, slotId, out var container) &&
                        container.ContainedEntities.Count == 1 &&
                        _solution.TryGetFitsInDispenser(container.ContainedEntities.First(), out var solution, out _))
                    {
                        _solution.AddSolution(solution.Value, toAdd);
                    }
                    else
                    {
                        _popup.PopupEntity(Loc.GetString("lathe-reagent-dispense-no-container", ("name", uid)), uid);
                        _puddle.TrySpillAt(uid, toAdd, out _);
                    }
                }
            }

            comp.CurrentRecipe = null;
            prodComp.StartTime = _timing.CurTime;

            if (!TryStartProducing(uid, comp))
            {
                RemCompDeferred(uid, prodComp);
                UpdateUserInterfaceState(uid, comp);
                UpdateRunningAppearance(uid, false);
            }
        }

        public void UpdateUserInterfaceState(EntityUid uid, LatheComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            var producing = component.CurrentRecipe;
            if (producing == null && component.Queue.TryPeek(out var next))
                producing = next;

            var state = new LatheUpdateState(GetAvailableRecipes(uid, component), component.Queue.ToArray(), producing);
            _uiSys.SetUiState(uid, LatheUiKey.Key, state);
        }

        /// <summary>
        /// Adds every unlocked recipe from each pack to the recipes list.
        /// </summary>
        public void AddRecipesFromDynamicPacks(ref LatheGetRecipesEvent args, TechnologyDatabaseComponent database, IEnumerable<ProtoId<LatheRecipePackPrototype>> packs)
        {
            foreach (var id in packs)
            {
                var pack = _proto.Index(id);
                foreach (var recipe in pack.Recipes)
                {
                    if (args.GetUnavailable || database.UnlockedRecipes.Contains(recipe))
                        args.Recipes.Add(recipe);
                }
            }
        }

        private void OnGetRecipes(EntityUid uid, TechnologyDatabaseComponent component, LatheGetRecipesEvent args)
        {
            if (uid == args.Lathe)
                AddRecipesFromDynamicPacks(ref args, component, args.Comp.DynamicPacks);
        }

        private void GetEmagLatheRecipes(EntityUid uid, EmagLatheRecipesComponent component, LatheGetRecipesEvent args)
        {
            if (uid != args.Lathe)
                return;

            if (!args.GetUnavailable && !HasComp<EmaggedComponent>(uid))
                return;

            AddRecipesFromPacks(args.Recipes, component.EmagStaticPacks);

            if (TryComp<TechnologyDatabaseComponent>(uid, out var database))
                AddRecipesFromDynamicPacks(ref args, database, component.EmagDynamicPacks);
        }

        private void OnHeatStartPrinting(EntityUid uid, LatheHeatProducingComponent component, LatheStartPrintingEvent args)
        {
            component.NextSecond = _timing.CurTime;
        }

        private void OnMaterialAmountChanged(EntityUid uid, LatheComponent component, ref MaterialAmountChangedEvent args)
        {
            UpdateUserInterfaceState(uid, component);
        }

        /// <summary>
        /// Initialize the UI and appearance.
        /// Appearance requires initialization or the layers break
        /// </summary>
        private void OnMapInit(EntityUid uid, LatheComponent component, MapInitEvent args)
        {
            _appearance.SetData(uid, LatheVisuals.IsInserting, false);
            _appearance.SetData(uid, LatheVisuals.IsRunning, false);

            _materialStorage.UpdateMaterialWhitelist(uid);
        }

        /// <summary>
        /// Sets the machine sprite to either play the running animation
        /// or stop.
        /// </summary>
        private void UpdateRunningAppearance(EntityUid uid, bool isRunning)
        {
            _appearance.SetData(uid, LatheVisuals.IsRunning, isRunning);
        }

        private void OnPowerChanged(EntityUid uid, LatheComponent component, ref PowerChangedEvent args)
        {
            if (!args.Powered)
            {
                RemComp<LatheProducingComponent>(uid);
                UpdateRunningAppearance(uid, false);
            }
            else if (component.CurrentRecipe != null)
            {
                EnsureComp<LatheProducingComponent>(uid);
                TryStartProducing(uid, component);
            }
        }

        private void OnDatabaseModified(Entity<LatheComponent> ent, ref TechnologyDatabaseModifiedEvent args)
        {
            UpdateUserInterfaceState(ent, ent.Comp);
        }

        private void OnAnnouncingDatabaseModified(
            Entity<LatheAnnouncingComponent> ent,
            ref TechnologyDatabaseModifiedEvent args
        )
        {
            if (!Exists(ent) || !Exists(args.Server))
                return;

            var latheStation = _station.GetOwningStation(ent);
            var serverStation = _station.GetOwningStation(args.Server);

            if (args.UnlockedRecipes.Count == 0
                || !TryGetAvailableRecipes(ent.Owner, out var potentialRecipes)
                || latheStation == null // no grid
                || serverStation != latheStation) // server is on a separate grid than lathe
                return;

            var recipeNames = new List<string>();
            var technologyName = GetTechnologyName(args);

            foreach (var recipeId in args.UnlockedRecipes)
            {
                if (string.IsNullOrWhiteSpace(recipeId)
                    || !_proto.TryIndex(recipeId, out var recipe)
                    || potentialRecipes.All(targetRecipe => targetRecipe.Id != recipeId))
                    continue;

                var itemName = GetRecipeName(recipe);
                recipeNames.Add(itemName);
            }

            if (recipeNames.Count == 0)
                return;

            var message = GetUpdateMessage(recipeNames, technologyName);

            foreach (var channel in ent.Comp.Channels)
                _radio.SendRadioMessage(ent.Owner, message, channel, ent.Owner, escapeMarkup: false);
        }

        private string? GetTechnologyName(TechnologyDatabaseModifiedEvent args)
        {
            if (args.Technology == null)
                return null;

            var technology = _proto.Index<TechnologyPrototype>(args.Technology);
            var technologyName = Loc.GetString(technology.Name);
            return technologyName;
        }

        private string GetUpdateMessage(List<string> recipes, string? technologyName)
        {
            if (technologyName != null)
            {
                return Loc.GetString("lathe-technology-recipes-update-message",
                    ("technology", technologyName),
                    ("count", recipes.Count));
            }

            // This will never happen. I think.
            // Best to be safe.
            if (recipes.Count != 1)
            {
                return Loc.GetString("lathe-technology-recipes-update-message-multiple",
                    ("count", recipes.Count));
            }

            return Loc.GetString(
                "lathe-technology-recipes-update-message-single",
                ("item", recipes.First()));
        }

        private void OnResearchRegistrationChanged(EntityUid uid, LatheComponent component, ref ResearchRegistrationChangedEvent args)
        {
            UpdateUserInterfaceState(uid, component);
        }

        protected override bool HasRecipe(EntityUid uid, LatheRecipePrototype recipe, LatheComponent component)
        {
            return GetAvailableRecipes(uid, component).Contains(recipe.ID);
        }

        #region UI Messages

        private void OnLatheQueueRecipeMessage(EntityUid uid, LatheComponent component, LatheQueueRecipeMessage args)
        {
            if (_proto.TryIndex(args.ID, out LatheRecipePrototype? recipe))
            {
                var count = 0;
                for (var i = 0; i < args.Quantity; i++)
                {
                    if (TryAddToQueue(uid, recipe, component))
                        count++;
                    else
                        break;
                }
                if (count > 0)
                {
                    _adminLogger.Add(LogType.Action,
                        LogImpact.Low,
                        $"{ToPrettyString(args.Actor):player} queued {count} {GetRecipeName(recipe)} at {ToPrettyString(uid):lathe}");
                }
            }
            TryStartProducing(uid, component);
            UpdateUserInterfaceState(uid, component);
        }

        private void OnLatheSyncRequestMessage(EntityUid uid, LatheComponent component, LatheSyncRequestMessage args)
        {
            UpdateUserInterfaceState(uid, component);
        }
        #endregion
    }
}
