namespace Arctos.ConfigXml
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class XmlAfterDeserialized : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class XmlBeforeSerialized : Attribute
    {
    }

}
