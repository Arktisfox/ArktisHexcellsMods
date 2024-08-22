using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace HexcellsMultiplayer.Packets
{
    public enum PacketTypeCodes
    {
        Connect = 0,
        Disconnect = 1,
        PeerList = 2,
        Update = 3,
        GameStart = 4,
        GameQuit = 5,
        GameStartCustom = 6
    }

    public abstract class PacketBase
    {
        public byte PacketTypeCode { get; private set; }

        public virtual void Deserialize(Stream stream)
        {
            var reader = new BinaryReader(stream);
            PacketTypeCode = reader.ReadByte();
        }

        public virtual void Serialize(Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(PacketTypeCode);
        }

        public PacketBase(int typeCode)
        {
            PacketTypeCode = (byte)typeCode;
        }
    }

    public abstract class ClientTaggedPacket : PacketBase
    {
        public Nullable<uint> SenderID { get; private set; }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);
            var reader = new BinaryReader(stream);

            bool haveValue = reader.ReadBoolean();
            if (haveValue)
            {
                SenderID = reader.ReadUInt32();
            }
            else
            {
                SenderID = null;
            }
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);
            var writer = new BinaryWriter(stream);
            writer.Write(SenderID.HasValue);
            if (SenderID.HasValue)
            {
                writer.Write(SenderID.Value);
            }
        }

        public ClientTaggedPacket(int typeCode, uint? senderId) : base(typeCode)
        {
            this.SenderID = senderId;
        }
    }

    public class DisconnectPacket : ClientTaggedPacket
    {
        public DisconnectPacket() : base((int)PacketTypeCodes.Disconnect, null)
        {

        }

        public DisconnectPacket(uint peerId) : base((int)PacketTypeCodes.Disconnect, peerId)
        {
        }
    }

    public class UpdatePacket : ClientTaggedPacket
    {
        public float Progress;
        public float Time;
        public byte State;

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            var reader = new BinaryReader(stream);
            Time = reader.ReadSingle();
            Progress = reader.ReadSingle();
            State = reader.ReadByte();
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            var writer = new BinaryWriter(stream);
            writer.Write(Time);
            writer.Write(Progress);
            writer.Write(State);
        }

        public UpdatePacket() : base((int)PacketTypeCodes.Update, null)
        {

        }

        public UpdatePacket(uint? peerId) : base((int)PacketTypeCodes.Update, peerId)
        {

        }
    }

    public class ConnectPacket  : PacketBase
    {
        public string Name;
        public string ModVersion;

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            var reader = new BinaryReader(stream);
            Name = reader.ReadString();

            // older versions of the mod had no version string
            if(stream.Position != stream.Length)
            {
                ModVersion = reader.ReadString();
            }
            else
            {
                ModVersion = "UNKNOWN";
            }
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            var writer = new BinaryWriter(stream);
            writer.Write(Name);
            writer.Write(ModVersion);
        }

        public ConnectPacket() : base((int)PacketTypeCodes.Connect)
        {
            ModVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }

    public class GameStartPacket : PacketBase
    {
        public int Seed;
        public bool HardMode;

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            var reader = new BinaryReader(stream);
            Seed = reader.ReadInt32();
            HardMode = reader.ReadBoolean();
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            var writer = new BinaryWriter(stream);
            writer.Write(Seed);
            writer.Write(HardMode);
        }

        public GameStartPacket() : base((int)PacketTypeCodes.GameStart)
        {

        }
    }

    public class GameStartCustomPacket : PacketBase
    {
        public string LevelText;
        public int LevelIndex;

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            var reader = new BinaryReader(stream);
            LevelIndex = reader.ReadInt32();
            LevelText = StringCompressor.DecompressString(reader.ReadString());
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            var writer = new BinaryWriter(stream);
            writer.Write(LevelIndex);
            writer.Write(StringCompressor.CompressString(LevelText));
        }

        public GameStartCustomPacket() : base((int)PacketTypeCodes.GameStartCustom)
        {

        }
    }

    public class GameEndPacket : PacketBase
    {
        public GameEndPacket() : base((int)PacketTypeCodes.GameQuit)
        {
        }
    }

    public class PeerListPacket : PacketBase
    {
        private Server server;
        public readonly List<KeyValuePair<uint, string>> Peers = new List<KeyValuePair<uint, string>>();

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            var reader = new BinaryReader(stream);
            int numPeers = reader.ReadInt32();
            for(int i=0; i < numPeers; i++)
            {
                uint peerId = reader.ReadUInt32();
                string peerName = reader.ReadString();
                Peers.Add(new KeyValuePair<uint, string>(peerId, peerName));
            }
        }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            var writer = new BinaryWriter(stream);
            writer.Write(server.Peers.Count);
            foreach(var peer in server.Peers)
            {
                writer.Write(peer.ID);
                writer.Write(peer.Name);
            }
        }

        public PeerListPacket() : base((int)PacketTypeCodes.PeerList)
        {
        }

        public PeerListPacket(Server server) : this()
        {
            this.server = server;
        }
    }
}
