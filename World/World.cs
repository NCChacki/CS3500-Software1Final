using System.Numerics;

namespace Model
{


    public class World
    {
        public Dictionary<int, Snake> Players;
        public Dictionary<int, Power> Powerups;
        public Dictionary<int, Wall> Walls; 
        public int Size
        { get; private set; }


        

        public World(int size) 
        {
            this.Players = new Dictionary<int, Snake>();
            this.Powerups = new Dictionary<int, Power>();
            this.Walls = new Dictionary<int, Wall>();
            
            this.Size = size;
        }

    }
}