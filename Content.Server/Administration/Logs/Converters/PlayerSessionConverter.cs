// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using Robust.Shared.Player;

namespace Content.Server.Administration.Logs.Converters;

[AdminLogConverter]
public sealed class PlayerSessionConverter : AdminLogConverter<SerializablePlayer>
{
    // System.Text.Json actually keeps hold of your JsonSerializerOption instances in a cache on .NET 7.
    // Use a weak reference to avoid holding server instances live too long in integration tests.
    private WeakReference<IEntityManager> _entityManager = default!;

    public override void Init(IDependencyCollection dependencies)
    {
        _entityManager = new WeakReference<IEntityManager>(dependencies.Resolve<IEntityManager>());
    }

    public override void Write(Utf8JsonWriter writer, SerializablePlayer value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        if (value.Player.AttachedEntity is {Valid: true} playerEntity)
        {
            if (!_entityManager.TryGetTarget(out var entityManager))
                throw new InvalidOperationException("EntityManager got garbage collected!");

            writer.WriteNumber("id", (int) value.Player.AttachedEntity);
            writer.WriteString("name", entityManager.GetComponent<MetaDataComponent>(playerEntity).EntityName);
        }

        writer.WriteString("player", value.Player.UserId.UserId);

        writer.WriteEndObject();
    }
}

public readonly struct SerializablePlayer
{
    public readonly ICommonSession Player;

    public SerializablePlayer(ICommonSession player)
    {
        Player = player;
    }
}
