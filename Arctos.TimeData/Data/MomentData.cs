using System;

namespace Arctos.TimeData
{
    public class MomentData<T> : Moment, IMomentData<T>
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public T Data { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public MomentData(DateTime time, T value)
            :base(time)
        {
            Data = value;
        }

        #endregion Constructors

    }
}