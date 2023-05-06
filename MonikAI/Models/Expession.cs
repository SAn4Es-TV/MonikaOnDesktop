using System;

namespace MonikaOnDesktop
{
    public class Expression
    {
        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                _text = string.Intern(value);
            }
        }
        private string _face;
        public string Face
        {
            get => _face;
            set
            {
                _face = string.Intern(value);
            }
        }

        public String[] Hello = { "Привет, {PlayerName}! Я скучала", "Привет!" };

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
