// SPDX-FileCopyrightText: 2020 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2020 chairbender <kwhipke1@gmail.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2021 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2022 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jezithyr <Jezithyr.@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Client.UserInterface.Systems.Alerts.Controls;
using Content.Client.UserInterface.Systems.Alerts.Widgets;
using Content.Shared.Alert;
using Robust.Client.UserInterface;
using Robust.Server.Player;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.GameObjects.Components.Mobs
{
    [TestFixture]
    [TestOf(typeof(AlertsComponent))]
    public sealed class AlertsComponentTests
    {
        [Test]
        public async Task AlertsTest()
        {
            await using var pair = await PoolManager.GetServerClient(new PoolSettings
            {
                Connected = true,
                DummyTicker = false
            });
            var server = pair.Server;
            var client = pair.Client;

            var clientUIMgr = client.ResolveDependency<IUserInterfaceManager>();
            var clientEntManager = client.ResolveDependency<IEntityManager>();

            var entManager = server.ResolveDependency<IEntityManager>();
            var serverPlayerManager = server.ResolveDependency<IPlayerManager>();
            var alertsSystem = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<AlertsSystem>();

            EntityUid playerUid = default;
            await server.WaitAssertion(() =>
            {
                playerUid = serverPlayerManager.Sessions.Single().AttachedEntity.GetValueOrDefault();
#pragma warning disable NUnit2045 // Interdependent assertions.
                Assert.That(playerUid, Is.Not.EqualTo(default(EntityUid)));
                // Making sure it exists
                Assert.That(entManager.HasComponent<AlertsComponent>(playerUid));
#pragma warning restore NUnit2045

                var alerts = alertsSystem.GetActiveAlerts(playerUid);
                Assert.That(alerts, Is.Not.Null);
                var alertCount = alerts.Count;

                alertsSystem.ShowAlert(playerUid, "Debug1");
                alertsSystem.ShowAlert(playerUid, "Debug2");

                Assert.That(alerts, Has.Count.EqualTo(alertCount + 2));
            });

            await pair.RunTicksSync(5);

            AlertsUI clientAlertsUI = default;
            await client.WaitAssertion(() =>
            {
                var local = client.Session;
                Assert.That(local, Is.Not.Null);
                var controlled = local.AttachedEntity;
#pragma warning disable NUnit2045 // Interdependent assertions.
                Assert.That(controlled, Is.Not.Null);
                // Making sure it exists
                Assert.That(clientEntManager.HasComponent<AlertsComponent>(controlled.Value));
#pragma warning restore Nunit2045

                // find the alertsui

                clientAlertsUI = FindAlertsUI(clientUIMgr.ActiveScreen);
                Assert.That(clientAlertsUI, Is.Not.Null);

                static AlertsUI FindAlertsUI(Control control)
                {
                    if (control is AlertsUI alertUI)
                        return alertUI;
                    foreach (var child in control.Children)
                    {
                        var found = FindAlertsUI(child);
                        if (found != null)
                            return found;
                    }

                    return null;
                }

                // This originally was hardcoded to expect a player character to have a Human Healthbar.
                // It is no longer hardcoded to demand that.
                // We should be seeing 2 alerts - the 2 debug alerts, in a specific order.
                Assert.That(clientAlertsUI.AlertContainer.ChildCount, Is.GreaterThanOrEqualTo(2));
                var alertControls = clientAlertsUI.AlertContainer.Children.Select(c => (AlertControl) c);
                var alertIDs = alertControls.Select(ac => ac.Alert.ID).ToArray();
                var expectedIDs = new[] { "Debug1", "Debug2" };
                Assert.That(alertIDs, Is.SupersetOf(expectedIDs));
            });

            await server.WaitAssertion(() =>
            {
                alertsSystem.ClearAlert(playerUid, "Debug1");
            });

            await pair.RunTicksSync(5);

            await client.WaitAssertion(() =>
            {
                // We should be seeing 1 alert now because one was cleared
                Assert.That(clientAlertsUI.AlertContainer.ChildCount, Is.GreaterThanOrEqualTo(1));
                var alertControls = clientAlertsUI.AlertContainer.Children.Select(c => (AlertControl) c);
                var alertIDs = alertControls.Select(ac => ac.Alert.ID).ToArray();
                var expectedIDs = new[] { "Debug2" };
                Assert.That(alertIDs, Is.SupersetOf(expectedIDs));
            });

            await pair.CleanReturnAsync();
        }
    }
}
