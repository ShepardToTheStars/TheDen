// SPDX-FileCopyrightText: 2021 Acruid <shatter66@gmail.com>
// SPDX-FileCopyrightText: 2021 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Debug <49997488+DebugOk@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server.EUI
{
    /// <summary>
    ///     Base class to implement server-side for an EUI.
    /// </summary>
    /// <remarks>
    ///     An EUI is a system for making a relatively-easy connection between client and server
    ///     for the purposes of UIs.
    /// </remarks>
    /// <remarks>
    ///     An equivalently named class much exist server side for an EUI to work.
    ///     It will be instantiated, opened and closed automatically.
    /// </remarks>
    public abstract class BaseEui
    {
        private bool _isStateDirty = false;

        /// <summary>
        ///     The player that this EUI is open for.
        /// </summary>
        public ICommonSession Player { get; private set; } = default!;
        public bool IsShutDown { get; private set; }
        public EuiManager Manager { get; private set; } = default!;
        public uint Id { get; private set; }

        /// <summary>
        ///     Called when the UI has been opened. Do initializing logic here.
        /// </summary>
        public virtual void Opened()
        {

        }

        /// <summary>
        ///     Called when the UI has been closed.
        /// </summary>
        public virtual void Closed()
        {

        }

        /// <summary>
        ///     Called when a message comes in from the client.
        /// </summary>
        public virtual void HandleMessage(EuiMessageBase msg)
        {
            if (msg is CloseEuiMessage)
                Close();
        }

        /// <summary>
        ///     Mark the current UI state as dirty and queue for an update.
        /// </summary>
        /// <seealso cref="GetNewState"/>
        public void StateDirty()
        {
            if (_isStateDirty)
            {
                return;
            }

            _isStateDirty = true;
            Manager.QueueStateUpdate(this);
        }

        /// <summary>
        ///     Called some time after <see cref="StateDirty"/> has been called
        ///     to get a new UI state that can be sent to the client.
        /// </summary>
        public virtual EuiStateBase GetNewState()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        ///     Send a message to the client-side EUI.
        /// </summary>
        public void SendMessage(EuiMessageBase message)
        {
            var netMgr = IoCManager.Resolve<IServerNetManager>();
            var msg = new MsgEuiMessage();
            msg.Id = Id;
            msg.Message = message;

            netMgr.ServerSendMessage(msg, Player.Channel);
        }

        /// <summary>
        ///     Close the EUI, breaking the connection between client and server.
        /// </summary>
        public void Close()
        {
            Manager.CloseEui(this);
        }

        internal void Shutdown()
        {
            Closed();
            IsShutDown = true;
        }

        internal void DoStateUpdate()
        {
            _isStateDirty = false;

            var state = GetNewState();

            var netMgr = IoCManager.Resolve<IServerNetManager>();
            var msg = new MsgEuiState();
            msg.Id = Id;
            msg.State = state;

            netMgr.ServerSendMessage(msg, Player.Channel);
        }

        internal void Initialize(EuiManager manager, ICommonSession player, uint id)
        {
            Manager = manager;
            Player = player;
            Id = id;
            Opened();
        }
    }
}
