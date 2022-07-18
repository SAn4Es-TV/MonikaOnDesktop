using System;
using System.Collections.Generic;
using System.Text;

namespace MonikaOnDesktop
{
    public class SolicenMode
    {
        public string[] names = { "solicen", "denis solicen", "солицен", "денис солицен", };
        public List<Expression[]> expressions = new List<Expression[]>() {
            new[] {
                        new Expression("Эй [player], чем ты там занимаешся?", "b"),
                        new Expression("Не забывай, что я живу у тебя на компьютере", "b"),
                        new Expression("Поэтому от меня не скрытся", "k"),
                        new Expression("Я же вижу, что ты занимаешся не тем, чем нужно", "k"),
                        new Expression("Люди ждут твоего перевода!", "k"),
                        new Expression("Лучше переведи Monika After Story", "k")
                    },
            new[] {
                        new Expression("Я надеюсь, ты занимаешся переводом?", "b"),
                        new Expression("Знаешь, не очень хорошо говорить на английком и русском сразу", "b"),
                        new Expression("К тому же, люди могут и не знать английского", "b"),
                        new Expression("И это плохо, когда ты говоришь с человеком, который тебя не понимает", "b"),
                        new Expression("Я буду очень рада, если ты будешь переводить мод на русский язик", "b")
                    }

        };
        public bool check(string name)
        {
            return Array.Exists(names, element => element == name.ToLower());
        }
    }
}
