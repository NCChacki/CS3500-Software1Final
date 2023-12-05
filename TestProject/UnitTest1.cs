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
using System.Xml.Linq;


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

            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[0], walls[0].p1, walls[0].p2,25));
            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[1], walls[1].p1, walls[1].p2, 25));
            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[2], walls[2].p1, walls[2].p2, 25));
            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[3], walls[3].p1, walls[3].p2, 25));

            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[4], walls[0].p1, walls[0].p2, 25));
            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[5], walls[1].p1, walls[1].p2, 25));
            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[6], walls[2].p1, walls[2].p2, 25));
            Assert.IsTrue(Server.Server.checkForCollsion(testSnakeHeads[7], walls[3].p1, walls[3].p2, 25));
        }

        [TestMethod] public void TestMovingSnakeInDirection()
        {
            List<Vector2D> body = new List<Vector2D>();
            body.Add(new Vector2D(0, 0));
            body.Add(new Vector2D(0, 100));

            Vector2D dir = new Vector2D(-1, 0);
            Snake testSnake = new Snake(0,"testSnake",body, dir, 0,false,true,false,false);

            testSnake.turned = true;
            
            Vector2D newHead = Server.Server.MoveTowardDirection(testSnake.dir, testSnake.body.Last<Vector2D>(), 6);

            if (testSnake.turned)
            {
                testSnake.body.Add(newHead);
                testSnake.turned = false;
            }
            else
            {
                testSnake.body[testSnake.body.Count - 1] = newHead;
            }

            Assert.AreEqual(3, testSnake.body.Count);
            Assert.AreEqual(-6, testSnake.body.Last<Vector2D>().X);
            Assert.AreEqual(100, testSnake.body.Last<Vector2D>().Y);

        }

        [TestMethod] public void TestTailCatchingUpToNextSegment()
        {
            List<Vector2D> body = new List<Vector2D>();
            body.Add(new Vector2D(0, 99));
            body.Add(new Vector2D(0, 100));
            body.Add(new Vector2D(6, 100));

            Vector2D dir = new Vector2D(1, 0);
            Snake testSnake = new Snake(0, "testSnake", body, dir, 0, false, true, false, false);

            //now move the tail.
            Vector2D tail = testSnake.body[0];
            Vector2D tailDirection = tail - testSnake.body[1];

            //move the tail in the correct direction and reasign the new tail if it catches up with a bend.
            //TODO: Get the speed from the XML again.
            Vector2D newTail = Server.Server.MoveTowardDirection(tailDirection, tail, 6);

            Vector2D newTailAndNextSegmentRelation = newTail + testSnake.body[1];
            newTailAndNextSegmentRelation.Normalize();

            if (newTail == testSnake.body[1] || newTailAndNextSegmentRelation.IsOppositeCardinalDirection(tailDirection))
            {
                testSnake.body.RemoveAt(0);
            }
            else
            {
                testSnake.body[0] = newTail;
            }


            Assert.AreEqual(2,testSnake.body.Count);
            Assert.AreEqual(new Vector2D(0, 100), testSnake.body[0]);
            Assert.AreEqual(new Vector2D(6, 100), testSnake.body[1]);
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

        [TestMethod]
        public void testGrowSnakeWithMultipleTurns()
        {
            List<Vector2D> body = new List<Vector2D>();
            body.Add(new Vector2D(0, 94));
            body.Add(new Vector2D(0, 100));

            
        

            Vector2D dir = new Vector2D(0, 1);
            Snake testSnake = new Snake(0, "testSnake", body, dir, 0, false, true, false, false);

            Server.Server.UpdateSnake(testSnake);

            Server.Server.UpdateSnake(testSnake);

            testSnake.dir = new Vector2D(1, 0);
            testSnake.turned = true;
            Server.Server.UpdateSnake(testSnake);

            testSnake.dir = new Vector2D(0, -1);
            testSnake.turned = false;   

            Server.Server.UpdateSnake(testSnake);
            Server.Server.UpdateSnake(testSnake);
            Server.Server.UpdateSnake(testSnake);



            int i = 1;





        }


        
        
    }
}