// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Marat Gadzhiev <15rinkashikachi15@gmail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2023 Checkraze <71046427+Cheackraze@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Eoin Mcloughlin <helloworld@eoinrul.es>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Tom Leys <tom@crump-leys.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 eoineoineoin <eoin.mcloughlin+gh@gmail.com>
// SPDX-FileCopyrightText: 2023 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Flesh <62557990+PolterTzi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 blueDev2 <89804215+blueDev2@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Falcon <falcon@zigtag.dev>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <flyingkarii@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Server.Cargo.Components;
using Content.Server.GameTicking.Events;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;
using Content.Shared.Stacks;
using Content.Shared.Cargo;
using Content.Shared.Cargo.BUI;
using Content.Shared.Cargo.Components;
using Content.Shared.Cargo.Events;
using Content.Shared.CCVar;
using Content.Shared.GameTicking;
using Content.Shared.Shuttles.Components;
using Content.Shared.Tiles;
using Content.Shared.Whitelist;
using Robust.Shared.Map;
using Robust.Shared.Random;
using Robust.Shared.Audio;
using Robust.Shared.Physics.Components;
using Robust.Shared.Utility;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;

namespace Content.Server.Cargo.Systems;

public sealed partial class CargoSystem
{
    /*
     * Handles cargo shuttle / trade mechanics.
     */

    public MapId? CargoMap { get; private set; }

    private static readonly SoundPathSpecifier ApproveSound = new("/Audio/Effects/Cargo/ping.ogg");
    private static readonly ResPath MapPath = new ResPath("/Maps/Shuttles/trading_outpost.yml");

    private void InitializeShuttle()
    {
        SubscribeLocalEvent<TradeStationComponent, GridSplitEvent>(OnTradeSplit);

        SubscribeLocalEvent<CargoShuttleConsoleComponent, ComponentStartup>(OnCargoShuttleConsoleStartup);

        SubscribeLocalEvent<CargoPalletConsoleComponent, CargoPalletSellMessage>(OnPalletSale);
        SubscribeLocalEvent<CargoPalletConsoleComponent, CargoPalletAppraiseMessage>(OnPalletAppraise);
        SubscribeLocalEvent<CargoPalletConsoleComponent, BoundUIOpenedEvent>(OnPalletUIOpen);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<StationInitializedEvent>(OnStationInitialize);

        Subs.CVar(_cfgManager, CCVars.GridFill, SetGridFill);
    }

    private void SetGridFill(bool obj)
    {
        if (obj)
        {
            SetupTradePost();
        }
    }

    #region Console

    private void UpdateCargoShuttleConsoles(EntityUid shuttleUid, CargoShuttleComponent _)
    {
        // Update pilot consoles that are already open.
        _console.RefreshDroneConsoles();

        // Update order consoles.
        var shuttleConsoleQuery = AllEntityQuery<CargoShuttleConsoleComponent>();

        while (shuttleConsoleQuery.MoveNext(out var uid, out var _))
        {
            var stationUid = _station.GetOwningStation(uid);
            if (stationUid != shuttleUid)
                continue;

            UpdateShuttleState(uid, stationUid);
        }
    }

    private void UpdatePalletConsoleInterface(EntityUid uid)
    {
        if (Transform(uid).GridUid is not EntityUid gridUid)
        {
            _uiSystem.SetUiState(uid, CargoPalletConsoleUiKey.Sale,
            new CargoPalletConsoleInterfaceState(0, 0, false));
            return;
        }
        GetPalletGoods(gridUid, out var toSell, out var amount);
        _uiSystem.SetUiState(uid, CargoPalletConsoleUiKey.Sale,
            new CargoPalletConsoleInterfaceState((int) amount, toSell.Count, true));
    }

    private void OnPalletUIOpen(EntityUid uid, CargoPalletConsoleComponent component, BoundUIOpenedEvent args)
        => UpdatePalletConsoleInterface(uid);

    /// <summary>
    /// Ok so this is just the same thing as opening the UI, its a refresh button.
    /// I know this would probably feel better if it were like predicted and dynamic as pallet contents change
    /// However.
    /// I dont want it to explode if cargo uses a conveyor to move 8000 pineapple slices or whatever, they are
    /// known for their entity spam i wouldnt put it past them
    /// </summary>

    private void OnPalletAppraise(EntityUid uid, CargoPalletConsoleComponent component, CargoPalletAppraiseMessage args)
        => UpdatePalletConsoleInterface(uid);

    private void OnCargoShuttleConsoleStartup(EntityUid uid, CargoShuttleConsoleComponent component, ComponentStartup args)
        => UpdateShuttleState(uid, _station.GetOwningStation(uid));

