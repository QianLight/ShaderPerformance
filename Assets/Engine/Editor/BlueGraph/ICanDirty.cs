
namespace CFEngine.Editor
{
    public interface ICanDirty
    {
        void OnDirty();
        void OnUpdate();
    }
}
