using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace Arctos.Wpf.Charts
{
    [ContentProperty("Content")]
    public class AxisMarker : ScrollBar, IDataPresenterHelper
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private PropertyInfo? _propertyInfo;

        #endregion Fields
 
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public Axis? Axis { get; private set; }
        public bool IsShared { get; set; }

        #region DataPresenter
        private IDataPresenter? _dataPresenter;
        public IDataPresenter? DataPresenter
        {
            get => _dataPresenter;
            set
            {
                if (value == _dataPresenter)
                    return;
                _dataPresenter = value;
                if (value == null)
                    return;

                if (FocalDataAlignment != DataAlignment.None)
                    if (_dataPresenter != null)
                        _dataPresenter.FocalDataChanged += OnFocalDataChanged;

                if (AxisName != null)
                    Axis = _dataPresenter?.Axes.FirstOrDefault(a => a.Name == AxisName);
            }
        }
        #endregion

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        static AxisMarker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AxisMarker), new FrameworkPropertyMetadata(typeof(AxisMarker)));
            ValueProperty.OverrideMetadata
              (
                typeof(AxisMarker),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure, OnValueChanged)
              );
        }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Dependency Properties

        #region IsScrollable
        public static readonly DependencyProperty IsScrollableProperty = DependencyProperty.Register
        (
          "IsScrollable",
          typeof(bool),
          typeof(AxisMarker),
          new PropertyMetadata(true, OnIsScrollableChanged)
        );

        public bool IsScrollable
        {
            get => (bool)GetValue(IsScrollableProperty);
            set => SetValue(IsScrollableProperty, value);
        }

        private static void OnIsScrollableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion IsScrollable

        #region Content
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register
        (
          "Content",
          typeof(object),
          typeof(AxisMarker),
          new PropertyMetadata(default, OnContentChanged)
        );

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion Content

        #region FocalDataAlignment

        public static readonly DependencyProperty FocalDataAlignmentProperty = DependencyProperty.Register
        (
          "FocalDataAlignment",
          typeof(DataAlignment),
          typeof(AxisMarker),
          new PropertyMetadata(DataAlignment.None, OnFocalDataAlignmentChanged)
        );

        public DataAlignment FocalDataAlignment
        {
            get => (DataAlignment)GetValue(FocalDataAlignmentProperty);
            set => SetValue(FocalDataAlignmentProperty, value);
        }

        private static void OnFocalDataAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion FocalDataAlignment

        #region FocalDataProperty

        public static readonly DependencyProperty FocalDataPropertyProperty = DependencyProperty.Register
        (
          nameof(FocalDataProperty),
          typeof(string),
          typeof(AxisMarker),
          new PropertyMetadata(null, OnFocalDataPropertyChanged)
        );

        public string FocalDataProperty
        {
            get => (string)GetValue(FocalDataPropertyProperty);
            set => SetValue(FocalDataPropertyProperty, value);
        }

        private static void OnFocalDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion FocalDataProperty

        #region AxisName

        public static readonly DependencyProperty AxisNameProperty = DependencyProperty.Register
        (
          "AxisName",
          typeof(string),
          typeof(AxisMarker),
          new PropertyMetadata(null, OnAxisNameChanged)
        );

        public string? AxisName
        {
            get => (string?)GetValue(AxisNameProperty);
            set => SetValue(AxisNameProperty, value);
        }

        private static void OnAxisNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion AxisName

        #region DragAxis

        public static readonly DependencyProperty DragAxisProperty = DependencyProperty.Register
        (
          "DragAxis",
          typeof(bool),
          typeof(AxisMarker),
          new PropertyMetadata(default(bool), OnDragAxisChanged)
        );

        public bool DragAxis
        {
            get => (bool)GetValue(DragAxisProperty);
            set => SetValue(DragAxisProperty, value);
        }

        private static void OnDragAxisChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion DragAxis

        #region LineStyle
        public static readonly DependencyProperty LineStyleProperty = DependencyProperty.Register
        (
          "LineStyle",
          typeof(Style),
          typeof(AxisMarker),
          new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.AffectsParentMeasure, OnLineStyleChanged)
        );

        public Style LineStyle
        {
            get => (Style)GetValue(LineStyleProperty);
            set => SetValue(LineStyleProperty, value);
        }

        private static void OnLineStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AxisMarker {Parent: SharedAxisChart sac})
                return;
            sac.CreateLayout();
        }
        #endregion

        #endregion Dependency Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        //---------------------------------------------------------------------
        #region Public Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Value < Minimum || Value > Maximum)
                HideThumb();
        }

        #endregion Public Methods

        //---------------------------------------------------------------------
        #region Protected Methods

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsScrollable)
                e.Handled = true;
            else
                base.OnPreviewMouseLeftButtonDown(e);
        }

        #endregion Protected Methods

        //---------------------------------------------------------------------
        #region Private Methods

        public void HideThumb()
        {
            if (Template == null)
                return;
            var thumb = (Thumb)Template.FindName("PART_Thumb", this);
            thumb.Visibility = Visibility.Hidden;
        }

        #endregion Private Methods

        #endregion Methods

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Event Handlers

        public static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AxisMarker marker)
                return;

            var value = (double)e.NewValue;

            if (value < marker.Minimum || value > marker.Maximum)
                marker.HideThumb();
            if (marker.Axis != null)
            {
                if (marker.DragAxis)
                    marker.Axis.CurrentValueOffset = value;
                marker.Axis.InvalidateAxis();
            }
        }

        public void OnFocalDataChanged(object? sender, FocalDataEventArgs args)
        {
            if (FocalDataAlignment == DataAlignment.None)
                return;

            if (_propertyInfo == null)
                _propertyInfo = DataPresenter?.DataProvider?.PropertyInfo(FocalDataProperty);

            var visualRange = DataPresenter?.Axes.FirstOrDefault(a => a.Source == FocalDataProperty)?.VisualRange;
            if (visualRange == null)
                return;

            var propValue = _propertyInfo?.GetValue(args.Data);
            Value = propValue != null ? visualRange.ValueToFractional(propValue) : 0;
        }

        #endregion Event Handlers
    }
}
