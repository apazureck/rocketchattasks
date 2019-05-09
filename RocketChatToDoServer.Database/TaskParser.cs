using Microsoft.Extensions.Logging;
using RocketChatToDoServer.Database.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RocketChatToDoServer.Database
{
    public class TaskParser
    {
        public TaskParser(ILogger logger, Func<string, ILogger, User> getUserCallback, CreateTaskDelegate createTaskCallback)
        {
            this.logger = logger;
            GetUser = getUserCallback;
            CreateTask = createTaskCallback;
        }

        private const string USERGROUPNAME = "name";
        private const string TASKGROUPNAME = "task";
        public string SeparateUsersAndTaskRegex { get => separateUsersAndTaskRegex != null ? separateUsersAndTaskRegex.ToString() : null; set => separateUsersAndTaskRegex = new Regex(value); }
        private Regex separateUsersAndTaskRegex = new Regex($@"(?:@(?<{USERGROUPNAME}>\w+).*?)+:(?<{TASKGROUPNAME}>.*)");

        public Task ParseMessage(string initiator, string message, ILogger contextLogger, DateTime? now = null)
        {
            using (contextLogger.BeginScope("ParseMessage"))
            {
                Task task = null;
                contextLogger.LogTrace("Calling Parse Message");
                now = now ?? DateTime.Now;
                // get users via regex:

                contextLogger.LogDebug("Got input message {message}", message);
                var usermatch = separateUsersAndTaskRegex.Match(message);
                string taskmessage = usermatch.Groups[TASKGROUPNAME].Value;
                DateTime? duedate = GetDueDate(taskmessage, now.Value);
                var assignees = new List<User>();
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
                    assignees.Add(user);
                    logger.LogDebug("Creating Task for user '{username}'", user.Name);
                    
                }
                var inituser = GetUser(initiator, logger);
                task = CreateTask(inituser, assignees, taskmessage, contextLogger, duedate);
                return task;
            }
        }

        public string UntilRegex { get => untilRegex.ToString(); set => untilRegex = new Regex(value); }
        private Regex untilRegex = new Regex(@"until(?<dateexpression>.*?)(?=until|\z)", RegexOptions.IgnoreCase);

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

        private static readonly List<DateFromTextDelegate> dayFunctions = new List<DateFromTextDelegate>()
        {
            (string message, DateTime now, ILogger logger) =>
            {
                Match match = Regex.Match(message, @"(next |coming |upcoming |this )?(?<day>"+WEEKDAYS+@")(?: "+DAYTIMES+@")?(?: in (?<weekcount>\d+) (?<weekmonth>weeks?|months?))?", RegexOptions.IgnoreCase);
                if(match.Success)
                {
                    // Get the day and determine the next 
                    string day = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(match.Groups["day"].Value.ToLower());
                    var dayofweek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), day);
                    DateTime date = now;
                    do
                        date = date.AddDays(1);
                    while(date.DayOfWeek != dayofweek);

                    
                    if(match.Groups["weekcount"].Success)
                    {
                        int weeks = int.Parse(match.Groups["weekcount"].Value);
                        string weekselect = match.Groups["weekmonth"]?.Value;
                        switch(weekselect.TrimEnd('s'))
                        {
                            case "week":
                                date = date.AddDays(weeks*7);
                                break;
                            case "month":
                                date = date.AddMonths(weeks);
                                break;
                            default:
                                throw new ArgumentException("no weeks or months are given");
                        }
                    }
                    return new DateTime(date.Year, date.Month, date.Day, 12, 0, 0);
                } else
                {
                    return null;
                }
            },
            (string message, DateTime now, ILogger logger) =>
            {
                Match match = Regex.Match(message, "this ("+DAYTIMES+")", RegexOptions.IgnoreCase);
                if(match.Success)
                {
                    return now;
                } else
                {
                    return null;
                }
            },
            (string message, DateTime now, ILogger logger) =>
            {
                Match match = Regex.Match(message, "tomorrow", RegexOptions.IgnoreCase);
                if(match.Success)
                {
                    var date = now + new TimeSpan(1, 0, 0, 0);
                    return new DateTime(date.Year, date.Month, date.Day, 12, 0, 0);
                } else
                {
                    return null;
                }
            }
        };

        const string DAYTIMES = "afternoon|evening|noon|morning|night";
        const string WEEKDAYS = "sunday|monday|tuesday|wednesday|thursday|friday|saturday";

        private static readonly List<TimeFromTextDelegate> timeFunctions = new List<TimeFromTextDelegate>()
        {
            (string message, DateTime dueDate, DateTime now) =>
            {
                Match m = Regex.Match(message, @"(?<time>"+DAYTIMES+@")(?: in \d+ (weeks?|months?)|\z)");
                if(m.Success)
                {
                    switch(m.Groups["time"].Value.ToUpper())
                    {
                        case "AFTERNOON":
                            return new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 15, 0, 0);
                        case "EVENING":
                            return new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 18, 0, 0);
                        case "MORNING":
                            return new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 9, 0, 0);
                        case "NIGHT":
                            return new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 23, 0, 0);
                        case "NOON":
                            return new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 12, 0, 0);
                        default:
                            throw new Exception("Wrong match found in regex!");
                    }
                }
                else
                    return null;
            },
            (string message, DateTime dueDate, DateTime now) =>
            {
                Match m = Regex.Match(message, @"(?<hour>\d+):(?<minute>\d+)");
                if(m.Success)
                    return new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, int.Parse(m.Groups["hour"].Value), int.Parse(m.Groups["minute"].Value), 0);
                else
                    return null;
            }
        };
        private readonly ILogger logger;

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
    public delegate Task CreateTaskDelegate(User owner, IEnumerable<User> assignees, string taskDescription, ILogger logger, DateTime? dueDate);

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
