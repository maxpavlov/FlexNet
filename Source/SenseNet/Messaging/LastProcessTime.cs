using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SenseNet.Messaging
{
    internal partial class LastProcessTime
    {
        //database:
        //    lastimmediately:  2011-03-18 14:05
        //    lastdaily:        2011-03-17 23:10
        //    lastweekly:       2011-03-13 23:10
        //    lastmonthly:      2011-02-27 23:10
        //config:
        //    Daily:    hour-minute             23:10
        //    Weekly:   weekday + hour-minute   Sunday 23:10
        //    Monthly:  day + hour-minute       1. Sunday 23:10
        //                                      2. Sunday 23:10
        //                                      3. Sunday 23:10
        //                                      Last Sunday 23:10
        //                                      Every 10. 23:10
        //Words: Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Last, Every

        private static DateTime? _nextDaily;
        private static DateTime? _nextWeekly;
        private static DateTime? _nextMonthly;

        internal static DateTime GetNextTime(NotificationFrequency freq, DateTime now)
        {
            switch (freq)
            {
                case NotificationFrequency.Immediately: return now;
                case NotificationFrequency.Daily: return GetNextDailyTimeWithoutOrigin(now);
                case NotificationFrequency.Weekly: return GetNextWeeklyTimeWithoutOrigin(now);
                case NotificationFrequency.Monthly: return GetNextMonthlyTimeWithoutOrigin(now);
                default: throw NotificationHandler.GetUnknownFrequencyException(freq);
            }
        }

        internal static DateTime GetNextDailyTimeWithoutOrigin(DateTime now)
        {
            if (_nextDaily.HasValue)
                return _nextDaily.Value;
            var origin = GetLastProcessTime(NotificationFrequency.Daily);
            _nextDaily = GetNextDailyTime(origin == DateTime.MinValue ? now : origin);
            return _nextDaily.Value;
        }
        internal static DateTime GetNextDailyTime(DateTime origin)
        {
            var ct = Configuration.DailyHour * 60 + Configuration.DailyMinute;
            var ot = origin.Hour * 60 + origin.Minute;
            var d = ct > ot ? 0 : 1;
            return new DateTime(origin.Year, origin.Month, origin.Day + d, Configuration.DailyHour, Configuration.DailyMinute, 0);
        }

        internal static DateTime GetNextWeeklyTimeWithoutOrigin(DateTime now)
        {
            if (_nextWeekly.HasValue)
                return _nextWeekly.Value;
            var origin = GetLastProcessTime(NotificationFrequency.Weekly);
            _nextWeekly = GetNextWeeklyTime(origin == DateTime.MinValue ? now : origin);
            return _nextWeekly.Value;
        }
        internal static DateTime GetNextWeeklyTime(DateTime origin)
        {
            DateTime nextTime = new DateTime(origin.Year, origin.Month, origin.Day, Configuration.WeeklyHour, Configuration.WeeklyMinute, 0);

            if (origin.DayOfWeek == Configuration.WeeklyWeekDay)
                if (origin.Hour * 60 + origin.Minute > Configuration.WeeklyHour * 60 + Configuration.WeeklyMinute)
                    nextTime = nextTime.AddDays(7);

            nextTime = GoToDayOfWeek(nextTime, Configuration.WeeklyWeekDay);

            return nextTime;
        }

        internal static DateTime GetNextMonthlyTimeWithoutOrigin(DateTime now)
        {
            if (_nextMonthly.HasValue)
                return _nextMonthly.Value;
            var origin = GetLastProcessTime(NotificationFrequency.Monthly);
            _nextMonthly = GetNextMonthlyTime(origin == DateTime.MinValue ? now : origin);
            return _nextMonthly.Value;
        }
        internal static DateTime GetNextMonthlyTime(DateTime origin)
        {
            if (Configuration.MonthlyEvery)
                return GetNextMonthlyTimeByEveryDayNr(origin);
            if (Configuration.MonthlyLast)
                return GetNextMonthlyTimeByLastWeekday(origin);
            return GetNextMonthlyTimeByWeekNrWeekday(origin);
        }
        private static DateTime GetNextMonthlyTimeByEveryDayNr(DateTime origin)
        {
            var daysInMonth = DateTime.DaysInMonth(origin.Year, origin.Month);
            var dayOfMonth = Configuration.MonthlyDay > daysInMonth ? daysInMonth : Configuration.MonthlyDay;
            var nextTime = new DateTime(origin.Year, origin.Month, dayOfMonth, Configuration.MonthlyHour, Configuration.MonthlyMinute, 0);
            if (nextTime > origin)
                return nextTime;

            var nextMonth = origin.Month + 1;
            if (nextMonth == 13)
                return new DateTime(origin.Year + 1, 1, Configuration.MonthlyDay, Configuration.MonthlyHour, Configuration.MonthlyMinute, 0);

            daysInMonth = DateTime.DaysInMonth(origin.Year, nextMonth);
            dayOfMonth = Configuration.MonthlyDay > daysInMonth ? daysInMonth : Configuration.MonthlyDay;
            return new DateTime(origin.Year, nextMonth, dayOfMonth, Configuration.MonthlyHour, Configuration.MonthlyMinute, 0);
        }
        private static DateTime GetNextMonthlyTimeByWeekNrWeekday(DateTime origin)
        {
            //var startTime = new DateTime(origin.Year, origin.Month, 1, Configuration.MonthlyHour, Configuration.MonthlyMinute, 0);
            //var nextTime = GoToDayOfWeek(startTime, Configuration.MonthlyWeekDay);
            //nextTime = SkipWeeks(nextTime, Configuration.MonthlyWeek - 1);

            //if (nextTime > origin)
            //    return nextTime;

            //startTime = startTime.AddMonths(1);
            //nextTime = GoToDayOfWeek(startTime, Configuration.MonthlyWeekDay);
            //nextTime = SkipWeeks(nextTime, Configuration.MonthlyWeek - 1);

            //return nextTime;
            return GoToNthWeekDay(origin, Configuration.MonthlyWeek, Configuration.MonthlyWeekDay, Configuration.MonthlyHour, Configuration.MonthlyMinute);
        }
        private static DateTime GetNextMonthlyTimeByLastWeekday(DateTime origin)
        {
            return GoToNthWeekDay(origin, 5, Configuration.MonthlyWeekDay, Configuration.MonthlyHour, Configuration.MonthlyMinute);
        }
        private static DateTime GoToNthWeekDay(DateTime origin, int weekNum, DayOfWeek weekDay, int hour, int minute)
        {
            var startTime = new DateTime(origin.Year, origin.Month, 1, hour, minute, 0);
            var nextTime = GoToDayOfWeek(startTime, weekDay);
            nextTime = SkipWeeks(nextTime, weekNum - 1);

            if (nextTime > origin)
                return nextTime;

            startTime = startTime.AddMonths(1);
            nextTime = GoToDayOfWeek(startTime, weekDay);
            nextTime = SkipWeeks(nextTime, weekNum - 1);

            return nextTime;

        }
        private static DateTime GoToDayOfWeek(DateTime startTime, DayOfWeek dayOfWeek)
        {
            var w = dayOfWeek - startTime.DayOfWeek;
            if (w < 0)
                w += 7;
            return startTime.AddDays(w);
        }
        private static DateTime SkipWeeks(DateTime d, int weeks)
        {
            var week = 0;
            while (week < weeks)
            {
                var nextWeek = d.AddDays(7);
                if (nextWeek.Month > d.Month)
                    break;
                d = nextWeek;
                week++;
            }
            return d;
        }

        internal static void SetLastProcessTime(NotificationFrequency freq, DateTime lastTime)
        {
            using (var context = new DataHandler())
            {
                var existingEntry = context.LastProcessTimes.FirstOrDefault();
                if (existingEntry == null)
                    context.LastProcessTimes.InsertOnSubmit(GetDefaultInstance(freq, lastTime));
                else
                    SetValue(existingEntry, freq, lastTime);
                context.SubmitChanges();
            }
            switch (freq)
            {
                case NotificationFrequency.Daily: _nextDaily = null; break;
                case NotificationFrequency.Weekly: _nextWeekly = null; break;
                case NotificationFrequency.Monthly: _nextMonthly = null; break;
            }
        }
        internal static DateTime GetLastProcessTime(NotificationFrequency freq)
        {
            using (var context = new DataHandler())
            {
                var existingEntry = context.LastProcessTimes.FirstOrDefault();
                return (existingEntry == null) ? DateTime.MinValue : GetValue(existingEntry, freq);
            }
        }
        private static LastProcessTime GetDefaultInstance(NotificationFrequency freq, DateTime value)
        {
            var instance = new LastProcessTime();
            SetValue(instance, freq, value);
            return instance;
        }
        private static void SetValue(LastProcessTime instance, NotificationFrequency freq, DateTime value)
        {
            DateTime? dbValue = null;
            if (value >= SenseNet.ContentRepository.Storage.Data.DataProvider.Current.DateTimeMinValue)
                dbValue = value;
            switch (freq)
            {
                case NotificationFrequency.Immediately: instance.Immediately = dbValue; break;
                case NotificationFrequency.Daily: instance.Daily = dbValue; _nextDaily = null; break;
                case NotificationFrequency.Weekly: instance.Weekly = dbValue; _nextWeekly = null; break;
                case NotificationFrequency.Monthly: instance.Monthly = dbValue; _nextMonthly = null; break;
            }
        }
        private static DateTime GetValue(LastProcessTime instance, NotificationFrequency freq)
        {
            DateTime? value = null;
            switch (freq)
            {
                case NotificationFrequency.Immediately: value = instance.Immediately; break;
                case NotificationFrequency.Daily: value = instance.Daily; break;
                case NotificationFrequency.Weekly: value = instance.Weekly; break;
                case NotificationFrequency.Monthly: value = instance.Monthly; break;
            }
            return value.HasValue ? value.Value : DateTime.MinValue;
        }

        internal static void Reset()
        {
            using (var context = new DataHandler())
            {
                context.ExecuteCommand("DELETE FROM [Messaging.LastProcessTime]");
            }
        }

    }
}
