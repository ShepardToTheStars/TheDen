// SPDX-FileCopyrightText: 2022 Flipp Syder <76629141+vulppine@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.StationRecords;

/// <summary>
/// Station record keys. These should be stored somewhere,
/// preferably within an ID card.
/// This refers to both the id and station. This is suitable for an access reader field etc,
/// but when you already know the station just store the id itself.
/// </summary>
public readonly struct StationRecordKey : IEquatable<StationRecordKey>
{
    [DataField]
    public readonly uint Id;

    [DataField("station")]
    public readonly EntityUid OriginStation;

    public static StationRecordKey Invalid = default;

    public StationRecordKey(uint id, EntityUid originStation)
    {
        Id = id;
        OriginStation = originStation;
    }

    public bool Equals(StationRecordKey other)
    {
        return Id == other.Id && OriginStation.Id == other.OriginStation.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is StationRecordKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, OriginStation);
    }

    public bool IsValid() => OriginStation.IsValid();
}
