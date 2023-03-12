using ENet;
using HexcellsMultiplayer.Packets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HexcellsMultiplayer
{
    public class Client : NetworkedBase
    {
        public enum State
        {
            Disconnected,
            Connecting,
            Connected,
        }

        public bool Connected => CurrentState == State.Connected;
        public State CurrentState = State.Disconnected;

        /// <summary>
        /// Parameters: seed, hardMode
        /// </summary>
        public Action<int, bool> OnGameStart;
        public Action OnGameEnd;

        public readonly List<HexPeer> Peers = new List<HexPeer>();
        private Peer? hostPeer;

        private string name;

        protected override void HandlePacket(ref Peer sender, ref Packet basePacket, PacketBase packet)
        {
            uint senderID = sender.ID;
            if (packet is PeerListPacket plp)
            {
                Peers.Clear();
                foreach(var peer in plp.Peers)
                {
                    Peers.Add(new HexPeer(peer.Value, peer.Key));
                }
            }
            else if(packet is UpdatePacket up)
            {
                var peer = Peers.FirstOrDefault((x) => x.ID == up.SenderID);
                if(peer != null)
                {
                    peer.Time = up.Time;
                    peer.Progress = up.Progress;
                    peer.CurrentState = (HexPeer.State)up.State;
                }
            }
            else if(packet is DisconnectPacket dp)
            {
                Peers.RemoveAll((x) => x.ID == dp.SenderID);
            }
            else if(packet is GameStartPacket sp && sender.ID == hostPeer.Value.ID)
            {
                OnGameStart?.Invoke(sp.Seed, sp.HardMode);
            }
            else if(packet is GameEndPacket ep && sender.ID == hostPeer.Value.ID)
            {
                OnGameEnd?.Invoke();
            }
        }

        public void SendUpdatePacket(float time, float progress, HexPeer.State state)
        {
            var hostPeerValue = hostPeer.Value;
            SendPacket(ref hostPeerValue, new UpdatePacket(null) { Time = time, Progress = progress, State = (byte)state });
        }

        private void SendConnectPacket()
        {
            var packet = new ConnectPacket()
            {
                Name = name
            };
            var hostPeerValue = hostPeer.Value;
            SendPacket(ref hostPeerValue, packet, PacketFlags.Reliable);
        }

        public void Connect(string hostname, ushort port)
        {
            Address address = new Address
            {
                Port = port
            };

            if (!address.SetHost(hostname))
            {
                throw new Exception("Failed to set host!");
            }

            Disconnect();
            CurrentState = State.Connecting;
            host = new Host();
            host.Create();
            hostPeer = new Peer?(host.Connect(address));            
        }

        public void Disconnect()
        {
            if (hostPeer.HasValue)
            {
                hostPeer.Value.DisconnectNow(default);
                hostPeer = null;
            }
            if(host != null)
            {
                host.Dispose();
                host = null;
            }
            CurrentState = State.Disconnected;
        }

        public override void UpdateLoop()
        {
            if (host == null)
                return;

            host.Flush();
            Event @event;

            if (host.CheckEvents(out @event) <= 0 && host.Service(0, out @event) <= 0)
            {
                return;
            }

            switch (@event.Type)
            {
                case ENet.EventType.None:
                    return;
                case ENet.EventType.Connect:
                    SendConnectPacket();
                    CurrentState = State.Connected;
                    return;
                case ENet.EventType.Disconnect:
                    CurrentState = State.Disconnected;
                    return;
                case ENet.EventType.Receive:
                    {
                        Packet packet = @event.Packet;
                        Peer peer = @event.Peer;
                        HandlePacketRaw(ref peer, ref packet);
                        packet.Dispose();
                        return;
                    }
                case ENet.EventType.Timeout:
                    CurrentState = State.Disconnected;
                    return;
                default:
                    return;
            }

        }

        public Client(string name)
        {
            this.name = name;
        }
    }
}
