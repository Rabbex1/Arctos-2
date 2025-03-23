namespace Arctos.Shell
{
    public class ArctosOptionsException : Exception
    {
        public ArctosOptionsException()
        {
        }

        public ArctosOptionsException(string message)
            : base(message)
        {
        }

        public ArctosOptionsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
