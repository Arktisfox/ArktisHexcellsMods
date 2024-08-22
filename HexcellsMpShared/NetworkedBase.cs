using ENet;
using HexcellsMultiplayer.Packets;
using System;
using System.Collections.Generic;
using System.IO;

namespace HexcellsMultiplayer
{
    public abstract class NetworkedBase : IDisposable
    {
        protected byte channelID = 0;
        private byte[] packetBuf = new byte[8192];

        protected Host host;

        public void Dispose()
        {
            if(host != null)
            {
                host.Dispose();
            }
        }

        private static Dictionary<byte, Type> typeCodeMapping = new Dictionary<byte, Type>()
        {
            {(byte)PacketTypeCodes.Connect, typeof(ConnectPacket)},
            {(byte)PacketTypeCodes.Disconnect, typeof(DisconnectPacket)},
            {(byte)PacketTypeCodes.GameQuit, typeof(GameEndPacket)},
            {(byte)PacketTypeCodes.GameStart, typeof(GameStartPacket)},
            {(byte)PacketTypeCodes.PeerList, typeof(PeerListPacket)},
            {(byte)PacketTypeCodes.Update, typeof(UpdatePacket)},
            {(byte)PacketTypeCodes.GameStartCustom, typeof(GameStartCustomPacket)},
        };

        protected virtual void HandlePacket(ref Peer sender, ref Packet basePacket, PacketBase packet) 
        {
        }

        protected void HandlePacketRaw(ref Peer sender, ref Packet packet)
        {
            packet.CopyTo(packetBuf);
            byte packetTypeCode = packetBuf[0];
            if (typeCodeMapping.TryGetValue(packetTypeCode, out Type packetType))
            {
                var packetBase = (PacketBase)Activator.CreateInstance(packetType);
                packetBase.Deserialize(new MemoryStream(packetBuf));
                HandlePacket(ref sender, ref packet, packetBase);
            }
            else
            {
                throw new Exception($"HandlePacketRaw: I don't understand how to handle packet type {packetTypeCode}");
            }
        }

        protected void BroadcastPacket(PacketBase packet, PacketFlags flags = PacketFlags.None)
        {
            using (var ms = new MemoryStream())
            {
                packet.Serialize(ms);
                byte[] data = ms.ToArray();

                Packet enetPacket = default;
                enetPacket.Create(data, flags);
                host.Broadcast(channelID, ref enetPacket);
            }
        }

        protected void SendPacket(ref Peer peer, PacketBase packet, PacketFlags flags = PacketFlags.None)
        {
            using (var ms = new MemoryStream())
            {
                packet.Serialize(ms);
                byte[] data = ms.ToArray();

                Packet enetPacket = default;
                enetPacket.Create(data, flags);
                peer.Send(channelID, ref enetPacket);
            }
        }

        protected void KickPeer(ref Peer peer, uint reason)
        {
            peer.Disconnect(reason);
        }

        public abstract void UpdateLoop();
    }
}
