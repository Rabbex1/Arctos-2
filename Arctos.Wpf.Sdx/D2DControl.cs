// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// This file is derived from a file under the copy right above. 
// Copyright was given by 2010-2012 SharpDX - Alexandre Mutel

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using SharpDX.Direct2D1;
using SharpDX.Direct3D10;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using Factory = SharpDX.Direct2D1.Factory;
using PixelFormat = SharpDX.Direct2D1.PixelFormat;

namespace Arctos.Wpf.Sdx
{
    using Device = SharpDX.Direct3D10.Device1;
    using FeatureLevel = SharpDX.Direct3D10.FeatureLevel;

    public abstract class D2DControl : System.Windows.Controls.Image, IDisposable
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private Device _device;
        private Texture2D _renderTarget;
        private DX10ImageSource _d3DSurface;
        protected RenderTarget D2DRenderTarget;
        protected Factory D2DFactory;

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public static bool IsInDesignMode
        {
            get
            {
                var prop = DesignerProperties.IsInDesignModeProperty;
                var isDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(prop, typeof(FrameworkElement)).Metadata.DefaultValue;
                return isDesignMode;
            }
        }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructor

        protected D2DControl()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            Stretch = Stretch.Fill;
        }

        #endregion Constructor

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public abstract void Render(RenderTarget target);


        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected void Refresh()
        {
            if (_d3DSurface == null)
                return;
            PrepareAndCallRender();
            _d3DSurface.InvalidateD3DImage();
        }

        #endregion Protected Methods

        //---------------------------------------------------------------------
        #region Private Methods

        private void StartD3D()
        {
            _device = new Device(DriverType.Hardware, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_10_0);

            _d3DSurface = new DX10ImageSource();

            CreateAndBindTargets();

            Source = _d3DSurface;
        }

        private void EndD3D()
        {
            Source = null;

            Disposer.SafeDispose(ref D2DRenderTarget);
            Disposer.SafeDispose(ref D2DFactory);
            Disposer.SafeDispose(ref _d3DSurface);
            Disposer.SafeDispose(ref _renderTarget);
            Disposer.SafeDispose(ref _device);
        }

        private void CreateAndBindTargets()
        {
            _d3DSurface.SetRenderTargetDX10(null);

            Disposer.SafeDispose(ref D2DRenderTarget);
            Disposer.SafeDispose(ref D2DFactory);
            Disposer.SafeDispose(ref _renderTarget);

            var width = Math.Max((int)ActualWidth, 100);
            var height = Math.Max((int)ActualHeight, 100);

            var colorDescription = new Texture2DDescription
            {
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                Format = Format.B8G8R8A8_UNorm,
                Width = width,
                Height = height,
                MipLevels = 1,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                OptionFlags = ResourceOptionFlags.Shared,
                CpuAccessFlags = CpuAccessFlags.None,
                ArraySize = 1
            };

            _renderTarget = new Texture2D(_device, colorDescription);

            var surface = _renderTarget.QueryInterface<Surface>();

            D2DFactory = new Factory();
            var rtp = new RenderTargetProperties(new PixelFormat(Format.Unknown, SharpDX.Direct2D1.AlphaMode.Premultiplied));
            D2DRenderTarget = new RenderTarget(D2DFactory, surface, rtp);

            _d3DSurface.SetRenderTargetDX10(_renderTarget);
        }

        private void PrepareAndCallRender()
        {
            var device = _device;
            if (device == null)
            {
                return;
            }

            var renderTarget = _renderTarget;
            if (renderTarget == null)
            {
                return;
            }

            var targetWidth = (int)ActualWidth;// renderTarget.Description.Width;
            var targetHeight = (int)ActualHeight;// renderTarget.Description.Height;

            device.Rasterizer.SetViewports(new[]
            {
                  new RawViewport()
                  {
                    X = 0,
                    Y = 0,
                    Width = targetWidth,
                    Height = targetHeight,
                    MaxDepth = 0,
                    MinDepth = 0
                  }
            });

            // Does the actual rendering
            if (Visibility == Visibility.Visible)
            {
                D2DRenderTarget.BeginDraw();
                Render(D2DRenderTarget);
                D2DRenderTarget.EndDraw();

                device.Flush();
            }
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (IsInDesignMode)
            {
                return;
            }

            StartD3D();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (IsInDesignMode)
            {
                return;
            }

            EndD3D();
        }

        public void OnRender(object sender, EventArgs e)
        {
            Refresh();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (_d3DSurface == null)
                return;
            CreateAndBindTargets();
            base.OnRenderSizeChanged(sizeInfo);
            PrepareAndCallRender();
            _d3DSurface.InvalidateD3DImage();
        }

        #endregion Event Handlers

        public void Dispose()
        {
            Disposer.SafeDispose(ref D2DRenderTarget);
            Disposer.SafeDispose(ref D2DFactory);
            Disposer.SafeDispose(ref _d3DSurface);
            Disposer.SafeDispose(ref _renderTarget);
            Disposer.SafeDispose(ref _device);
            Loaded -= OnLoaded;
            Unloaded -= OnUnloaded;
        }
    }
}