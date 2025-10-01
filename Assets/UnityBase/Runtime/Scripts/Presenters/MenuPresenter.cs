using VContainer;
using VContainer.Unity;

namespace UnityBase.Presenters
{
    public class MenuPresenter : IInitializable
    {
        public MenuPresenter(IObjectResolver objectResolver, AmbientResolverProvider ambientResolverProvider)
        {
            ambientResolverProvider.UpdateResolver(objectResolver);
        }

        public void Initialize() {}
    }
}