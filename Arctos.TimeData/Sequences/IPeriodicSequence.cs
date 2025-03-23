namespace Arctos.TimeData
{
    public interface IPeriodicSequence<TPeriod> : ISequence<TPeriod>
        where TPeriod : class, IPeriod, new()
    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        PeriodicSequence<Period> Gaps();
        PeriodicSequence<Period> Gaps(Period totalPeriod);
        void Insert(TPeriod item);
        ISequence<TPeriod>? Zip(PeriodicSequence<TPeriod> other);

        #endregion Methods
    }
}