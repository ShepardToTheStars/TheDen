// SPDX-FileCopyrightText: 2023 Nemanja <98561806+emogarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

/// <summary>
/// This is a station event that randomly removes some records from the station record database.
/// </summary>
[RegisterComponent]
[Access(typeof(ClericalErrorRule))]
public sealed partial class ClericalErrorRuleComponent : Component
{
    /// <summary>
    /// The minimum percentage number of records to remove from the station.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MinToRemove = 0.0025f;

    /// <summary>
    /// The maximum percentage number of records to remove from the station.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaxToRemove = 0.1f;
}
