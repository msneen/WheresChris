using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AppCenter.Crashes;

namespace StayTogether.Helpers
{
    public class TimerState
    {
        public Action Action { get; set; }
    }
    public class Interval
    {
        private Timer _timer;

        public void SetInterval(Action action, int interval)
        {
            TimerCallback timerCallback = OnInterval;
            _timer = new Timer(timerCallback, action, interval, 0);
        }

        private void OnInterval(object state)
        {
            try
            {
                _timer.Dispose();
                var action = (Action) state;
                action();
            }
            catch (Exception ex)
            {               
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    {"Source", ex.Source },
                    { "stackTrace",ex.StackTrace}
                });
            }
        }

    }
}
