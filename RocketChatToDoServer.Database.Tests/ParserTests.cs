using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        public Tests()
        {
            loggerFactory = new LoggerFactory()
                .AddNunit();
        }
        private readonly ILoggerFactory loggerFactory;
        private volatile TaskParser parser;
        private readonly Dictionary<string, User> users = new Dictionary<string, User>();
        private readonly List<Task> tasks = new List<Task>();
        private volatile int userIdCounter = 0;
        [SetUp]
        public void Setup()
        {
            ILogger<TaskParser> log = loggerFactory.CreateLogger<TaskParser>();
            parser = new TaskParser(log,
                (string userName, ILogger logger) =>
                {
                    User user;
                    if (!users.TryGetValue(userName, out user))
                    {
                        logger.LogDebug("User not found: Creating user with name {userName}", userName);
                        user = new User()
                        {
                            ID = ++userIdCounter,
                            Name = userName,
                            Tasks = new List<Task>()
                        };
                        users.Add(userName, user);
                    }
                    else
                        logger.LogDebug("Found User with username {username}", userName);
                    return user;
                },
                (User owner, string taskDescription, ILogger logger, DateTime? dueDate) =>
                {
                    logger.LogDebug("Adding new Task for user {username}", owner.Name);
                    tasks.Add(new Task()
                    {
                        CreationDate = DateTime.Now,
                        DueDate = dueDate ?? default,
                        ID = 1,
                        TaskDescription = taskDescription,
                        User = owner,
                        UserID = owner.ID
                    });
                    return tasks.Last();
                });
        }

        //[Test]
        //public void TestSingleUser()
        //{
        //    string message = "@pazureck @wuzifuzi: Put new Task together, helping @mabbl until next wednesday";
        //    var tp = new TaskParser();
        //    tp.ParseMessage(message, DateTime.Now);
        //    Assert.Pass();
        //}

        [TestCaseSource(nameof(TestDueDateData))]
        public DateTime? TestDueDate(string message, DateTime now)
        {
            loggerFactory.CreateLogger("test").LogInformation("Test");
            return parser.GetDueDate(message, now);
        }

        public static List<TestCaseData> TestDueDateData
        {
            get
            {
                DateTime now = DateTime.Now;
                return new List<TestCaseData>
                {
                    new TestCaseData("Do stuff until tomorrow", now)
                    {
                        ExpectedResult = now + new TimeSpan(1, 0, 0, 0)
                    },
                    new TestCaseData("Work until everything is done and report until tomorrow", now)
                    {
                        ExpectedResult = now + new TimeSpan(1, 0, 0, 0)
                    }
                };
            }
        }
    }

    public class ParseResult
    {
        List<Task> Tasks { get; set; }
        List<User> Users { get; set; }
    }
}