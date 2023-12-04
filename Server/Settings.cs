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
    [DataContract(Name="Settings",Namespace ="")]
    public class Settings
    {

        [DataMember(Name = "MSPerFrame")]
        public int MSPerFrame { get; set; }

        [DataMember(Name = "MaxPowerUpDelay")]
        public int MaxPowerUpDelay { get; set; }

        [DataMember(Name = "MaxPowerUps")]
        public int MaxPowerUps { get; set; }

        
        [DataMember(Name = "RespawnRate")]
        public int RespawnRate { get; set; }



        [DataMember(Name = "SnakeGrowth")]
        public int SnakeGrowth { get; set; }

        [DataMember(Name = "SnakeSpeed")]
        public int SnakeSpeed { get; set; }

        [DataMember(Name = "SnakeStartingLength")]
        public int SnakeStartingLength { get; set; }

        
      
        [DataMember(Name = "UniverseSize")]
        public int UniverseSize { get; set; }

        [DataMember(Name = "Walls")]
        public List<Wall> Walls { get; set; }


        public Settings() { }
        public Settings(int MaxPowerUpDelay, int MaxPowerUps, int MSPerFrame, int RespawnRate, int SnakeGrowth, int SnakeSpeed, int SnakeStartingLength, int UniverseSize, List<Wall> Walls) : base()
        {
            this.MaxPowerUpDelay = MaxPowerUpDelay;
            this.MaxPowerUps = MaxPowerUps;
            this.MSPerFrame = MSPerFrame;
            this.SnakeGrowth = SnakeGrowth;
            this.SnakeStartingLength = SnakeStartingLength;
            this.SnakeSpeed = SnakeSpeed;
           
            this.RespawnRate = RespawnRate;
            this.UniverseSize = UniverseSize;
            this.Walls = Walls;
        }







    }
}
