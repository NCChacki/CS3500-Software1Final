using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Server
{
    [DataContract(Namespace ="")]
    public class Settings
    {
        [DataMember(Name = "MaxPowerUpDelay")]
        public int maxPowerUpDelay { get; set; }
        [DataMember(Name = "MaxPowerUps")]
        public int maxPowerUps { get; set; }
        [DataMember(Name = "SnakeGrowth")]
        public int snakeGrowth { get; set; }
        [DataMember(Name = "SnakeStartingLength")]
        public int snakeStartingLength { get; set; }
        [DataMember(Name = "SnakeSpeed")]
        public int snakeSpeed { get; set; }
        [DataMember(Name = "MSPerFrame")]
        public int mSPerFrame { get; set; }
        [DataMember(Name = "RespawnRate")]
        public int respawnRate { get; set; }
        [DataMember(Name = "UniverseSize")]
        public int universeSize { get; set; }
        [DataMember(Name = "Walls")]
        public Dictionary<Double, Wall> walls { get; set; }

        public Settings() { }
        public Settings(int maxPowerUpDelay, int maxPowerUps, int snakeGrowth, int snakeStartingLength, int snakeSpeed, int mSPerFrame, int respawnRate, int universeSize, Dictionary<double, Wall> walls) : base()
        {
            this.maxPowerUpDelay = maxPowerUpDelay;
            this.maxPowerUps = maxPowerUps;
            this.snakeGrowth = snakeGrowth;
            this.snakeStartingLength = snakeStartingLength;
            this.snakeSpeed = snakeSpeed;
            this.mSPerFrame = mSPerFrame;
            this.respawnRate = respawnRate;
            this.universeSize = universeSize;
            this.walls = walls;
        }







    }
}
