using System.Numerics;

namespace Model
{


    public class World
    {
        public Dictionary<string, Snake> Players;
        public Dictionary<int, Power> Powerups;
        public Dictionary<int, Wall> Walls;

        public int playerID
        {  get; private set; }
        public int size
        { get; private set; }


   
        public World(int size, int playerID) 
        {
            this.Players = new Dictionary<string, Snake>();
            this.Powerups = new Dictionary<int, Power>();
            this.Walls = new Dictionary<int, Wall>();
            
            this.playerID = playerID;
            this.size = size;
        }

    }
}