// SPDX-FileCopyrightText: 2023 Ed <96445749+theshued@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Errant <35878406+Errant-4@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Added to mind role entities to tag that they are a thief.
/// </summary>
[RegisterComponent]
public sealed partial class ThiefRoleComponent : BaseMindRoleComponent
{
}
