using System;
using Arctos.Utils;

namespace Arctos.Shell.Options
{
    public class OptionsPageKey : IEquatable<OptionsPageKey>

    {
        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Fields

        private readonly Guid _guid = Guid.NewGuid();

        #endregion Fields

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Properties

        public string CategoryId { get; set; } = null!;
        public string PageId { get; set; } = null!;
        public Guid OwnerId { get; set; }

        #endregion Properties

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Constructors

        public OptionsPageKey(string categoryId, string pageId, Guid ownerId)
        {
            Argument.IsNotNullOrWhiteSpace("categoryId", categoryId);
            Argument.IsNotNullOrWhiteSpace("pageId", categoryId);
            Argument.IsNotNull("ownerId", ownerId);

            CategoryId = categoryId;
            PageId = pageId;
            OwnerId = ownerId;
        }

        public OptionsPageKey(OptionsPageKey other)
        {
            Argument.IsNotNull("other", other);

            CategoryId = other.CategoryId;
            PageId = other.PageId;
            OwnerId = other.OwnerId;
        }

        public OptionsPageKey()
        { }

        #endregion Constructors

        //------------------------------------------------------------------------------------------------------------------------------------------
        #region Methods

        public bool Equals(OptionsPageKey? other)
        {
            if (other == null)
                return false;
            var equal = string.Equals(CategoryId, other.CategoryId) && string.Equals(PageId, other.PageId) && OwnerId == other.OwnerId;
            return equal;
        }

        public override bool Equals(object? obj)
        {
            var other = obj as OptionsPageKey;
            return !ReferenceEquals(other, null) && Equals(other);
        }

        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        #endregion Methods


    }
}
