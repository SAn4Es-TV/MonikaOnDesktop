using System;
using System.Collections.Generic;
using System.Text;

namespace MonikaOnDesktop.Models
{
    class Dialog
    {
        public int Type { get; set; }
        public Object Obj { get; set; }
        public Dialog(int type, Object obj)
        {
            this.Type = type;
            this.Obj = obj;
        }
        public Dialog(Object obj)
        {
            this.Type = 0;
            this.Obj = obj;
        }
        /*
        string[][] text = new string[2][] { new[] { "Привет!", "Как дела?", "У меня всё отлично" }, new[] { "Утречко!", "Что делаешь?" } }; // набор строк
        public async Task showTextAsync(string[] text)
        {
            _ = this.Dispatcher.Invoke(async () =>
              {
            int delay = 0; // задержка 
            foreach (string t in text)
            {
                for (int i = 0; i < t.Length; i++) //бежим по всем символам
                {
                    this.textBlock.Text += t[i]; //добавляем символ в текстовое поле
                    await Task.Delay(30); //задержка для побуквенного появления текста

                }
                delay = t.Length * 30 + 300;
                await Task.Delay(delay);//задержка от обЪявления функции до её конца 
                textBlock.Text = ""; //чистим поле
            }
              });
        }
        public async Task showTextAsync()
        {
            foreach (string[] t in text)
            {
                textWindow.Visibility = Visibility.Visible;//показываем поле
                await showTextAsync(t);//воспроизводим текст
                textWindow.Visibility = Visibility.Hidden;//скрываем поле
            }
        }*/
    }
}
