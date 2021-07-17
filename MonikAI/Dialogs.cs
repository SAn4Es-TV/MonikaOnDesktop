using System;
using System.Collections.Generic;
using System.Text;

namespace MonikaOnDesktop
{
    public class Dialogs
    {
        private string dialog;

        public string Dialog
        {
            get { return dialog; }
            set { dialog = value; }
        }

        public List<string> Phrases { get; private set; }

        //Конструктор
        public Dialogs()
        {
            //Инициализируем List
            Phrases = new List<string>();
        }
    }
}