    private void UpdateShuttleState(EntityUid uid, EntityUid? station = null)
    {
        TryComp<StationCargoOrderDatabaseComponent>(station, out var orderDatabase);
        TryComp<CargoShuttleComponent>(orderDatabase?.Shuttle, out var shuttle);

        var orders = GetProjectedOrders(station ?? EntityUid.Invalid, orderDatabase, shuttle);
        var shuttleName = orderDatabase?.Shuttle != null ? MetaData(orderDatabase.Shuttle.Value).EntityName : string.Empty;

        if (_uiSystem.HasUi(uid, CargoConsoleUiKey.Shuttle))
            _uiSystem.SetUiState(uid, CargoConsoleUiKey.Shuttle, new CargoShuttleConsoleBoundUserInterfaceState(
                station != null ? MetaData(station.Value).EntityName : Loc.GetString("cargo-shuttle-console-station-unknown"),
                string.IsNullOrEmpty(shuttleName) ? Loc.GetString("cargo-shuttle-console-shuttle-not-found") : shuttleName,
                orders
            ));
    }

    #endregion

    private void OnTradeSplit(EntityUid uid, TradeStationComponent component, ref GridSplitEvent args)
    {
        // If the trade station gets bombed it's still a trade station.
        foreach (var gridUid in args.NewGrids)
        {
            EnsureComp<TradeStationComponent>(gridUid);
        }
    }

    #region Shuttle

    /// <summary>
    /// Returns the orders that can fit on the cargo shuttle.
    /// </summary>
    private List<CargoOrderData> GetProjectedOrders(
        EntityUid shuttleUid,
        StationCargoOrderDatabaseComponent? component = null,
        CargoShuttleComponent? shuttle = null)
    {
        var orders = new List<CargoOrderData>();

        if (component == null || shuttle == null || component.Orders.Count == 0)
            return orders;

        var spaceRemaining = GetCargoSpace(shuttleUid);
        for (var i = 0; i < component.Orders.Count && spaceRemaining > 0; i++)
        {
            var order = component.Orders[i];
            if (order.Approved)
            {
                var numToShip = order.OrderQuantity - order.NumDispatched;
                if (numToShip > spaceRemaining)
                {
                    // We won't be able to fit the whole order on, so make one
                    // which represents the space we do have left:
                    var reducedOrder = new CargoOrderData(order.OrderId,
                            order.ProductId, order.ProductName, order.Price, spaceRemaining, order.Requester, order.Reason);
                    orders.Add(reducedOrder);
                }
                else
                {
                    orders.Add(order);
                }
                spaceRemaining -= numToShip;
            }
        }

        return orders;
    }

    /// <summary>
    /// Get the amount of space the cargo shuttle can fit for orders.
    /// </summary>
    private int GetCargoSpace(EntityUid gridUid)
    {
        var space = GetCargoPallets(gridUid, BuySellType.Buy).Count;
        return space;
    }

