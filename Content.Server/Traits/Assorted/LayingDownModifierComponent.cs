// SPDX-FileCopyrightText: 2024 Angelo Fallaria <ba.fallaria@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Server.Traits.Assorted;

/// <summary>
///     This is used for traits that modify values related to the Laying Down system.
/// </summary>
[RegisterComponent]
public sealed partial class LayingDownModifierComponent : Component
{
    /// <summary>
    ///     What to multiply the cooldown of laying down and standing up by.
    /// </summary>
    [DataField]
    public float LayingDownCooldownMultiplier = 1f;

    /// <summary>
    ///     What to multiply the speed multiplier when lying down by.
    /// </summary>
    [DataField]
    public float DownedSpeedMultiplierMultiplier = 1f;
}
