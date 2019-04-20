using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RocketChatToDoServer.Database
{
    public class TaskParser
    {
        public TaskParser()
        {
            GetUser = (string userName) =>
            {
                return new User()
                {
                    ID = 1,
                    Name = userName,
                    Tasks = new List<Task>()
                };
            };
            CreateTask = (User owner, string taskDescription, DateTime? dueDate) =>
            {
                return new Task()
                {
                    CreationDate = DateTime.Now,
                    DueDate = dueDate ?? default(DateTime),
                    ID = 1,
                    TaskDescription = taskDescription,
                    User = owner,
                    UserID = owner.ID
                };
            };
        }
        private const string USERGROUPNAME = "name";
        private const string TASKGROUPNAME = "task";
        public string SeparateUsersAndTaskRegex { get => separateUsersAndTaskRegex != null ? separateUsersAndTaskRegex.ToString() : null; set => separateUsersAndTaskRegex = new Regex(value); }
        private Regex separateUsersAndTaskRegex = new Regex($@"(?:@(?<{USERGROUPNAME}>\w+).*?)+:(?<{TASKGROUPNAME}>.*)");

        public void ParseMessage(string message)
        {
            // get users via regex:

            var usermatch = separateUsersAndTaskRegex.Match(message);
            string taskmessage = usermatch.Groups[TASKGROUPNAME].Value;
            DateTime? duedate = GetDueDate(taskmessage);
            foreach (System.Text.RegularExpressions.Capture username in usermatch.Groups[USERGROUPNAME].Captures)
            {
                var user = GetUser(username.Value);
                CreateTask(user, taskmessage, duedate);
            }
        }

        public string UntilRegex { get => untilRegex.ToString(); set => untilRegex = new Regex(value); }
        private Regex untilRegex = new Regex("until(?<dateexpression>.*?)");

        private DateTime? GetDueDate(string taskMessage)
        {
            DateTime? duedate = null;
            foreach (Match match in untilRegex.Matches(taskMessage))
            {
                foreach (DateFromTextDelegate func in dayFunctions)
                {
                    string timeMatchString = match.Groups["dateexpression"].Value.Trim();
                    try
                    {
                        duedate = func(timeMatchString);
                    }
                    catch (Exception)
                    {
                    }
                    if (duedate != null)
                    {
                        foreach(TimeFromTextDelegate timefunc in timeFunctions)
                        {
                            DateTime? dd = timefunc(timeMatchString, duedate.Value);
                            if(dd != null)
                            {
                                return dd;
                            }
                        }
                        return duedate;
                    }
                }
                    
                
            }
            return null;
        }

        private readonly List<DateFromTextDelegate> dayFunctions = new List<DateFromTextDelegate>()
        {
            (string message) =>
            {
                Match match = Regex.Match(message, "(next |coming |upcoming )?(?<day>monday|tuesday|wednesday|thursday|friday|saturday|sunday)", RegexOptions.IgnoreCase);
                if(match != null)
                {
                    return null;
                } else
                {
                    return null;
                }
            },
            (string message) =>
            {
                Match match = Regex.Match(message, "tomorrow", RegexOptions.IgnoreCase);
                if(match != null)
                {
                    return DateTime.Now + new TimeSpan(1, 0, 0, 0);
                } else
                {
                    return null;
                }
            }
        };

        private readonly List<TimeFromTextDelegate> timeFunctions = new List<TimeFromTextDelegate>();
        /// <summary>
        /// Regular expression to parse a task
        /// </summary>
        public string ParseExpression { get; set; }

        public Func<string, User> GetUser { get; set; }
        public CreateTaskDelegate CreateTask { get; set; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="taskDescription"></param>
    /// <param name="dueDate"></param>
    /// <returns></returns>
    public delegate Task CreateTaskDelegate(User owner, string taskDescription, DateTime? dueDate);

    /// <summary>
    /// Callback to get a message and parse it to a relative date, like "next wednesday", "tomorrow", "in 2 weeks"
    /// </summary>
    /// <param name="message">message to parse (trimmed)</param>
    /// <returns>datetime, return null if nothing can be found in the message</returns>
    public delegate DateTime? DateFromTextDelegate(string message);

    /// <summary>
    /// This delegate is meant to convert the time.
    /// </summary>
    /// <param name="message">message to parse (trimmed)</param>
    /// <param name="day">the day</param>
    /// <returns>Time of the day, day will be ignored. Use null if nothing is found</returns>
    public delegate DateTime? TimeFromTextDelegate(string message, DateTime day);
}
