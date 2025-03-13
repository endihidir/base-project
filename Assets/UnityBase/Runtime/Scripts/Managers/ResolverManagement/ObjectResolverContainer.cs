using VContainer;

namespace UnityBase.Manager
{
    public class ObjectResolverContainer : IObjectResolverContainer
    {
        private IObjectResolver _objectResolver;
        public IObjectResolver ObjectResolver => _objectResolver;

        public ObjectResolverContainer(IObjectResolver objectResolver) => _objectResolver = objectResolver;
        public void Update(IObjectResolver objectResolver) => _objectResolver = objectResolver;
    }

    public interface IObjectResolverContainer
    {
        public IObjectResolver ObjectResolver { get; }
        public void Update(IObjectResolver objectResolver);
    }
}