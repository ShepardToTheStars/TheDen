// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Galactic Chimp <63882831+GalacticChimp@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2021 pointer-to-null <91910481+pointer-to-null@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Jesse Rougeau <jmaster9999@gmail.com>
// SPDX-FileCopyrightText: 2022 Jessica M <jessica@jessicamaybe.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 Mervill <mervills.email@gmail.com>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Radosvik <65792927+Radosvik@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 moonheart08 <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 theashtronaut <112137107+theashtronaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kot <1192090+koteq@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 freeze2222 <46649391+freeze2222@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 12rabbits <53499656+12rabbits@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DoutorWhite <68350815+DoutorWhite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Fildrance <fildrance@gmail.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 Eris <eris@erisws.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Shared.GameTicking;
using Content.Server.Station.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using System.Text;

namespace Content.Server.GameTicking
{
    public sealed partial class GameTicker
    {
        [ViewVariables]
        private readonly Dictionary<NetUserId, PlayerGameStatus> _playerGameStatuses = new();

        [ViewVariables]
        private TimeSpan _roundStartTime;

        /// <summary>
        /// How long before RoundStartTime do we load maps.
        /// </summary>
        [ViewVariables]
        public TimeSpan RoundPreloadTime { get; } = TimeSpan.FromSeconds(20);

        [ViewVariables]
        private TimeSpan _pauseTime;

        [ViewVariables]
        public new bool Paused { get; set; }

        [ViewVariables]
        private bool _roundStartCountdownHasNotStartedYetDueToNoPlayers;

        /// <summary>
        /// The game status of a players user Id. May contain disconnected players
        /// </summary>
        public IReadOnlyDictionary<NetUserId, PlayerGameStatus> PlayerGameStatuses => _playerGameStatuses;

        public void UpdateInfoText()
        {
            RaiseNetworkEvent(GetInfoMsg(), Filter.Empty().AddPlayers(_playerManager.NetworkedSessions));
            RaiseNetworkEvent(GetInGameInfoMsg(), Filter.Empty().AddPlayers(_playerManager.NetworkedSessions));
        }

        private string GetInfoText(bool isInGameInfo = false)
        {
            var preset = CurrentPreset ?? Preset;
            if (preset == null)
            {
                return string.Empty;
            }

            var playerCount = $"{_playerManager.PlayerCount}";
            var readyCount = _playerGameStatuses.Values.Count(x => x == PlayerGameStatus.ReadyToPlay);

            var stationNames = new StringBuilder();
            var query =
                EntityQueryEnumerator<StationJobsComponent, StationSpawningComponent, MetaDataComponent>();

            var foundOne = false;

            while (query.MoveNext(out _, out _, out var meta))
            {
                foundOne = true;
                if (stationNames.Length > 0)
                        stationNames.Append('\n');

                stationNames.Append(meta.EntityName);
            }

            if (!foundOne)
            {
                stationNames.Append(_gameMapManager.GetSelectedMap()?.MapName ??
                                    Loc.GetString("game-ticker-no-map-selected"));
            }

            var gmTitle = Loc.GetString(preset.ModeTitle);
            var desc = Loc.GetString(preset.Description);
            var infoText = RunLevel == GameRunLevel.PreRoundLobby
                    ? "game-ticker-get-info-preround-text"
                    : "game-ticker-get-info-text";

            if (isInGameInfo)
                infoText = "game-ticker-get-ingame-info-text";

            return Loc.GetString(
                infoText,
                ("roundId", RoundId),
                ("playerCount", playerCount),
                ("readyCount", readyCount),
                ("mapName", stationNames.ToString()),
                ("gmTitle", gmTitle),
                ("desc", desc));
        }

        private TickerConnectionStatusEvent GetConnectionStatusMsg()
        {
            return new TickerConnectionStatusEvent(RoundStartTimeSpan);
        }

        private TickerLobbyStatusEvent GetStatusMsg(ICommonSession session)
        {
            _playerGameStatuses.TryGetValue(session.UserId, out var status);
            return new TickerLobbyStatusEvent(RunLevel != GameRunLevel.PreRoundLobby, LobbyBackground, status == PlayerGameStatus.ReadyToPlay, _roundStartTime, RoundPreloadTime, RoundStartTimeSpan, Paused);
        }

        private void SendStatusToAll()
        {
            foreach (var player in _playerManager.Sessions)
            {
                RaiseNetworkEvent(GetStatusMsg(player), player.Channel);
            }
        }

        private TickerLobbyInfoEvent GetInfoMsg()
        {
            return new (GetInfoText());
        }

        private void UpdateLateJoinStatus()
        {
            RaiseNetworkEvent(new TickerLateJoinStatusEvent(DisallowLateJoin));
        }

        private TickerInGameInfoEvent GetInGameInfoMsg()
        {
            return new (GetInfoText(true));
        }
        public bool PauseStart(bool pause = true)
        {
            if (Paused == pause)
            {
                return false;
            }

            Paused = pause;

            if (pause)
            {
                _pauseTime = _gameTiming.CurTime;
            }
            else if (_pauseTime != default)
            {
                _roundStartTime += _gameTiming.CurTime - _pauseTime;
            }

            RaiseNetworkEvent(new TickerLobbyCountdownEvent(_roundStartTime, Paused));

            _chatManager.DispatchServerAnnouncement(Loc.GetString(Paused
                ? "game-ticker-pause-start"
                : "game-ticker-pause-start-resumed"));

            return true;
        }

        public bool TogglePause()
        {
            PauseStart(!Paused);
            return Paused;
        }

        public void ToggleReadyAll(bool ready)
        {
            var status = ready ? PlayerGameStatus.ReadyToPlay : PlayerGameStatus.NotReadyToPlay;
            foreach (var playerUserId in _playerGameStatuses.Keys)
            {
                _playerGameStatuses[playerUserId] = status;
                if (!_playerManager.TryGetSessionById(playerUserId, out var playerSession))
                    continue;
                RaiseNetworkEvent(GetStatusMsg(playerSession), playerSession.Channel);
                RaiseLocalEvent(new PlayerToggleReadyEvent(playerSession));
            }
        }

        public void ToggleReady(ICommonSession player, bool ready)
        {
            if (!_playerGameStatuses.ContainsKey(player.UserId))
                return;

            if (!_userDb.IsLoadComplete(player))
                return;

            if (RunLevel != GameRunLevel.PreRoundLobby)
            {
                return;
            }

            var status = ready ? PlayerGameStatus.ReadyToPlay : PlayerGameStatus.NotReadyToPlay;
            if (_playerGameStatuses[player.UserId] == status)
            {
                return;
            }

            _playerGameStatuses[player.UserId] = ready ? PlayerGameStatus.ReadyToPlay : PlayerGameStatus.NotReadyToPlay;
            RaiseNetworkEvent(GetStatusMsg(player), player.Channel);
            RaiseLocalEvent(new PlayerToggleReadyEvent(player));
            // update server info to reflect new ready count
            UpdateInfoText();
        }

        public bool UserHasJoinedGame(ICommonSession session)
            => UserHasJoinedGame(session.UserId);

        public bool UserHasJoinedGame(NetUserId userId)
            => PlayerGameStatuses.TryGetValue(userId, out var status) && status == PlayerGameStatus.JoinedGame;
    }

    public sealed class PlayerToggleReadyEvent : EntityEventArgs
    {
        public readonly ICommonSession PlayerSession;

        public PlayerToggleReadyEvent(ICommonSession playerSession)
        {
            PlayerSession = playerSession;
        }
    }
}
