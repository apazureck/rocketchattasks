using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RocketChatToDoServer.Database;
using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public class TestDueDates
    {
        public TestDueDates()
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
                            Tasks = new List<UserTaskMap>()
                        };
                        users.Add(userName, user);
                    }
                    else
                        logger.LogDebug("Found User with username {username}", userName);
                    return user;
                },
                (User owner, IEnumerable<User> assignees, string taskDescription, ILogger logger, DateTime? dueDate) =>
                {
                    logger.LogDebug("{username} added new task for {assignees}", owner.Name, assignees.Select(u => u.Name).Aggregate((a,b) => a+ ", " + b));
                    tasks.Add(new Task()
                    {
                        CreationDate = DateTime.Now,
                        DueDate = dueDate ?? default,
                        ID = 1,
                        Title = taskDescription,
                        Initiator = owner,
                        InitiatorID = owner.ID
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
                var tests = new List<TestCaseData>();
                var date = now + new TimeSpan(1, 0, 0, 0);
                date = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0);
                tests.Add(new TestCaseData("Do stuff until tomorrow", now)
                {
                    TestName = "Until tomorrow",
                    ExpectedResult = date
                });

                tests.Add(new TestCaseData("Work until everything is done and report until tomorrow", now)
                {
                    TestName = "Test two untils and tomorrow",
                    ExpectedResult = date
                });

                // Evening
                date = now + new TimeSpan(1, 0, 0, 0);
                date = new DateTime(date.Year, date.Month, date.Day, 18, 0, 0);
                tests.Add(new TestCaseData("Work until everything is done and report until tomorrow evening", now)
                {
                    TestName = "Time of day",
                    ExpectedResult = date
                });

                // Morning

                date = now + new TimeSpan(1, 0, 0, 0);
                date = new DateTime(date.Year, date.Month, date.Day, 9, 0, 0);
                tests.Add(new TestCaseData("Work until everything is done and report until tomorrow morning", now)
                {
                    TestName = "Time of day 2",
                    ExpectedResult = date
                });

                date = now;
                date = new DateTime(date.Year, date.Month, date.Day, 18, 0, 0);
                tests.Add(new TestCaseData("Work until everything is done and report until this evening", now)
                {
                    TestName = "Test today",
                    ExpectedResult = date
                });

                date = now;
                do
                    date = date.AddDays(1);
                while (date.DayOfWeek != DayOfWeek.Thursday);

                date = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0);
                tests.Add(new TestCaseData("Work until everything is done and report until next thursday", now)
                {
                    TestName = "Next weekdayname",
                    ExpectedResult = date
                });

                date = now;
                do
                    date = date.AddDays(1);
                while (date.DayOfWeek != DayOfWeek.Wednesday);

                date = new DateTime(date.Year, date.Month, date.Day, 15, 0, 0);
                tests.Add(new TestCaseData("Work until everything is done and report Until NexT WeDNESday afternoon", now)
                {
                    TestName = "Case Sensitivity and next day of week plus time of day",
                    ExpectedResult = date
                });

                date = now;
                do
                    date = date.AddDays(1);
                while (date.DayOfWeek != DayOfWeek.Friday);

                date = new DateTime(date.Year, date.Month, date.Day, 15, 0, 0).AddDays(5*7);
                tests.Add(new TestCaseData("Work and report until friday afternoon in 5 weeks", now)
                {
                    TestName = "in n weeks plus time of day plus weekday",
                    ExpectedResult = date
                });

                date = now;
                do
                    date = date.AddDays(1);
                while (date.DayOfWeek != DayOfWeek.Tuesday);

                date = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0).AddMonths(3);
                tests.Add(new TestCaseData("Work for two weeks until everything is done and report Until TUESDAY in 3 months", now)
                {
                    TestName = "allcaps and n months",
                    ExpectedResult = date
                });

                return tests;
            }
        }
    }

    public class ParseResult
    {
        List<Task> Tasks { get; set; }
        List<User> Users { get; set; }
    }
}