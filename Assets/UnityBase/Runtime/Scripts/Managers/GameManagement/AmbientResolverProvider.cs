using VContainer;

public class AmbientResolverProvider : IAmbientResolverProvider
{
    public IObjectResolver CurrentObjectResolver { get; private set; }

    public AmbientResolverProvider(IObjectResolver objectResolver)
    {
        CurrentObjectResolver = objectResolver;
    }
    
    public void UpdateResolver(IObjectResolver resolver)
    {
        CurrentObjectResolver = resolver;
    }
}

public interface IAmbientResolverProvider
{
    public IObjectResolver CurrentObjectResolver { get; }
}