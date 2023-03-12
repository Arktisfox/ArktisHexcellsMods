using ENet;
using System;

namespace HexcellsMultiplayer
{   
    public class HexPeer
    {
        public enum State
        {
            InMenu,
            InPuzzle,
            InPuzzleComplete
        }

        // ENet Part
        public uint ID { get; private set; }
        public Peer? Peer { get; private set; }

        // Hexcells part
        private DateTime timeSetTime;
        private float timeBackingField;

        public State CurrentState = State.InMenu;
        public string Name;
        public float Progress;
        public float Time
        {
            get {
                return timeBackingField;
            }
            set
            {
                timeBackingField = value;
                timeSetTime = DateTime.Now;
            }
        }

        public float SmoothedTime => (float)((DateTime.Now - timeSetTime).TotalSeconds + timeBackingField);

        public HexPeer(string name, Peer peer)
        {
            Name = name;
            Peer = peer;
            ID = peer.ID;
        }

        public HexPeer(string name, uint id)
        {
            Name = name;
            ID = id;
        }
    }
}
