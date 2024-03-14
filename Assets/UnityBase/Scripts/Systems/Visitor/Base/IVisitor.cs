namespace UnityBase.Visitor
{
    public interface IVisitor
    {
        void Visit<T>(T visitable) where T : IVisitable;
    }
}