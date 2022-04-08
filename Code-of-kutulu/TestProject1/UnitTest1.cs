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
/*
15
13
###############
#.............#
#.#.#.#.#.#.#.#
#..w.......w..#
#.#.#.#.#.#.#.#
#.............#
#.#.#.#.#.#.#.#
#.............#
#.#.#.#.#.#.#.#
#..w.......w..#
#.#.#.#.#.#.#.#
#.............#
###############
3 1 3 40
4
EXPLORER 0 5 5 250 2 3
EXPLORER 1 9 5 250 2 3
EXPLORER 2 5 7 250 2 3
EXPLORER 3 9 7 250 2 3 
*/
        [Test]
        public void Test1()
        {
            string msg = "15\n13\n###############\n#.............#\n#.#.#.#.#.#.#.#\n#..w.......w..#\n#.#.#.#.#.#.#.#\n#.............#\n#.#.#.#.#.#.#.#\n#.............#\n#.#.#.#.#.#.#.#\n#..w.......w..#\n#.#.#.#.#.#.#.#\n#.............#\n###############\n3 1 3 40\n4\nEXPLORER 0 5 5 250 2 3\nEXPLORER 1 9 5 250 2 3\nEXPLORER 2 5 7 250 2 3\nEXPLORER 3 9 7 250 2 3 ";
            test(msg);
            Assert.Pass();
        }
    }
}