namespace UnityBase.Visitor
{
    public interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}