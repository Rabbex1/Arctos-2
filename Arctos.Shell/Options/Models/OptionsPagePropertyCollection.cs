using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Shell.Options
{
    public class OptionPagePropertyCollection : OptionPageProperty
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public OptionPagePropertyCollection(string name, IEnumerable value, Type type) 
            : base(name, new List<OptionPageProperty>(), type)
        {
            foreach (var item in value)
            {
                var newElement = new OptionPageProperty("element", item, item.GetType());
                Add(newElement);
            }
        }

        public OptionPagePropertyCollection(string name, Type type) 
            : base(name, type)
        {
            Value = new List<OptionPageProperty>();
        }

        #endregion Constructors

        public void Add(OptionPageProperty item)
        {
            var content = Value as List<OptionPageProperty>;
            content?.Add(item);
            item.PropertyChanged += (sender, args) => Changed = true;
        }
    }
}
