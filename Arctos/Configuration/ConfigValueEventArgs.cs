using System;

namespace Arctos.Configuration
{
    public class ConfigValueEventArgs : EventArgs
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public ConfigValueAction Action { get; set; }
        public string Name { get; set; }
        public Type Type { get; set; }
        public object NewValue { get; set; }
        public object OldValue { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public ConfigValueEventArgs(ConfigValueAction action, string name, Type type, object newValue, object oldValue)
        {
            Action = action;
            Name = name;
            Type = type;
            NewValue = newValue;
            OldValue = oldValue;
        }
        
        #endregion Constructors

    }
}