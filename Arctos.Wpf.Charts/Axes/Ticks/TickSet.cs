using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Arctos.Utils;
using Arctos.Wpf.Helpers;

namespace Arctos.Wpf.Charts
{
  public class TickSet : Canvas
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Properties

    public ObjectPool<Tick> Ticks { get; set; }

    public double CurrentFrequency { get; set; }

    public string CurrentLabelFormat { get; set; } = string.Empty;

    #region Owner
    private Axis? _owner;
    public Axis? Owner
    {
      get
      {
        if (_owner != null) return _owner;
        if (Parent is not Axis axis)
          throw new ArgumentException("the parent of a TickSet must be Axis");
        _owner = axis;
        return _owner;
      }
    }
    #endregion

    #endregion Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructor

    public TickSet()
    {
      Ticks = new ObjectPool<Tick>(GlyphCreator);
    }

    #endregion Constructor

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Dependency Properties

    #region Glyph
    public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register
    (
      "Glyph",
      typeof(DataTemplate),
      typeof(TickSet),
      new PropertyMetadata(default(DataTemplate), OnGlyphChanged)
    );

    public DataTemplate Glyph
    {
      get => (DataTemplate)GetValue(GlyphProperty);
      set => SetValue(GlyphProperty, value);
    }

    private static void OnGlyphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region Rank
    public static readonly DependencyProperty RankProperty = DependencyProperty.Register
    (
      "Rank",
      typeof(int),
      typeof(TickSet),
      new PropertyMetadata(1, OnRankChanged)
    );

    public int Rank
    {
      // ReSharper disable once PossibleNullReferenceException
      get => (int)GetValue(RankProperty);
      set => SetValue(RankProperty, value);
    }

    private static void OnRankChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region Frequency

    public static readonly DependencyProperty FrequencyProperty = DependencyProperty.Register
    (
      nameof(Frequency),
      typeof(double),
      typeof(TickSet),
      new PropertyMetadata(0.0, OnFrequencyChanged)
    );

    public double Frequency
    {
      get => (double)GetValue(FrequencyProperty);
      set => SetValue(FrequencyProperty, value);
    }

