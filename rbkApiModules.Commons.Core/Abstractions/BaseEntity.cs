public abstract class BaseEntity
{
    protected BaseEntity()
    {

    }

    public virtual Guid Id { get; protected set; }
}