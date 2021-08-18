using System;
using System.Collections.Generic;
using System.Text;

namespace MonikaOnDesktop
{
    public class Expression
    {
        public String Text { get; set; }
        public String Face { get; set; }

        public String[] Hello = {"Привет, {PlayerName}! Я скучала", "Привет!"};

        public Expression(string text, string face)
        {
            if (string.IsNullOrWhiteSpace(face))
            {
                face = "a";
            }

            this.Text = text;
            this.Face = face;
        }
        public Expression(string text)
        {
            this.Text = text;
            this.Face = "a";
        }

    }
}
