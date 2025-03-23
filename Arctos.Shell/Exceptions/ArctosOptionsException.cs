namespace Arctos.Shell
{
    public class ArctosDocumentException : Exception
    {
        public ArctosDocumentException()
        {
        }

        public ArctosDocumentException(string message)
            : base(message)
        {
        }

        public ArctosDocumentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
