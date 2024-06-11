namespace MultiThreadDbContext;

/// <summary>
/// This class uses a single shared DbContext instance and is not thread-safe.
/// </summary>
public class MyDataAccessWithoutThreadLocal
{
    private readonly MyDbContext _context = new();

    public MyDbContext Context => _context;

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