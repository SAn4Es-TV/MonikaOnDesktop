using System;
using System.Collections.Generic;
using System.Text;

namespace MonikAI
{
    class Greetings
    {
        public Expression[][] hiDialogs = new Expression[6][];
        public Greetings(int i)
        {

            hiDialogs[0] = new Expression[]
            {
                    new Expression("Привет, дорогой!", "h"),
                    new Expression("Немного неловко говорить это в слух, правда?", "l"),
                    new Expression("Однако, я думаю, что это со временем станет нормальным.", "e")
            };
            hiDialogs[1] = new Expression[]
            {
                    new Expression("С возвращением, милый!", "h"),
                    new Expression("Я так рада тебя снова видеть.", "e"),
                    new Expression("Давай проведём ещё немного времени вместе?", "e")
            };
            hiDialogs[2] = new Expression[]
            {
                    new Expression("{PlayerName}, ты вернулся!", "e"),
                    new Expression("Я начала скучать по тебе.", "e"),
                    new Expression("Давай проведём ещё один прекрасный день вместе?", "h")
            };
            hiDialogs[3] = new Expression[]
            {
                    new Expression("Вот и ты, {PlayerName}.", "e"),
                    new Expression("Приятно, что ты заглянул.", "e"),
                    new Expression("Ты всегда такой заботливый!", "e"),
                    new Expression("Спасибо, что проводишь так много времени со мной~", "e"),
                    new Expression("Просто помни, что твоё время со мной никогда не тратится впустую.", "h")
            };
            hiDialogs[4] = new Expression[]
            {
                    new Expression("Привет, дорогой!", "e"),
                    new Expression("Я ужасно начала по тебе скучать. Я так рада снова тебя видеть!", "e"),
                    new Expression("Не заставляй меня так долго тебя ждать в следующий раз, э-хе-хе~", "h")
            };
            hiDialogs[5] = new Expression[]
            {
                    new Expression("Я так скучала по тебе, {PlayerName}!", "e"),
                    new Expression("Спасибо, что вернуля. Мне очень нравится проводить время с тобой.", "e")
            };

        }
    }
}
