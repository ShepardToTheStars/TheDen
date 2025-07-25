// SPDX-FileCopyrightText: 2020 ColdAutumnRain
// SPDX-FileCopyrightText: 2021 20kdc
// SPDX-FileCopyrightText: 2021 Acruid
// SPDX-FileCopyrightText: 2021 Paul Ritter
// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto
// SPDX-FileCopyrightText: 2021 Visne
// SPDX-FileCopyrightText: 2021 collinlunn
// SPDX-FileCopyrightText: 2021 moonheart08
// SPDX-FileCopyrightText: 2021 py01
// SPDX-FileCopyrightText: 2022 Leon Friedrich
// SPDX-FileCopyrightText: 2022 metalgearsloth
// SPDX-FileCopyrightText: 2022 mirrorcult
// SPDX-FileCopyrightText: 2022 wrexbe
// SPDX-FileCopyrightText: 2023 DrSmugleaf
// SPDX-FileCopyrightText: 2025 ArtisticRoomba
// SPDX-FileCopyrightText: 2025 Solaris
// SPDX-FileCopyrightText: 2025 chromiumboy
// SPDX-FileCopyrightText: 2025 sleepyyapril
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.NodeGroups;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Utility;

namespace Content.Server.NodeContainer.Nodes
{
    /// <summary>
    ///     Connects with other <see cref="PipeNode"/>s whose <see cref="PipeDirection"/>
    ///     and <see cref="CurrentPipeLayer"/> correctly correspond.
    /// </summary>
    [DataDefinition]
    [Virtual]
    public partial class PipeNode : Node, IGasMixtureHolder, IRotatableNode
    {
        /// <summary>
        ///     The directions in which this pipe can connect to other pipes around it.
        /// </summary>
        [DataField("pipeDirection")]
        public PipeDirection OriginalPipeDirection;

        /// <summary>
        ///     The *current* layer to which the pipe node is assigned.
        /// </summary>
        [DataField("pipeLayer")]
        public AtmosPipeLayer CurrentPipeLayer = AtmosPipeLayer.Primary;

        /// <summary>
        ///     The *current* pipe directions (accounting for rotation)
        ///     Used to check if this pipe can connect to another pipe in a given direction.
        /// </summary>
        public PipeDirection CurrentPipeDirection { get; private set; }

        private HashSet<PipeNode>? _alwaysReachable;

        public void AddAlwaysReachable(PipeNode pipeNode)
        {
            if (pipeNode.NodeGroupID != NodeGroupID) return;
            _alwaysReachable ??= new();
            _alwaysReachable.Add(pipeNode);

            if (NodeGroup != null)
                EntitySystem.Get<NodeGroupSystem>().QueueRemakeGroup((BaseNodeGroup) NodeGroup);
        }

        public void RemoveAlwaysReachable(PipeNode pipeNode)
        {
            if (_alwaysReachable == null) return;

            _alwaysReachable.Remove(pipeNode);

            if (NodeGroup != null)
                EntitySystem.Get<NodeGroupSystem>().QueueRemakeGroup((BaseNodeGroup) NodeGroup);
        }

        /// <summary>
        ///     Whether this node can connect to others or not.
        /// </summary>
        [ViewVariables(VVAccess.ReadWrite)]
        public bool ConnectionsEnabled
        {
            get => _connectionsEnabled;
            set
            {
                _connectionsEnabled = value;

                if (NodeGroup != null)
                    EntitySystem.Get<NodeGroupSystem>().QueueRemakeGroup((BaseNodeGroup) NodeGroup);
            }
        }

        [DataField("connectionsEnabled")]
        private bool _connectionsEnabled = true;

        public override bool Connectable(IEntityManager entMan, TransformComponent? xform = null)
        {
            return _connectionsEnabled && base.Connectable(entMan, xform);
        }

        [DataField("rotationsEnabled")]
        public bool RotationsEnabled { get; set; } = true;

        /// <summary>
        ///     The <see cref="IPipeNet"/> this pipe is a part of.
        /// </summary>
        [ViewVariables]
        private IPipeNet? PipeNet => (IPipeNet?) NodeGroup;

        /// <summary>
        ///     The gases in this pipe.
        /// </summary>
        [ViewVariables]
        public GasMixture Air
        {
            get => PipeNet?.Air ?? GasMixture.SpaceGas;
            set
            {
                DebugTools.Assert(PipeNet != null);
                PipeNet!.Air = value;
            }
        }

