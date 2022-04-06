using System;
using System.ComponentModel.Design;
using System.IO;
using NUnit.Framework;

namespace the_resistance_test
{
    public class Tests
    {

        public static UInt64 test(string msgIn)
        {
            var stringIn = new StringReader(msgIn);
            var stringOut = new StringWriter();
            Console.SetIn(stringIn);
            Console.SetOut(stringOut);
            Solution.Main(null);
            return UInt64.Parse(stringOut.ToString().Trim());
        }
        
        [SetUp]
        public void Setup()
        {
        }
        /*......-...-..---.-----.-..-..-..
        5
        HELL
        HELLO
        OWORLD
        WORLD
        TEST*/
        [Test]
        public void Test1()
        {
            var param = "......-...-..---.-----.-..-..-..\n"+
                        "5\n"+
                        "HELL\n"+
                        "HELLO\n"+
                        "OWORLD\n"+
                        "WORLD\n"+
                        "TEST";
            Assert.True(test(param)==(UInt64)2);
        }
        /*-.-..---.-..---.-..--
5
CAT
KIM
TEXT
TREM
CEM*/
        [Test]
        public void Test2()
        {
            var param = "-.-..---.-..---.-..--\n"+
                        "5\n"+
                        "CAT\n"+
                        "KIM\n"+
                        "TEXT\n"+
                        "TREM\n"+
                        "CEM";
            Assert.True(test(param)==(UInt64)125);
        }
        /*..............................................
2
E
I*/
        [Test]
        public void Test3()
        {
            var param = "..............................................\n"+
                        "2\n"+
                        "E\n"+
                        "I";
            Assert.True(test(param)==(UInt64)2971215073);
        }
        /*
         * -.-
6
A
B
C
HELLO
K
WORLD
         */
        [Test]
        public void Test4()
        {
            var param = "-.-\n"+
                        "6\n"+
                        "A\n"+
                        "B\n"+
                        "C\n"+
                        "HELLO\n"+
                        "K\n"+
                        "WORLD";
            Assert.True(test(param)==(UInt64)1);
        }
        /*--.-------..
5
GOD
GOOD
MORNING
G
HELLO*/
        [Test]
        public void Test5()
        {
            var param = "--.-------..\n"+
                        "5\n"+
                        "GOD\n"+
                        "GOOD\n"+
                        "MORNING\n"+
                        "G\n"+
                        "HELLO";
            Assert.True(test(param)==(UInt64)1);
        }
        /*......-...-..---.-----.-..-..-..
        5
        HELL
        HELLO
        OWORLD
        WORLD
        TEST*/
        [Test]
        public void Test6()
        {
            var param = "......-...-..---.-----.-..-..-..\n"+
                        "5\n"+
                        "HELL\n"+
                        "HELLO\n"+
                        "OWORLD\n"+
                        "WORLD\n"+
                        "TEST";
            Assert.True(test(param)==(UInt64)2);
        }
    }
}