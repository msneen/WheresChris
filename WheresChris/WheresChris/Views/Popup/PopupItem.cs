using System;
using System.Collections.Generic;
using System.Text;

namespace WheresChris.Views.Popup
{
    public class PopupItem
    {
        public PopupItem()
        {
            
        }

        public PopupItem(string text, Action clickAction)
        {
            Text = text;
            ClickAction = clickAction;
        }

        public string Text { get; set; }

        public Action ClickAction { get; set; }
    }
}
