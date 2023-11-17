using Model;
using SnakeGame;
using System.Text.Json;


namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {
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
            String wall = "{\"p1\":{\"x\":-575.0,\"y\":-575.0}}";

            Vector2D p1 = new Vector2D(-575.0, -575.0);




            Vector2D wall1 = JsonSerializer.Deserialize<Vector2D>(wall);

            Assert.AreEqual(wall1,p1);
          


        }
    }
}