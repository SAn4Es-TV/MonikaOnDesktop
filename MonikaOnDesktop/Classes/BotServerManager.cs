using System.Diagnostics;

namespace MonikaOnDesktop {
    public class BotServerManager {
        private Process _serverProcess;

        public void Start(string exePath) {
            if (_serverProcess != null && !_serverProcess.HasExited) return;

            _serverProcess = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true, // Скрываем черное окно консоли
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            _serverProcess.Start();
        }

        public void Stop() {
            if (_serverProcess != null && !_serverProcess.HasExited) {
                _serverProcess.Kill();
                _serverProcess.Dispose();
            }
        }
    }
}
