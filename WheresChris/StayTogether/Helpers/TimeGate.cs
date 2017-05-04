using System;
using System.Collections.Generic;
using System.Text;

namespace StayTogether.Helpers
{
    /// <summary>
    /// .CanProcess subtracts last processing time from current time.  Returns true if greater than interval.
    /// if updateTime parameter is true, updates last processing time to current time(resets the clock)
    /// </summary>
    public class TimeGate
    {

        private DateTime _lastProcessingTime;
        private readonly TimeSpan _intervalTimeSpan;

        /// <summary>
        /// Sets the interval in Milliseconds
        /// </summary>
        /// <param name="interval">Interval in Milliseconds</param>
        public TimeGate(int interval)
        {
            _intervalTimeSpan = TimeSpan.FromMilliseconds(interval);
            UpdateLastProcessingTime();
        }

        /// <summary>
        /// Sets the interval
        /// </summary>
        /// <param name="hours">Hours</param>
        /// <param name="minutes">Minutes</param>
        /// <param name="seconds">Seconds</param>
        public TimeGate(int hours, int minutes, int seconds)
        {
            _intervalTimeSpan = new TimeSpan(hours, minutes, seconds);
            UpdateLastProcessingTime();
        }

        /// <summary>
        /// Sets the interval with the provided TimeSpan
        /// </summary>
        /// <param name="timeSpan"></param>
        public TimeGate(TimeSpan timeSpan)
        {
            _intervalTimeSpan = timeSpan;
            UpdateLastProcessingTime();
        }

        /// <summary>
        /// subtracts last processing time from current time.  Returns true if greater than interval.
        /// if updateTime is true, updates last processing time to current time(resets the clock)
        /// </summary>
        /// <param name="updateTime">bool.  if true, updates last processing time to current time</param>
        /// <returns>bool.  True if greater than interval</returns>
        public bool CanProcess(bool updateTime)
        {
            if (DateTime.Now.Subtract(_lastProcessingTime) < _intervalTimeSpan) return false;
            if (updateTime)
            {
                UpdateLastProcessingTime();
            }
            return true;
        }

        /// <summary>
        /// Updates last processing time to current time.
        /// </summary>
        public void UpdateLastProcessingTime()
        {
            _lastProcessingTime = DateTime.Now;
        }
    }
}
