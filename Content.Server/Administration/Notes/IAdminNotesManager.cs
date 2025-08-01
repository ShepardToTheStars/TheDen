// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 SimpleStation14 <130339894+SimpleStation14@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared.Administration.Notes;
using Content.Shared.Database;
using Robust.Shared.Player;

namespace Content.Server.Administration.Notes;

public interface IAdminNotesManager
{
    event Action<SharedAdminNote>? NoteAdded;
    event Action<SharedAdminNote>? NoteModified;
    event Action<SharedAdminNote>? NoteDeleted;

    bool CanCreate(ICommonSession admin);
    bool CanDelete(ICommonSession admin);
    bool CanEdit(ICommonSession admin);
    bool CanView(ICommonSession admin);
    Task OpenEui(ICommonSession admin, Guid notedPlayer);
    Task OpenUserNotesEui(ICommonSession player);
    Task AddAdminRemark(ICommonSession createdBy, Guid player, NoteType type, string message, NoteSeverity? severity, bool secret, DateTime? expiryTime);
    Task DeleteAdminRemark(int noteId, NoteType type, ICommonSession deletedBy);
    Task ModifyAdminRemark(int noteId, NoteType type, ICommonSession editedBy, string message, NoteSeverity? severity, bool secret, DateTime? expiryTime);
    /// <summary>
    /// Queries the database and retrieves all notes, secret and visible
    /// </summary>
    /// <param name="player">Desired player's <see cref="Guid"/></param>
    /// <returns>ALL non-deleted notes, secret or not</returns>
    Task<List<IAdminRemarksRecord>> GetAllAdminRemarks(Guid player);
    /// <summary>
    /// Queries the database and retrieves the notes a player should see
    /// </summary>
    /// <param name="player">Desired player's <see cref="Guid"/></param>
    /// <returns>All player-visible notes</returns>
    Task<List<IAdminRemarksRecord>> GetVisibleRemarks(Guid player);
    /// <summary>
    /// Queries the database and retrieves watchlists that may have been placed on the player
    /// </summary>
    /// <param name="player">Desired player's <see cref="Guid"/></param>
    /// <returns>Active watchlists</returns>
    Task<List<AdminWatchlistRecord>> GetActiveWatchlists(Guid player);
    /// <summary>
    /// Queries the database and retrieves new messages a player has gotten
    /// </summary>
    /// <param name="player">Desired player's <see cref="Guid"/></param>
    /// <returns>All unread messages</returns>
    Task<List<AdminMessageRecord>> GetNewMessages(Guid player);

    /// <summary>
    /// Mark an admin message as being seen by the target player.
    /// </summary>
    /// <param name="id">The database ID of the admin message.</param>
    /// <param name="dismissedToo">
    /// If true, the message is "permanently dismissed" and will not be shown to the player again when they join.
    /// </param>
    Task MarkMessageAsSeen(int id, bool dismissedToo);
}
