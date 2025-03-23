using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Arctos.Wpf.Charts
{
  public class TickIcon : Viewbox, ITickDecorator
  {
    public Point Center { get; set; }
  }
}
