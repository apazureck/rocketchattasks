using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Returns a new <see cref="DateTime"/> that adds the specified numbers of weeks to the value of this instance
        /// </summary>
        /// <param name="dt"><see cref="DateTime"/> instance to extend</param>
        /// <param name="weeks">Number of whole weeks and fractions to add to the <see cref="DateTime"/> instance</param>
        /// <returns>The <see cref="DateTime"/> of the added weeks</returns>
        public static DateTime AddWeeks(this DateTime dt, double weeks) => dt.AddDays(weeks * 7);

        /// <summary>
        /// Gets the <see cref="DateTime"/> of the next day of the week.
        /// Example:
        /// Current day is a Monday the 4th this method will get the <see cref="DateTime"/> of the next Friday the 8th, when Friday is given. If a Friday the 1st is given the method will put out Friday the 8th, too.
        /// </summary>
        /// <param name="date"><see cref="DateTime"/> to extend</param>
        /// <param name="dayOfWeek">The day of the week to find</param>
        /// <returns>The found weekday</returns>
        public static DateTime GetNextWeekDay(this DateTime date, DayOfWeek dayOfWeek)
        {
            do
                date = date.AddDays(1);
            while (date.DayOfWeek != dayOfWeek);
            return date;
        }
    }
}
