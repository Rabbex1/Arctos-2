namespace Arctos.Plugins
{
    public interface IArctosPlugin
    {
        void Initialize(IServiceProvider services);
        void Terminate(IServiceProvider services);
    }
}
