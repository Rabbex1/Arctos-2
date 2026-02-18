using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Arctos.Wpf.Helpers
{
    public static class UiHelper
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public static void Thread(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
                return;
            }

            dispatcher.Invoke(action, DispatcherPriority.Normal);
        }

        public static void ThreadLocked(object syncLock, Action action)
        {
            ArgumentNullException.ThrowIfNull(syncLock);
            ArgumentNullException.ThrowIfNull(action);

            lock (syncLock)
            {
                Thread(action);
            }
        }

        public static Task ThreadAsync(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            var dispatcher = System.Windows.Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
                return Task.CompletedTask;
            }

            return dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task;
        }

        #endregion Methods
    }
}