    private static void OnFrequencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is not TickSet tickSet)
        return;
      tickSet.CurrentFrequency = (double)e.NewValue;
    }

    #endregion

    #region IsLabeled
    public static readonly DependencyProperty IsLabeledProperty = DependencyProperty.Register
    (
      "IsLabeled",
      typeof(bool),
      typeof(TickSet),
      new PropertyMetadata(false, OnIsLabeledChanged)
    );

    public bool IsLabeled
    {
      // ReSharper disable once PossibleNullReferenceException
      get => (bool)GetValue(IsLabeledProperty);
      set => SetValue(IsLabeledProperty, value);
    }

    private static void OnIsLabeledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region HasIcons
    public static readonly DependencyProperty HasIconsProperty = DependencyProperty.Register
    (
      "HasIcons",
      typeof(bool),
      typeof(TickSet),
      new PropertyMetadata(default(bool), OnHasIconsChanged)
    );

    public bool HasIcons
    {
      get => (bool)GetValue(HasIconsProperty);
      set => SetValue(HasIconsProperty, value);
    }

    private static void OnHasIconsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion HasIcons

    #region IconConverter

    public static readonly DependencyProperty IconConverterProperty = DependencyProperty.Register
    (
      "IconConverter",
      typeof(IValueConverter),
      typeof(TickSet),
      new PropertyMetadata(default(IValueConverter), OnIconConverterChanged)
    );

    public IValueConverter IconConverter
    {
      get => (IValueConverter)GetValue(IconConverterProperty);
      set => SetValue(IconConverterProperty, value);
    }

    private static void OnIconConverterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion IconConverter

    #region LabelFormat
    public static readonly DependencyProperty LabelFormatProperty = DependencyProperty.Register
    (
      "LabelFormat",
      typeof(string),
      typeof(TickSet),
      new PropertyMetadata(default(string?), OnLabelFormatChanged)
    );

    public string? LabelFormat
    {
      get => (string?)GetValue(LabelFormatProperty);
      set => SetValue(LabelFormatProperty, value);
    }

    private static void OnLabelFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TickSet tickSet || e.NewValue is not string label)
            return;
        tickSet.CurrentLabelFormat = label;
    }
    #endregion

    #region LabelForeground
    public static readonly DependencyProperty LabelForegroundProperty = DependencyProperty.Register
    (
      "LabelForeground",
      typeof(SolidColorBrush),
      typeof(TickSet),
      new PropertyMetadata(Brushes.AliceBlue, OnLabelForegroundChanged)
    );

    public SolidColorBrush LabelForeground
    {
      get => (SolidColorBrush)GetValue(LabelForegroundProperty);
      set => SetValue(LabelForegroundProperty, value);
    }

    private static void OnLabelForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region LabelFontSize
    public static readonly DependencyProperty LabelFontSizeProperty = DependencyProperty.Register
    (
      "LabelFontSize",
      typeof(double),
      typeof(TickSet),
      new PropertyMetadata(9.0, OnLabelFontSizeChanged)
    );

    public double LabelFontSize
    {
      // ReSharper disable once PossibleNullReferenceException
      get => (double)GetValue(LabelFontSizeProperty);
      set => SetValue(LabelFontSizeProperty, value);
    }

    private static void OnLabelFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion LabelFontSize

    #region IsAuto
    public static readonly DependencyProperty IsAutoProperty = DependencyProperty.Register
    (
      "IsAuto",
      typeof(bool),
      typeof(TickSet),
      new PropertyMetadata(false, OnIsAutoChanged)
    );

    public bool IsAuto
    {
      // ReSharper disable once PossibleNullReferenceException
      get => (bool)GetValue(IsAutoProperty);
      set => SetValue(IsAutoProperty, value);
    }

    private static void OnIsAutoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region MinimumSpacing
    public static readonly DependencyProperty MinimumSpacingProperty = DependencyProperty.Register
    (
      "MinimumSpacing",
      typeof(double),
      typeof(TickSet),
      new PropertyMetadata(0.0, OnMinimumSpacingChanged)
    );

    public double MinimumSpacing
    {
      // ReSharper disable once PossibleNullReferenceException
      get => (double)GetValue(MinimumSpacingProperty);
      set => SetValue(MinimumSpacingProperty, value);
    }

    private static void OnMinimumSpacingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region IsGridLine
    public static readonly DependencyProperty IsGridLineProperty = DependencyProperty.Register
    (
      "IsGridLine",
      typeof(bool),
      typeof(TickSet),
      new PropertyMetadata(default(bool), OnIsGridLineChanged)
    );

    public bool IsGridLine
    {
      // ReSharper disable once PossibleNullReferenceException
      get => (bool)GetValue(IsGridLineProperty);
      set => SetValue(IsGridLineProperty, value);
    }

    private static void OnIsGridLineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion

    #region HideTicks
    public static readonly DependencyProperty HideTicksProperty = DependencyProperty.Register
    (
      "HideTicks",
      typeof(bool),
      typeof(TickSet),
      new PropertyMetadata(false, OnHideTicksChanged)
    );

    public bool HideTicks
    {
      // ReSharper disable once PossibleNullReferenceException
      get => (bool)GetValue(HideTicksProperty);
      set => SetValue(HideTicksProperty, value);
    }

    private static void OnHideTicksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion 


    #endregion Dependency Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Methods

    public List<Tick> GetMarkers()
    {
      var ticks = new List<Tick>();
      Ticks.ForEach(m => ticks.Add(m));
      return ticks;
    }

    protected bool IsTickVisible(object position, Tick marker, Size constraint)
    {
      double half;
      var visualUnits = Owner?.VisualRange?.ValueToVisual(position);

      if (Owner?.Orientation == PartOrientation.Horizontal)
      {
        half = marker.DesiredSize.Width / 2;
        if (visualUnits - half < 0 && visualUnits + half < 0)
          return false;
        return !(visualUnits - half > constraint.Width) || !(visualUnits + half > constraint.Width);
      }
      half = marker.DesiredSize.Height / 2;
      if (visualUnits - half < 0 && visualUnits + half < 0)
        return false;
      return !(visualUnits - half > constraint.Height) || !(visualUnits + half > constraint.Height);
    }

    protected virtual double ToVisualUnits(object source)
    {
      throw new NotImplementedException();
    }

    private Tick GlyphCreator()
    {
      var h = new Tick((UIElement)Glyph.LoadContent());
      return h;
    }

    #endregion Methods

  }
}
