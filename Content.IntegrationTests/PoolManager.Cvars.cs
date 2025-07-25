// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DEATHB4DEFEAT <77995199+DEATHB4DEFEAT@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 FoxxoTrystan <45297731+FoxxoTrystan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 Cam <Nop>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

#nullable enable
using Content.Shared.CCVar;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.UnitTesting;

namespace Content.IntegrationTests;

// Partial class containing cvar logic
public static partial class PoolManager
{
    private static readonly (string cvar, string value)[] TestCvars =
    {
        // @formatter:off
        (CCVars.DatabaseSynchronous.Name,     "true"),
        (CCVars.DatabaseSqliteDelay.Name,     "0"),
        (CCVars.HolidaysEnabled.Name,         "false"),
        (CCVars.GameMap.Name,                 TestMap),
        (CCVars.AdminLogsQueueSendDelay.Name, "0"),
        (CVars.NetPVS.Name,                   "false"),
        (CCVars.NPCMaxUpdates.Name,           "999999"),
        (CVars.ThreadParallelCount.Name,      "1"),
        (CCVars.GameRoleTimers.Name,          "false"),
        (CCVars.GameRoleWhitelist.Name,       "false"),
        (CCVars.GridFill.Name,                "false"),
        (CCVars.PreloadGrids.Name,            "false"),
        (CCVars.ArrivalsShuttles.Name,        "false"),
        (CCVars.EmergencyShuttleEnabled.Name, "false"),
        (CCVars.ProcgenPreload.Name,          "false"),
        (CCVars.WorldgenEnabled.Name,         "false"),
        (CVars.ReplayClientRecordingEnabled.Name, "false"),
        (CVars.ReplayServerRecordingEnabled.Name, "false"),
        (CCVars.GameDummyTicker.Name, "true"),
        (CCVars.GameLobbyEnabled.Name, "false"),
        (CCVars.ConfigPresetDevelopment.Name, "false"),
        (CCVars.AdminLogsEnabled.Name, "false"),
        (CCVars.AutosaveEnabled.Name, "false"),
        (CVars.NetBufferSize.Name, "0"),
        (CCVars.InteractionRateLimitCount.Name, "9999999"),
        (CCVars.InteractionRateLimitPeriod.Name, "0.1"),
        (CCVars.LavalandEnabled.Name, "false")
    };

    public static async Task SetupCVars(RobustIntegrationTest.IntegrationInstance instance, PoolSettings settings)
    {
        var cfg = instance.ResolveDependency<IConfigurationManager>();
        await instance.WaitPost(() =>
        {
            if (cfg.IsCVarRegistered(CCVars.GameDummyTicker.Name))
                cfg.SetCVar(CCVars.GameDummyTicker, settings.UseDummyTicker);

            if (cfg.IsCVarRegistered(CCVars.GameLobbyEnabled.Name))
                cfg.SetCVar(CCVars.GameLobbyEnabled, settings.InLobby);

            if (cfg.IsCVarRegistered(CVars.NetInterp.Name))
                cfg.SetCVar(CVars.NetInterp, settings.DisableInterpolate);

            if (cfg.IsCVarRegistered(CCVars.GameMap.Name))
                cfg.SetCVar(CCVars.GameMap, settings.Map);

            if (cfg.IsCVarRegistered(CCVars.AdminLogsEnabled.Name))
                cfg.SetCVar(CCVars.AdminLogsEnabled, settings.AdminLogsEnabled);

            if (cfg.IsCVarRegistered(CVars.NetInterp.Name))
                cfg.SetCVar(CVars.NetInterp, !settings.DisableInterpolate);
        });
    }

    private static void SetDefaultCVars(RobustIntegrationTest.IntegrationOptions options)
    {
        foreach (var (cvar, value) in TestCvars)
        {
            options.CVarOverrides[cvar] = value;
        }
    }
}
