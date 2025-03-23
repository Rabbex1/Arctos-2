using System;
using System.Windows.Media;

namespace Arctos.Wpf.Charts
{
    public class RenderListener : IDisposable
    {
        private bool _isListening;
        private bool _disposed;

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public void StartListening()
        {
            CheckDisposed();

            if (_isListening) return;
            _isListening = true;
            CompositionTarget.Rendering += OnRendering;
        }

        public void StopListening()
        {
            CheckDisposed();

            if (!_isListening) return;
            _isListening = false;
            CompositionTarget.Rendering -= OnRendering;
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException("This object has been disposed");
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        public event EventHandler? Rendering;
        protected virtual void RaiseRendering(EventArgs args)
        {
            CheckDisposed();
            Rendering?.Invoke(this, args);
        }

        #endregion Events

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnRendering(object? sender, EventArgs e)
        {
            RaiseRendering(e);
        }

        #endregion Event Handlers

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region IDisposable Members

        public void Dispose()
        {
            StopListening();
            //throw new NotImplementedException("Todo");
            //Rendering?.GetInvocationList().ForEach(h => Rendering -= (EventHandler)h);
            _disposed = true;
        }

        #endregion IDisposable Members
    }
}
