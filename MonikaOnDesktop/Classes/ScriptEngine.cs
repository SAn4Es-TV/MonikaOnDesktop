using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MonikaOnDesktop {
    public enum CommandType { Speech, Menu, Jump, Scene, If, Variable, Random, NoRandom, Return, Quit, Call, Input }

    public class GameCommand {
        public CommandType Type { get; set; }
        public string Character { get; set; }
        public string Text { get; set; }
        public string Target { get; set; }
        public Dictionary<string, string> Options { get; set; }

        public int IndentLevel;

        // Для переменных и условий
        public string VarName, VarOperator;
        public string VarValue;
        public object VarObjectValue;
    }
    public class ScriptEngine {
        public Func<string, string, Task> OnSpeech;
        public Action<int> OnMenuRequired;
        public Action OnScriptFinished;
        public Action OnQuitCalled;
        public Action<string> OnLog;
        public Action<string, string> OnInputRequired;

        // Данные скрипта
        public List<GameCommand> Commands = new List<GameCommand>();
        public Dictionary<string, int> Labels = new Dictionary<string, int>();
        public Dictionary<string, object> Variables { get; private set; } = new Dictionary<string, object>();
        public Dictionary<string, Action> RegisteredFunctions { get; set; } = new Dictionary<string, Action>();

        public int CurrentIndex = 0;
        public bool isRandom = false;

        public bool isAuto = true;

        public void LoadContent(string content) {
            Commands.Clear();
            Labels.Clear();
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++) {
                string rawLine = lines[i];
                string trimmed = rawLine.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;

                int indent = rawLine.TakeWhile(c => c == ' ').Count();

                if (trimmed.StartsWith("label ")) {
                    string labelName = trimmed.Replace("label ", "").Replace(":", "").Trim();
                    Labels[labelName] = Commands.Count;
                } else if (trimmed.StartsWith("menu:")) {
                    Commands.Add(new GameCommand { Type = CommandType.Menu, IndentLevel = indent });
                } else if (trimmed.StartsWith("if ")) {
                    var m = Regex.Match(trimmed, @"if\s+(\w+)\s*([><=]+)\s*(.*):");
                    if (m.Success)
                        Commands.Add(new GameCommand {
                            Type = CommandType.If,
                            VarName = m.Groups[1].Value,
                            VarOperator = m.Groups[2].Value,
                            VarValue = m.Groups[3].Value,
                            IndentLevel = indent
                        });
                } else if (trimmed.StartsWith("$") && trimmed.Contains("= input")) {
                    // Извлекаем имя переменной из строки "$ player_name = input"
                    string varName = trimmed.Replace("$", "").Replace("= input", "").Trim();

                    var m = Regex.Match(trimmed, @"\$ (.*) = input\[""(.*)""]");
                    Debug.WriteLine(@"\$ (.*) = input\[""(.*)""]");
                    if (m.Success)
                        Commands.Add(new GameCommand {
                            Type = CommandType.Input,
                            VarName = m.Groups[1].Value,
                            Text = m.Groups[2].Value,
                            IndentLevel = indent
                        });

                } else if (trimmed.StartsWith("$")) {
                    var p = trimmed.Replace("$", "").Trim().Split(new[] { ' ' }, 3);
                    if (p.Length >= 3) {
                        string varName = p[0];
                        string op = p[1];
                        string rawVal = p[2].Trim();

                        object finalVal;
                        if (rawVal.StartsWith("'") || rawVal.StartsWith("\"")) {
                            finalVal = rawVal.Trim('\'', '\"');
                        } else if (int.TryParse(rawVal, out int intVal)) {
                            finalVal = intVal;
                        } else {
                            finalVal = rawVal;
                        }

                        Commands.Add(new GameCommand {
                            Type = CommandType.Variable,
                            IndentLevel = indent,
                            VarName = varName,
                            VarOperator = op,
                            VarObjectValue = finalVal
                        });
                    }
                } else if (trimmed.StartsWith("jump ")) {
                    Commands.Add(new GameCommand { Type = CommandType.Jump, Target = trimmed.Split(' ')[1], IndentLevel = indent });
                } else if (trimmed.StartsWith("return \"quit\"")) {
                    Commands.Add(new GameCommand { Type = CommandType.Quit, IndentLevel = indent });
                } else if (trimmed.StartsWith("return")) {
                    Commands.Add(new GameCommand { Type = CommandType.Return, IndentLevel = indent });
                } else if (trimmed.StartsWith("random")) {
                    Commands.Add(new GameCommand { Type = CommandType.Random, IndentLevel = indent });
                } else if (trimmed.StartsWith("norandom")) {
                    Commands.Add(new GameCommand { Type = CommandType.NoRandom, IndentLevel = indent });
                } else if (trimmed.StartsWith("call_func ")) {
                    string funcName = trimmed.Replace("call_func ", "").Trim();
                    Commands.Add(new GameCommand {
                        Type = CommandType.Call, // Не забудь добавить Call в enum CommandType
                        Target = funcName,
                        IndentLevel = indent
                    });
                } else if (trimmed.Contains("\"")) {
                    if (!trimmed.EndsWith(':')) {
                        var parts = trimmed.Split('\"');
                        string charId = (trimmed[0] != 'm' && trimmed[0] != '[') ? "null" : parts[0].TrimEnd();
                        Commands.Add(new GameCommand { Type = CommandType.Speech, Character = charId, Text = parts[1], IndentLevel = indent });
                    } else {
                        var parts = trimmed.Split('\"');
                        string charId = (trimmed[0] != 'm' && trimmed[0] != '[') ? "null" : parts[0].TrimEnd();
                        Commands.Add(new GameCommand { Type = CommandType.Speech, Character = charId, Text = parts[1] + ":", IndentLevel = indent });
                    }

                }
            }
        }

        public async Task ExecuteNext() {
            int startIndent = (CurrentIndex < Commands.Count) ? Commands[CurrentIndex].IndentLevel : -1;

            while (CurrentIndex < Commands.Count) {
                var cmd = Commands[CurrentIndex];
                if (startIndent != -1 && cmd.IndentLevel < startIndent) break;
                if (cmd.Type == CommandType.Speech && cmd.Text.EndsWith(":")) {
                    break;
                }

                switch (cmd.Type) {
                    case CommandType.Speech:
                        CurrentIndex++;
                        if (OnSpeech != null) await OnSpeech(cmd.Text, cmd.Character);
                        if (!isAuto)
                            return; // Останавливаемся и ЖДЕМ КЛИКА игрока;
                        else
                            continue; // Останавливаемся и ЖДЕМ КЛИКА игрока
                    case CommandType.Return:
                        CurrentIndex = Commands.Count;
                        break;
                    case CommandType.Variable:
                        UpdateVariable(cmd.VarName, cmd.VarOperator, cmd.VarObjectValue);
                        CurrentIndex++;
                        break;

                    case CommandType.If:
                        if (CheckCondition(cmd.VarName, cmd.VarOperator, cmd.VarValue)) CurrentIndex++;
                        else SkipBlock(cmd.IndentLevel);
                        break;

                    case CommandType.Menu:
                        if (isRandom) {
                            AutoSelectRandomOptionAsync(cmd.IndentLevel);
                        } else {
                            OnMenuRequired?.Invoke(cmd.IndentLevel);
                        }
                        return;
                    case CommandType.Input:
                        OnInputRequired?.Invoke(cmd.VarName, cmd.Text);
                        CurrentIndex++;
                        return; // Останавливаем выполнение, пока пользователь не введет текст
                    case CommandType.Random:
                        isRandom = true;
                        CurrentIndex++;
                        break;
                    case CommandType.NoRandom:
                        isRandom = false;
                        CurrentIndex++;
                        break;
                    case CommandType.Quit:
                        OnQuitCalled?.Invoke();
                        return;

                    case CommandType.Jump:
                        if (Labels.ContainsKey(cmd.Target)) {
                            CurrentIndex = Labels[cmd.Target];
                            startIndent = Commands[CurrentIndex].IndentLevel;
                        }
                        break;
                    case CommandType.Call:
                        if (RegisteredFunctions.ContainsKey(cmd.Target)) {
                            // Вызываем функцию в главном потоке (важно для UI)
                            App.Current.Dispatcher.Invoke(() => RegisteredFunctions[cmd.Target]());
                        } else {
                            OnLog?.Invoke($"[Error] Функция {cmd.Target} не зарегистрирована!");
                        }
                        CurrentIndex++;
                        break;
                }
            }
            if (CurrentIndex >= Commands.Count)
                OnScriptFinished?.Invoke();
        }
        private async Task AutoSelectRandomOptionAsync(int menuIndent) {
            List<int> optionIndices = new List<int>();
            int i = CurrentIndex + 1;

            while (i < Commands.Count && Commands[i].IndentLevel > menuIndent) {
                if (Commands[i].Type == CommandType.Speech && Commands[i].IndentLevel == menuIndent + 4) {
                    optionIndices.Add(i + 1);
                }
                i++;
            }

            if (optionIndices.Count > 0) {
                Random rnd = new Random();
                CurrentIndex = optionIndices[rnd.Next(optionIndices.Count)];


                await ExecuteNext();
                SkipBlock(menuIndent);
                await ExecuteNext();
            } else {
                isRandom = false;
                CurrentIndex++; // Если кнопок нет, просто идем дальше
            }
        }
        public void SkipBlock(int currentIndent) {
            CurrentIndex++;
            while (CurrentIndex < Commands.Count && Commands[CurrentIndex].IndentLevel > currentIndent)
                CurrentIndex++;
        }
        private bool CheckCondition(string name, string op, object targetVal) {
            if (!Variables.ContainsKey(name)) return false;

            var current = Variables[name];
            string target = targetVal.ToString().Replace("\"", "");
            if (int.TryParse(target, out int number)) {
                if (current is int i1 && number is int i2) {
                    return op switch {
                        ">" => i1 > i2,
                        "<" => i1 < i2,
                        "==" => i1 == i2,
                        ">=" => i1 >= i2,
                        "<=" => i1 <= i2,
                        "!=" => i1 != i2,
                        _ => false
                    };
                }
            } else {
                if (current is string curStr || targetVal is string) {
                    string s1 = current.ToString();
                    string s2 = targetVal.ToString().Replace("\"", "");
                    return op == "==" ? s1 == s2 : s1 != s2;
                }
            }

            return false;
        }
        private void UpdateVariable(string name, string op, object newVal) {
            if (!Variables.ContainsKey(name))
                Variables[name] = newVal;
            var current = Variables[name];

            if (op == "=") {
                Variables[name] = newVal;
            } else if (op == "+=") {
                if (current is int i1 && newVal is int i2) Variables[name] = i1 + i2;
                else if (current is string s) Variables[name] = s + newVal.ToString();
            } else if (op == "-=") {
                if (current is int i1 && newVal is int i2) Variables[name] = i1 - i2;
            }
        }
        public string GetRandomLabel() {
            if (Labels.Count == 0) return null;
            var keys = Labels.Keys.ToList();
            Random rnd = new Random();
            return keys[rnd.Next(keys.Count)];
        }

        public string GetLabelByIndex(int index) {
            if (index < 0 || index >= Labels.Count) return null;
            return Labels.Keys.ElementAt(index);
        }
        public void RegisterFunction(string name, Action action) {
            RegisteredFunctions[name] = action;
        }
    }
}