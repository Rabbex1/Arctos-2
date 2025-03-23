using System;
using System.Collections.Generic;

namespace Arctos.Shell.Options
{
    public class OptionsPageChangedEventArgs : EventArgs
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public List<OptionPageProperty> NewProperties;
        public List<OptionPageProperty> OldProperties;

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public OptionsPageChangedEventArgs(List<OptionPageProperty> newProperties, List<OptionPageProperty> oldProperties)
        {
            NewProperties = newProperties;
            OldProperties = oldProperties;
        }

        #endregion Constructors

    }
}