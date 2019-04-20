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

        [Test]
        public void TestSingleUser() {
            string message = "@pazureck @wuzifuzi: Put new Task together, helping @mabbl until next wednesday";
            var tp = new TaskParser();
            tp.ParseMessage(message);
            Assert.Pass();
        }
    }

    public class ParseResult
    {
        List<Task> Tasks { get; set; }
        List<User> Users { get; set; }
    }
}