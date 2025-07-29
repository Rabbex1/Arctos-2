using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Arctos.Wpf.Splash
{
    public static class SplashScreenManager
    {
        private static SplashScreenWindow? _window;

        public static void Show(string message = "Loading...", ImageSource? logo = null)
        {
            _window = new SplashScreenWindow();
            if (logo != null)
                _window.LogoSource = logo;

            _window.UpdateStatus(message);
            _window.Show();
        }

        public static void Update(string message)
        {
            _window?.Dispatcher.Invoke(() => _window.UpdateStatus(message));
        }

        public static void Close()
        {
            _window?.Dispatcher.Invoke(() => _window.Close());
            _window = null;
        }

        public static async Task RunWithSplash(Func<Task> asyncInit, string initialMessage = "Loading...", ImageSource? logo = null)
        {
            Show(initialMessage, logo);
            var stopwatch = Stopwatch.StartNew();

            await asyncInit();

            stopwatch.Stop();

            int remaining = 2000 - (int)stopwatch.ElapsedMilliseconds;
            if (remaining > 0)
                await Task.Delay(remaining);

            Close();
        }
    }
}
