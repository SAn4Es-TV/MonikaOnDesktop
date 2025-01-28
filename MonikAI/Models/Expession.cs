using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace MonikaOnDesktop
{
    public class Expression : IDisposable
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

        void IDisposable.Dispose()
        {
            Text = null; Face = null;
            GC.Collect(0); GC.SuppressFinalize(this); GC.ReRegisterForFinalize(this);
        }
    }
}
