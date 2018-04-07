using System;
using System.Collections.Generic;
using System.Text;
using WheresChris.ViewModels;

namespace WheresChris.NotificationCenter
{
    public class ChatMessageReceivedResponse
    {
        public static void HandleChatMessageReceived()
        {
            App.SetCurrentTab("Chat");
        }
    }
}
