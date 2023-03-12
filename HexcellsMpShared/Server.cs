using ENet;
using System.Collections.Generic;
using HexcellsMultiplayer.Packets;

namespace HexcellsMultiplayer
{
    public class Server : NetworkedBase
    {
        public readonly List<HexPeer> Peers = new List<HexPeer>();
		const int MAX_PLAYERS = 6;

		public void StartGame(int seed, bool hard)
        {
			BroadcastPacket(new GameStartPacket() { HardMode = hard, Seed = seed }, PacketFlags.Reliable);
		}

		public void StopGame()
        {
			BroadcastPacket(new GameEndPacket(), PacketFlags.Reliable);
        }

        protected override void HandlePacket(ref Peer sender, ref Packet basePacket, PacketBase packet)
        {
            if(packet is ConnectPacket ctp)
            {
                Peers.Add(new HexPeer(ctp.Name, sender));
                BroadcastPacket(new PeerListPacket(this), PacketFlags.Reliable); // send everyone the new peer list
            }
            else if(packet is DisconnectPacket dcp)
            {
                uint senderId = sender.ID;
                Peers.RemoveAll(x => x.ID == senderId);
                BroadcastPacket(new DisconnectPacket(sender.ID)); // rebroadcast the disconnection packet to all clients
            }
			else if(packet is UpdatePacket up)
            {
				BroadcastPacket(new UpdatePacket(sender.ID) { Progress = up.Progress, Time = up.Time, State = up.State }); // rebroadcast updates
            }
        }

        public void Start(ushort port)
        {
            Address address = new Address
            {
                Port = port
            };

            host = new Host();
            host.Create(address, MAX_PLAYERS);
        }

		public void Stop()
        {
			if(host != null)
            {
				host.Dispose();
				host = null;
            }
        }

		private void SendDisconnectPacket(uint peerId)
        {
			var packet = new DisconnectPacket(peerId);
			BroadcastPacket(packet);
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
					return;
				case ENet.EventType.Disconnect:
					Peers.RemoveAll(x => x.ID == @event.Peer.ID);
					SendDisconnectPacket(@event.Peer.ID);
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
					Peers.RemoveAll(x => x.ID == @event.Peer.ID);
					SendDisconnectPacket(@event.Peer.ID);
					return;
				default:
					return;
			}
			
		}
    }
}