        [DataField("volume")]
        public float Volume { get; set; } = DefaultVolume;

        private const float DefaultVolume = 200f;

        public override void Initialize(EntityUid owner, IEntityManager entMan)
        {
            base.Initialize(owner, entMan);

            if (!RotationsEnabled)
                return;

            var xform = entMan.GetComponent<TransformComponent>(owner);
            CurrentPipeDirection = OriginalPipeDirection.RotatePipeDirection(xform.LocalRotation);
        }

        bool IRotatableNode.RotateNode(in MoveEvent ev)
        {
            if (OriginalPipeDirection == PipeDirection.Fourway)
                return false;

            // update valid pipe direction
            if (!RotationsEnabled)
            {
                if (CurrentPipeDirection == OriginalPipeDirection)
                    return false;

                CurrentPipeDirection = OriginalPipeDirection;
                return true;
            }

            var oldDirection = CurrentPipeDirection;
            CurrentPipeDirection = OriginalPipeDirection.RotatePipeDirection(ev.NewRotation);
            return oldDirection != CurrentPipeDirection;
        }

        public override void OnAnchorStateChanged(IEntityManager entityManager, bool anchored)
        {
            if (!anchored)
                return;

            // update valid pipe directions

            if (!RotationsEnabled)
            {
                CurrentPipeDirection = OriginalPipeDirection;
                return;
            }

            var xform = entityManager.GetComponent<TransformComponent>(Owner);
            CurrentPipeDirection = OriginalPipeDirection.RotatePipeDirection(xform.LocalRotation);
        }

        public override IEnumerable<Node> GetReachableNodes(TransformComponent xform,
            EntityQuery<NodeContainerComponent> nodeQuery,
            EntityQuery<TransformComponent> xformQuery,
            MapGridComponent? grid,
            IEntityManager entMan)
        {
            if (_alwaysReachable != null)
            {
                var remQ = new RemQueue<PipeNode>();
                foreach (var pipe in _alwaysReachable)
                {
                    if (pipe.Deleting)
                    {
                        remQ.Add(pipe);
                    }
                    yield return pipe;
                }

                foreach (var pipe in remQ)
                {
                    _alwaysReachable.Remove(pipe);
                }
            }

            if (!xform.Anchored || grid == null)
                yield break;

            var pos = grid.TileIndicesFor(xform.Coordinates);

            for (var i = 0; i < PipeDirectionHelpers.PipeDirections; i++)
            {
                var pipeDir = (PipeDirection) (1 << i);

                if (!CurrentPipeDirection.HasDirection(pipeDir))
                    continue;

                foreach (var pipe in LinkableNodesInDirection(pos, pipeDir, grid, nodeQuery))
                {
                    yield return pipe;
                }
            }
        }

        /// <summary>
        ///     Gets the pipes that can connect to us from entities on the tile or adjacent in a direction.
        /// </summary>
        private IEnumerable<PipeNode> LinkableNodesInDirection(Vector2i pos, PipeDirection pipeDir, MapGridComponent grid,
            EntityQuery<NodeContainerComponent> nodeQuery)
        {
            foreach (var pipe in PipesInDirection(pos, pipeDir, grid, nodeQuery))
            {
                if (pipe.NodeGroupID == NodeGroupID
                    && pipe.CurrentPipeLayer == CurrentPipeLayer
                    && pipe.CurrentPipeDirection.HasDirection(pipeDir.GetOpposite()))
                {
                    yield return pipe;
                }
            }
        }

        /// <summary>
        ///     Gets the pipes from entities on the tile adjacent in a direction.
        /// </summary>
        protected IEnumerable<PipeNode> PipesInDirection(Vector2i pos, PipeDirection pipeDir, MapGridComponent grid,
            EntityQuery<NodeContainerComponent> nodeQuery)
        {
            var offsetPos = pos.Offset(pipeDir.ToDirection());

            foreach (var entity in grid.GetAnchoredEntities(offsetPos))
            {
                if (!nodeQuery.TryGetComponent(entity, out var container))
                    continue;

                foreach (var node in container.Nodes.Values)
                {
                    if (node is PipeNode pipe)
                        yield return pipe;
                }
            }
        }
    }
}
