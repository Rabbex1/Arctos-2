using Arctos.Messenger;
using Arctos.Wpf.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;

namespace Arctos.Wpf.Charts
{
    public class GLCanvas : ChartCanvas, IRecipient<DocumentMessage>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        public GLCanvas()
        {
            Graphics = new ObservableCollection<GLGraphic>();
            WeakReferenceMessenger.Default.Register<DocumentMessage>(this);
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region Graphics
        public static readonly DependencyProperty GraphicsProperty = DependencyProperty.Register
        (
          "Graphics",
          typeof(ObservableCollection<GLGraphic>),
          typeof(GLCanvas),
          new PropertyMetadata(null, OnGraphicsChanged)
        );

        public ObservableCollection<GLGraphic> Graphics
        {
            get => (ObservableCollection<GLGraphic>)GetValue(GraphicsProperty);
            set => SetValue(GraphicsProperty, value);
        }

        private static void OnGraphicsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var canvas = d as GLCanvas;
            if (canvas == null)
                return;
            var newGraphics = e.NewValue as ObservableCollection<GLGraphic>;
            var oldGraphics = e.OldValue as ObservableCollection<GLGraphic>;
            canvas.GraphicsChanged(newGraphics, oldGraphics);
        }
        #endregion

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in Children)
                child.Measure(availableSize);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in Children)
                child.Arrange(new Rect(new Point(0, 0), finalSize));
            RaiseRender();
            return finalSize;
        }

        #endregion Protected Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private void GraphicsChanged(ObservableCollection<GLGraphic>? newGraphics, ObservableCollection<GLGraphic>? oldGraphics)
        {
            if (oldGraphics != null)
            {
                foreach (var graphic in oldGraphics)
                {
                    Children.Remove(graphic);
                    Render -= graphic.OnRender;
                    graphic.Dispose();
                }
                oldGraphics.CollectionChanged -= OnGraphicsCollectionChanged;
            }

            if (newGraphics != null)
            {
                foreach (var graphic in newGraphics)
                {
                    Children.Add(graphic);
                    graphic.Start();
                    graphic.Initialize();
                    Render += graphic.OnRender;
                }
                newGraphics.CollectionChanged += OnGraphicsCollectionChanged;
            }
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnGraphicsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (GLGraphic graphic in e.OldItems)
                {
                    Children.Remove(graphic);
                    Render -= graphic.OnRender;
                    graphic.Dispose();
                }
            }

            if (e.NewItems != null)
            {
                foreach (GLGraphic graphic in e.NewItems)
                {
                    Children.Add(graphic);
                    graphic.Start();
                    graphic.Initialize();
                    Render += graphic.OnRender;
                }
            }
        }

        //private void OnClosed(object? sender, EventArgs e)
        //{
        //    foreach (GLGraphic graphic in Children)
        //        graphic.Dispose();
        //}

        #endregion Event Handlers

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Events

        public event EventHandler<EventArgs>? Render;
        public void RaiseRender()
        {
            Render?.Invoke(this, EventArgs.Empty);
        }

        #endregion Events

        public void Receive(DocumentMessage message)
        {
            if (message.Action != DocumentAction.DocumentRemoved)
                return;
            if (message.Params?[0] == DataContext)
            {
                foreach (GLGraphic graphic in Children)
                    graphic.Dispose();
            }
        }
    }
}


