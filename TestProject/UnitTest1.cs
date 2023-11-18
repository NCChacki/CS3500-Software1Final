using Model;
using NetworkUtil;
using SnakeGame;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;


namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {

            //Json document look at on canvas.

            String wall = "{\"wall\":1,\"p1\":{\"x\":-575.0,\"y\":-575.0},\"p2\":{\"x\":-575.0,\"y\":575.0}}";

            Vector2D p1 = new Vector2D(-575.0, -575.0);
            Vector2D p2 = new Vector2D(-575.0, 575.0);



            Wall? wall1 = JsonSerializer.Deserialize<Wall>(wall);

            Assert.AreEqual(wall1.wall, 1);
            Assert.AreEqual(wall1.p1, p1);
            Assert.AreEqual(wall1.p2, p2);



        }

        [TestMethod]
        public void Test2Dvector()
        {
           
            String Vector2 = "{\"x\":-575.0,\"y\":-575.0}";

            Vector2D p1 = new Vector2D(-575.0, -575.0);

            Vector2D Vector3 = JsonSerializer.Deserialize<Vector2D>(Vector2);

            Assert.AreEqual(Vector3,p1);
          


        }

        [TestMethod]
        public void TestGetRootElement()
        {

            String wall = "{\"wall\":1,\"p1\":{\"x\":-575.0,\"y\":-575.0},\"p2\":{\"x\":-575.0,\"y\":575.0}}";
            Wall test = new();
            JsonDocument doc = JsonDocument.Parse(wall);
            if (doc.RootElement.TryGetProperty("wall", out _))
                Assert.AreEqual(2, 3);


        }


        
        
    }
}