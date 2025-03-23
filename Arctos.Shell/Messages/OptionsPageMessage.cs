using Arctos.Shell.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arctos.Shell.Messages
{
    public class OptionsPageMessage
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public OptionsPage Page { get; }
        public List<OptionPageProperty> NewProperties { get; }
        public List<OptionPageProperty> OldProperties { get; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public OptionsPageMessage(OptionsPage page, List<OptionPageProperty> newProperties, List<OptionPageProperty> oldProperties)
        {
            Page = page;
            NewProperties = newProperties;
            OldProperties = oldProperties;
        }

        #endregion Constructors
    }
}