    /// GetCargoPallets(gridUid, BuySellType.Sell) to return only Sell pads
    /// GetCargoPallets(gridUid, BuySellType.Buy) to return only Buy pads
    private List<(EntityUid Entity, CargoPalletComponent Component, TransformComponent PalletXform)> GetCargoPallets(EntityUid gridUid, BuySellType requestType = BuySellType.All)
    {
        _pads.Clear();

        var query = AllEntityQuery<CargoPalletComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var comp, out var compXform))
        {
            if (compXform.ParentUid != gridUid ||
                !compXform.Anchored)
            {
                continue;
            }

            if ((requestType & comp.PalletType) == 0)
            {
                continue;
            }

            _pads.Add((uid, comp, compXform));

        }

        return _pads;
    }

    private List<(EntityUid Entity, CargoPalletComponent Component, TransformComponent Transform)>
        GetFreeCargoPallets(EntityUid gridUid,
            List<(EntityUid Entity, CargoPalletComponent Component, TransformComponent Transform)> pallets)
    {
        _setEnts.Clear();

        List<(EntityUid Entity, CargoPalletComponent Component, TransformComponent Transform)> outList = new();

        foreach (var pallet in pallets)
        {
            var aabb = _lookup.GetAABBNoContainer(pallet.Entity, pallet.Transform.LocalPosition, pallet.Transform.LocalRotation);

            if (_lookup.AnyLocalEntitiesIntersecting(gridUid, aabb, LookupFlags.Dynamic))
                continue;

            outList.Add(pallet);
        }

        return outList;
    }

    #endregion

    #region Station

    private bool SellPallets(EntityUid gridUid, out double amount)
    {
        GetPalletGoods(gridUid, out var toSell, out amount);

        Log.Debug($"Cargo sold {toSell.Count} entities for {amount}");

        if (toSell.Count == 0)
            return false;


        var ev = new EntitySoldEvent(toSell);
        RaiseLocalEvent(ref ev);

        foreach (var ent in toSell)
        {
            Del(ent);
        }

        return true;
    }

    private void GetPalletGoods(EntityUid gridUid, out HashSet<EntityUid> toSell, out double amount)
    {
        amount = 0;
        toSell = new HashSet<EntityUid>();

        foreach (var (palletUid, _, _) in GetCargoPallets(gridUid, BuySellType.Sell))
        {
            // Containers should already get the sell price of their children so can skip those.
            _setEnts.Clear();

            _lookup.GetEntitiesIntersecting(palletUid, _setEnts,
                LookupFlags.Dynamic | LookupFlags.Sundries);

            foreach (var ent in _setEnts)
            {
                // Dont sell:
                // - anything already being sold
                // - anything anchored (e.g. light fixtures)
                // - anything blacklisted (e.g. players).
                if (toSell.Contains(ent) ||
                    _xformQuery.TryGetComponent(ent, out var xform) &&
                    (xform.Anchored || !CanSell(ent, xform)))
                {
                    continue;
                }

                if (_blacklistQuery.HasComponent(ent))
                    continue;

                var price = _pricing.GetPrice(ent);
                if (price == 0)
                    continue;
                toSell.Add(ent);
                amount += price;
            }
        }
    }

    private bool CanSell(EntityUid uid, TransformComponent xform)
    {
        if (_mobQuery.HasComponent(uid))
        {
            return false;
        }

        var complete = IsBountyComplete(uid, out var bountyEntities);

        // Recursively check for mobs at any point.
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (complete && bountyEntities.Contains(child))
                continue;

            if (!CanSell(child, _xformQuery.GetComponent(child)))
                return false;
        }

        return true;
    }

    private void OnPalletSale(EntityUid uid, CargoPalletConsoleComponent component, CargoPalletSellMessage args)
    {
        var player = args.Actor;
        var xform = Transform(uid);

        if (xform.GridUid is not EntityUid gridUid)
        {
            _uiSystem.SetUiState(uid, CargoPalletConsoleUiKey.Sale,
            new CargoPalletConsoleInterfaceState(0, 0, false));
            return;
        }

        if (!SellPallets(gridUid, out var price))
            return;

        var stackPrototype = _protoMan.Index<StackPrototype>(component.CashType);
        _stack.Spawn((int) price, stackPrototype, xform.Coordinates);
        _audio.PlayPvs(ApproveSound, uid);
        UpdatePalletConsoleInterface(uid);
    }

    #endregion

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        Reset();
        CleanupTradeStation();
    }

    private void OnStationInitialize(StationInitializedEvent args)
    {
        if (!HasComp<StationCargoOrderDatabaseComponent>(args.Station)) // No cargo, L
            return;

        if (_cfgManager.GetCVar(CCVars.GridFill) && _cfg.GetCVar(CargoCVars.CreateCargoMap))
            SetupTradePost();
    }

    private void CleanupTradeStation()
    {
        if (CargoMap == null || !_sharedMapSystem.MapExists(CargoMap.Value))
        {
            CargoMap = null;
            DebugTools.Assert(!EntityQuery<CargoShuttleComponent>().Any());
            return;
        }

        _mapSystem.DeleteMap(CargoMap.Value);
        CargoMap = null;
    }

    private void SetupTradePost()
    {
        if (CargoMap != null && _sharedMapSystem.MapExists(CargoMap.Value))
            return;

        // It gets mapinit which is okay... buuutt we still want it paused to avoid power draining.
        var mapEntId = _mapSystem.CreateMap();
        CargoMap = _entityManager.GetComponent<MapComponent>(mapEntId).MapId;
        _mapLoader.TryLoadGrid(CargoMap.Value, MapPath, out var grid); // Oh boy oh boy, hardcoded paths!

        // If this fails to load for whatever reason, cargo is fucked
        if (!grid.HasValue)
            return;

        EnsureComp<ProtectedGridComponent>(grid.Value);
        EnsureComp<TradeStationComponent>(grid.Value);

        var shuttleComponent = EnsureComp<ShuttleComponent>(grid.Value);
        shuttleComponent.AngularDamping = 10000;
        shuttleComponent.LinearDamping = 10000;
        Dirty(grid.Value, shuttleComponent);

        var mapUid = _sharedMapSystem.GetMap(CargoMap.Value);
        var ftl = EnsureComp<FTLDestinationComponent>(mapUid);

        ftl.Whitelist = new EntityWhitelist()
        {
            Components =
            [
                _factory.GetComponentName(typeof(CargoShuttleComponent))
            ]
        };

        _metaSystem.SetEntityName(mapUid, $"Automated Trade Station {_random.Next(1000):000}");
        _console.RefreshShuttleConsoles();
    }
}

/// <summary>
/// Event broadcast raised by-ref before it is sold and
/// deleted but after the price has been calculated.
/// </summary>
[ByRefEvent]
public readonly record struct EntitySoldEvent(HashSet<EntityUid> Sold);
