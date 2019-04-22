using Microsoft.Extensions.Logging;
using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RocketChatToDoServer.Database
{
    public class TaskParser
    {
        public TaskParser(ILogger<TaskParser> logger, Func<string, ILogger, User> getUserCallback, CreateTaskDelegate createTaskCallback)
        {
            this.logger = logger;
            GetUser = getUserCallback;
            CreateTask = createTaskCallback;
        }

        private const string USERGROUPNAME = "name";
        private const string TASKGROUPNAME = "task";
        public string SeparateUsersAndTaskRegex { get => separateUsersAndTaskRegex != null ? separateUsersAndTaskRegex.ToString() : null; set => separateUsersAndTaskRegex = new Regex(value); }
        private Regex separateUsersAndTaskRegex = new Regex($@"(?:@(?<{USERGROUPNAME}>\w+).*?)+:(?<{TASKGROUPNAME}>.*)");

        public void ParseMessage(string message, ILogger contextLogger, DateTime? now)
        {
            using (contextLogger.BeginScope("ParseMessage"))
            {
                contextLogger.LogTrace("Calling Parse Message");
                now = now ?? DateTime.Now;
                // get users via regex:

                contextLogger.LogDebug("Got input message {message}", message);
                var usermatch = separateUsersAndTaskRegex.Match(message);
                string taskmessage = usermatch.Groups[TASKGROUPNAME].Value;
                DateTime? duedate = GetDueDate(taskmessage, now.Value);
                foreach (Capture username in usermatch.Groups[USERGROUPNAME].Captures)
                {
                    contextLogger.LogDebug("Creating task for user '{user}'", username.Value);
                    User user;
                    try
                    {
                        user = GetUser(username.Value, contextLogger);
                        if (user == null)
                        {
                            throw new InvalidOperationException("Could not find or create any user");
                        }
                    }
                    catch (Exception ex)
                    {
                        contextLogger.LogError(ex, "Could not find or create any user with the name '{name}'", username.Value);
                        continue;
                    }
                    logger.LogDebug("Creating Task for user '{username}'", user.Name);
                    CreateTask(user, taskmessage, contextLogger, duedate);
                }
            }
        }

        public string UntilRegex { get => untilRegex.ToString(); set => untilRegex = new Regex(value); }
        private Regex untilRegex = new Regex(@"until(?<dateexpression>.*?)(?=until|\z)");
        private ILogger<TaskParser> logger1;

        public DateTime? GetDueDate(string taskMessage, DateTime? now)
        {
            logger.LogTrace("Getting due date");
            now = now ?? DateTime.Now;
            DateTime? duedate = null;
            foreach (Match match in untilRegex.Matches(taskMessage))
            {
                logger.LogDebug("Parsing potential due date: '{duedate}'", match.Value);
                using (logger.BeginScope(match.Value))
                {
                    logger.LogDebug("Getting due day");
                    foreach (DateFromTextDelegate func in dayFunctions)
                    {
                        string timeMatchString = match.Groups["dateexpression"].Value.Trim();
                        try
                        {
                            duedate = func(timeMatchString, now.Value, logger);
                        }
                        catch (Exception)
                        {
                        }
                        if (duedate != null)
                        {
                            logger.LogDebug("Getting due time");
                            foreach (TimeFromTextDelegate timefunc in timeFunctions)
                            {
                                try
                                {
                                    DateTime? dd = timefunc(timeMatchString, duedate.Value, now.Value);
                                    if (dd != null)
                                    {
                                        return dd;
                                    }
                                }
                                catch (Exception)
                                {

                                }
                            }
                            return duedate;
                        }
                    }

                }


            }
            return null;
        }

        private readonly List<DateFromTextDelegate> dayFunctions = new List<DateFromTextDelegate>()
        {
            (string message, DateTime now, ILogger logger) =>
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
            (string message, DateTime now, ILogger logger) =>
            {
                Match match = Regex.Match(message, "tomorrow", RegexOptions.IgnoreCase);
                if(match != null)
                {
                    return now + new TimeSpan(1, 0, 0, 0);
                } else
                {
                    return null;
                }
            }
        };

        private readonly List<TimeFromTextDelegate> timeFunctions = new List<TimeFromTextDelegate>()
        {
            (string message, DateTime dueDate, DateTime now) =>
            {
                Match m = Regex.Match(message, @"(?<hour>\d+):(?<minute>\d+)");
                if(m != null)
                    return new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, int.Parse(m.Groups["hour"].Value), int.Parse(m.Groups["minute"].Value), 0);
                else
                    return null;
            }
        };
        private readonly ILogger<TaskParser> logger;

        /// <summary>
        /// Regular expression to parse a task
        /// </summary>
        public string ParseExpression { get; set; }

        public Func<string, ILogger, User> GetUser { get; set; }
        public CreateTaskDelegate CreateTask { get; set; }
        public DateTime Now { get; }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="taskDescription"></param>
    /// <param name="dueDate"></param>
    /// <returns></returns>
    public delegate Task CreateTaskDelegate(User owner, string taskDescription, ILogger logger, DateTime? dueDate);

    /// <summary>
    /// Callback to get a message and parse it to a relative date, like "next wednesday", "tomorrow", "in 2 weeks"
    /// </summary>
    /// <param name="message">message to parse (trimmed)</param>
    /// <param name="now">Will always give the current Timestamp used by the parser to sync methods</param>
    /// <returns>datetime, return null if nothing can be found in the message</returns>
    public delegate DateTime? DateFromTextDelegate(string message, DateTime now, ILogger logger);

    /// <summary>
    /// This delegate is meant to convert the time.
    /// </summary>
    /// <param name="message">message to parse (trimmed)</param>
    /// <param name="day">the day</param>
    /// <param name="now">The current now used in the parser to sync methods</param>
    /// <returns>Time of the day, day will be ignored. Use null if nothing is found</returns>
    public delegate DateTime? TimeFromTextDelegate(string message, DateTime day, DateTime now);
}
