namespace MultiThreadDbContext;

/// <summary>
/// This class uses ThreadLocal<DbContext> to ensure thread safety.
/// </summary>
public class MyDataAccess
{
    private readonly ThreadLocal<MyDbContext> _threadLocalContext = new(() => new MyDbContext());

    public MyDbContext Context => _threadLocalContext.Value;

    public void AddEntity(MyEntity entity)
    {
        Context.MyEntities.Add(entity);
        Context.SaveChanges();
    }

    public List<MyEntity> GetAllEntities()
    {
        return Context.MyEntities.ToList();
    }
}