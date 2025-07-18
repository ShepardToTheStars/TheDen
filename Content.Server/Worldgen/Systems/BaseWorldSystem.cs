// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Server.Worldgen.Components;
using JetBrains.Annotations;

namespace Content.Server.Worldgen.Systems;

/// <summary>
///     This provides some additional functions for world generation systems.
///     Exists primarily for convenience and to avoid code duplication.
/// </summary>
[PublicAPI]
public abstract class BaseWorldSystem : EntitySystem
{
    [Dependency] private readonly WorldControllerSystem _worldController = default!;

    /// <summary>
    ///     Gets a chunk's coordinates in chunk space as an integer value.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="xform"></param>
    /// <returns>Chunk space coordinates</returns>
    [Pure]
    public Vector2i GetChunkCoords(EntityUid ent, TransformComponent? xform = null)
    {
        if (!Resolve(ent, ref xform))
            throw new Exception("Failed to resolve transform, somehow.");

        return WorldGen.WorldToChunkCoords(xform.WorldPosition).Floored();
    }

    /// <summary>
    ///     Gets a chunk's coordinates in chunk space as a floating point value.
    /// </summary>
    /// <param name="ent"></param>
    /// <param name="xform"></param>
    /// <returns>Chunk space coordinates</returns>
    [Pure]
    public Vector2 GetFloatingChunkCoords(EntityUid ent, TransformComponent? xform = null)
    {
        if (!Resolve(ent, ref xform))
            throw new Exception("Failed to resolve transform, somehow.");

        return WorldGen.WorldToChunkCoords(xform.WorldPosition);
    }

    /// <summary>
    ///     Attempts to get a chunk, creating it if it doesn't exist.
    /// </summary>
    /// <param name="chunk">Chunk coordinates to get the chunk entity for.</param>
    /// <param name="map">Map the chunk is in.</param>
    /// <param name="controller">The controller this chunk belongs to.</param>
    /// <returns>A chunk, if available.</returns>
    [Pure]
    public EntityUid? GetOrCreateChunk(Vector2i chunk, EntityUid map, WorldControllerComponent? controller = null)
    {
        return _worldController.GetOrCreateChunk(chunk, map, controller);
    }
}

