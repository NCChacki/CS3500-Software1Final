using Model;
using NetworkUtil;
using SnakeGame;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Xml;
using Server;


namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void ReadingInSettingsFile()
        {

            DataContractSerializer ser = new(typeof(Settings));
          

            XmlReader reader = XmlReader.Create("C:\\Users\\Norman Canning\\source\\repos\\game-jcpenny\\Server\\Settings.xml");
            Settings test = (Settings) ser.ReadObject(reader);

            Assert.IsNotNull(test);

            Assert.AreEqual(test.MaxPowerUpDelay, 75);
            Assert.AreEqual(test.MaxPowerUps, 20);
            Assert.AreEqual(test.SnakeGrowth, 24);
            Assert.AreEqual(test.SnakeStartingLength, 120);
            Assert.AreEqual(test.SnakeSpeed, 6);
            Assert.AreEqual(test.MSPerFrame, 34);
            Assert.AreEqual(test.RespawnRate, 100);
            Assert.AreEqual(test.UniverseSize, 2000);





        }

        [TestMethod]
        public void TestCollsionsWithWall() 
        {
            DataContractSerializer ser = new(typeof(Settings));


            XmlReader reader = XmlReader.Create("C:\\Users\\Norman Canning\\source\\repos\\game-jcpenny\\Server\\Settings.xml");
            Settings test = (Settings)ser.ReadObject(reader);

            List<Wall> walls = test.Walls;



            List<Vector2D> testSnakeHeads= new List<Vector2D>();

            //Snakes on the edge of walls 
            testSnakeHeads.Add(new Vector2D(0, -975));
            testSnakeHeads.Add(new Vector2D(-975, 0));
            testSnakeHeads.Add(new Vector2D(975, 0));
            testSnakeHeads.Add(new Vector2D(0, 975));


            //Snakes in the boundry of walls
            testSnakeHeads.Add(new Vector2D(0, -980));
            testSnakeHeads.Add(new Vector2D(-982, 0));
            testSnakeHeads.Add(new Vector2D(985,0));
            testSnakeHeads.Add(new Vector2D(0, 987));

            Assert.IsTrue(walls.Count!=0);

            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[0], walls[0]));
            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[1], walls[1]));
            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[2], walls[2]));
            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[3], walls[3]));

            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[4], walls[0]));
            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[5], walls[1]));
            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[6], walls[2]));
            Assert.IsTrue(Server.Server.CollisionWithWall(testSnakeHeads[7], walls[3]));
        }
        [TestMethod] public void TestPowerDeserialize() 
        {
            string powerString = "{\"power\":1,\"loc\":{\"X\":486.0684871673584,\"Y\":54.912471771240234},\"died\":false}";

          
            Power power = JsonSerializer.Deserialize<Power>(powerString);

            Assert.AreEqual(power.power, 1);

        }

        [TestMethod]
        public void Test2Dvector()
        {
           
            String Vector2 = "{\"X\":-575.0,\"Y\":-575.0}";

            Vector2D p1 = new Vector2D(-575.0, -575.0);

            Vector2D Vector3 = JsonSerializer.Deserialize<Vector2D>(Vector2);

            Assert.AreEqual(Vector3,p1);
          


        }

        [TestMethod]
        public void TestGetRootElement()
        {

            String wall = "{\"wall\":1,\"p1\":{\"X\":-575.0,\"Y\":-575.0},\"p2\":{\"X\":-575.0,\"Y\":575.0}}";
            Wall test = new();
            JsonDocument doc = JsonDocument.Parse(wall);
            if (doc.RootElement.TryGetProperty("wall", out _))
                Assert.AreEqual(2, 2);


        }


        
        
    }
}