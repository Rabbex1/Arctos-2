using System.Windows;

namespace Arctos.Wpf.Charts
{
  public class RadialAxis : NumericAxis
  {
    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Dependency Properties

    #region InnerRadius
    public static readonly DependencyProperty InnerRadiusProperty = DependencyProperty.Register
    (
      "InnerRadius",
      typeof(double),
      typeof(RadialAxis),
      new PropertyMetadata(default(double), OnInnerRadiusChanged)
    );

    public double InnerRadius
    {
      get => (double)GetValue(InnerRadiusProperty);
      set => SetValue(InnerRadiusProperty, value);
    }

    private static void OnInnerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }
    #endregion InnerRadius  

    #endregion Dependency Properties

    //------------------------------------------------------------------------------------------------------------------------------------------
    #region Constructors

    static RadialAxis()
    {
      DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericAxis), new FrameworkPropertyMetadata(typeof(NumericAxis)));
    }

    public RadialAxis()
    {
      VisualRange = new VisualNumericRange();
      DataRange = new Range<double>();
    }

    #endregion Constructors
  }
}
