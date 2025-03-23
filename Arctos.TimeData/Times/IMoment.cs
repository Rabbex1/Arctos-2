using System;

namespace Arctos.TimeData
{
    public interface IMoment : IComparable<IMoment>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        DateTime Time { get; set; }

        #endregion Properties
    }
}