using System;
using System.Collections.Generic;
using System.Text;

namespace MonikaOnDesktop
{
    class Message
    {
        public Message(string message)
        {
            Value = message;
        }

        public DateTime DateTime { get; set; } = DateTime.Now;
        public string Value { get; set; }
    }
}
