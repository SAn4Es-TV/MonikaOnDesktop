using System.IO.Pipes;

namespace Monika.Shared {
    public static class PipeManager {
        //private const string PipeName = "MonikaInteractionPipe";

        // СЕРВЕР (Главное окно — слушает)
        public static async void StartServer(string PipeName, Action<string> onMessageReceived) {
            _ = Task.Run(async () => {
                while (true) // Цикл, чтобы сервер всегда ожидал новое подключение
                {
                    try {
                        using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In)) {
                            // Ждем клика от Шимеджи
                            await server.WaitForConnectionAsync();

                            using (var reader = new StreamReader(server)) {
                                var message = await reader.ReadToEndAsync();
                                if (!string.IsNullOrEmpty(message)) {
                                    onMessageReceived?.Invoke(message);
                                }
                            }
                        }
                    } catch (Exception ex) {
                        System.Diagnostics.Debug.WriteLine($"Ошибка сервера: {ex.Message}");
                    }
                }
            });
        }

        // КЛИЕНТ (Шимеджи — отправляет)
        public static void SendMessage(string PipeName, string message) {// Запускаем в отдельном потоке, чтобы UI Шимеджи не "фризил" при попытке подключения
            Task.Run(() => {
                try {
                    using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out)) {
                        // Пытаемся подключиться очень быстро (100-200 мс достаточно)
                        client.Connect(200);

                        using (var writer = new StreamWriter(client)) {
                            writer.Write(message);
                            writer.Flush();
                        }
                    }
                } catch (Exception) {
                }
            });
        }
    }
}
