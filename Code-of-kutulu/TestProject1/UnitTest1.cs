using System;
using System.IO;
using NUnit.Framework;

namespace TestProject1
{
    public class Tests
    {
        public static string test(string msgIn)
        {
            var stringIn = new StringReader(msgIn);
            var stringOut = new StringWriter();
            Console.SetIn(stringIn);
            Console.SetOut(stringOut);
            Player.Main(null);
            return stringOut.ToString().Trim();
        }

        [SetUp]
        public void Setup()
        {
        }
        
        [Test]
        public void Test1()
        {
            // msg is content file game.txt in project folder
            //this.wanderers.Find(wanderer => wanderer.id==4) == graph[4,3]
            var msg = File.ReadAllText("../../../game.txt");
            test(msg);
            Assert.Pass();
        }
    }
}