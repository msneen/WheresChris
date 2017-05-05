using System;
using System.Collections.Generic;
using System.Text;
using StayTogether.Helpers;

namespace WheresChris.Messaging
{
    public class MessageEventBase
    {
        protected TimeGate MessageTimeGate = new TimeGate(1000);

        public MessageEventBase(TimeSpan? interval = null)
        {
            if (interval.HasValue)
            {
                MessageTimeGate = new TimeGate(interval.Value);
            }
        }
    }
}
