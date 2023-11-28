//World Object Class for Snake Game. Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023
using System.Numerics;

namespace Model
{


    public class World
    {
        /// <summary>
        /// Dictionary of all players in the current world. The name of the player the snake repersents is the key with 
        /// the respective snake as the value. 
        /// </summary>
        public Dictionary<string, Snake> Players;
        /// <summary>
        /// Dictionary of all powerups in the current world. The ID of the powerup is the key with the repsective powerUp
        /// as the value.
        /// </summary>
        public Dictionary<int, Power> Powerups;
        /// <summary>
        /// Dictionary of all Walls in the current world. The ID of the wall are the key with the repsective wall
        /// as the value.
        /// </summary>
        public Dictionary<int, Wall> Walls;

        /// <summary>
        /// Player ID of the player
        /// </summary>
        public int playerID
        {  get; private set; }
        /// <summary>
        /// Size of the world
        /// </summary>
        public int size
        { get; private set; }


    /// <summary>
    /// Construtor of a world object, takes in the world size and the playerID of the current player.
    /// </summary>
    /// <param name="size"></param>
    /// <param name="playerID"></param>
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