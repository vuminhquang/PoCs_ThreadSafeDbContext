using MultiThreadDbContext;

namespace TestProject1;

public class MyDataAccessTests
{
    [Fact]
    public void AddEntity_ShouldAddEntityToDatabase()
    {
        // Arrange
        var dataAccess = new MyDataAccess();
        var entity = new MyEntity { Id = Guid.NewGuid(), Name = "Test Entity" };

        // Act
        dataAccess.AddEntity(entity);
        var allEntities = dataAccess.GetAllEntities();

        // Assert
        Assert.Single(allEntities);
        Assert.Equal("Test Entity", allEntities.First().Name);
    }

    [Fact]
    public async Task ConcurrentAccess_TaskWhenAll_ShouldNotShareDbContext_WithThreadLocal()
    {
        // Should run independently, if run after AddEntity_ShouldAddEntityToDatabase, it will fail
        
        // Arrange
        var dataAccess = new MyDataAccess();
        var tasks = new List<Task>();

        // Act
        for (int i = 0; i < 10; i++)
        {
            var index = i; // Capture the loop variable
            tasks.Add(Task.Run(() =>
            {
                var entity = new MyEntity { Id = Guid.NewGuid(), Name = $"Test Entity {index}" };
                dataAccess.AddEntity(entity);
            }));
        }

        await Task.WhenAll(tasks);
        var allEntities = dataAccess.GetAllEntities();

        // Assert
        Assert.Equal(10, allEntities.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Contains(allEntities, e => e.Name == $"Test Entity {i}");
        }
    }
    
    [Fact]
    public async Task ConcurrentAccess_Parallel_ShouldNotShareDbContext_WithThreadLocal()
    {
        // Arrange
        var dataAccess = new MyDataAccess();
        var entities = Enumerable.Range(0, 10).Select(i => new MyEntity { Id = Guid.NewGuid(), Name = $"Test Entity {i}" }).ToList();

        // Act
        await Parallel.ForEachAsync(entities, async (entity, token) =>
        {
            await Task.Run(() => dataAccess.AddEntity(entity));
        });

        var allEntities = dataAccess.GetAllEntities();

        // Assert
        Assert.Equal(10, allEntities.Count);
        for (int i = 0; i < 10; i++)
        {
            Assert.Contains(allEntities, e => e.Name == $"Test Entity {i}");
        }
    }
}
