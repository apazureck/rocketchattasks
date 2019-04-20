using NUnit.Framework;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using System.Collections.Generic;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase]
        public void TestSingleUser(string inputMessage)
        {
            var tp = new TaskParser();
            Assert.Pass();
        }
    }

    public class ParseResult
    {
        List<Task> Tasks { get; set; }
        List<User> Users { get; set; }
    }
}