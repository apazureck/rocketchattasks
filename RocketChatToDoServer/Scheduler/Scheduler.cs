using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RocketChatToDoServer.TodoBot;
using System;

namespace RocketChatToDoServer.Scheduler
{
    public class Scheduler
    {
        private readonly BotService botService;
        private readonly ILogger<Scheduler> logger;
        System.Timers.Timer timer;
        private readonly TimeSpan reminderTime;
        public Scheduler(BotService botService, IConfiguration config, ILogger<Scheduler> logger)
        {
            timer = new System.Timers.Timer();
            this.botService = botService;
            this.logger = logger;
            reminderTime = config.GetValue("reminderTime", new TimeSpan(9, 0, 0));
        }

        public void StartScheduler()
        {
            DateTime st = SetupReminder(reminderTime);
            logger.LogInformation("Set up reminder. I am reminding them next time at {remTime}", st);
        }

        private DateTime SetupReminder(TimeSpan reminderTime)
        {
            logger.LogTrace("Setting Up Reminder Timer");
            DateTime now = DateTime.Now;
            var scheduledTime = new DateTime(now.Year, now.Month, now.Day, reminderTime.Hours, reminderTime.Minutes, reminderTime.Seconds);
            if (now > scheduledTime)
                scheduledTime = scheduledTime.AddDays(1);

            logger.LogDebug("Scheduled time for next Reminder: {time}", scheduledTime);

            TimeSpan nextRemind = scheduledTime - DateTime.Now;

            timer = new System.Timers.Timer(nextRemind.TotalMilliseconds);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            logger.LogDebug("Sucessfully started new timer");
            return scheduledTime;
        }

        private async void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            logger.LogInformation("Reminder Timer expired. Sending reminders to users.");
            try
            {
                timer.Stop();
                timer.Elapsed -= Timer_Elapsed;
                timer.Dispose();
                logger.LogDebug("Sucessfully disposed old timer");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Could not stop timer, continuing to setup new timer");
            }

            await botService.SendReminders();

            StartScheduler();
        }
    }
}
