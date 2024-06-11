using MultiThreadDbContext;

namespace TestProject1;

public class MyDataAccessWithoutThreadLocalTests
{
    [Fact]
    public async Task ConcurrentAccess_ShouldFailWithoutThreadLocal()
    {
        // Arrange
        var dataAccess = new MyDataAccessWithoutThreadLocal();
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

        Exception caughtException = null;

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<InvalidOperationException>(caughtException);
        //Exception: An attempt was made to use the context instance while it is being configured. A DbContext instance cannot be used inside 'OnConfiguring' since it is still being configured at this point. This can happen if a second operation is started on this context instance before a previous operation completed. Any instance members are not guaranteed to be thread safe.
        Assert.Contains("An attempt was made to use the context in", caughtException.Message);
    }
    
    [Fact]
    public async Task ConcurrentAccess2_ShouldFailWithoutThreadLocal()
    {
        // Arrange
        var dataAccess = new MyDataAccessWithoutThreadLocal();
        var entities = Enumerable.Range(0, 10).Select(i => new MyEntity { Id = Guid.NewGuid(), Name = $"Test Entity {i}" }).ToList();

        // Act
        Exception caughtException = null;
        try
        {
            await Parallel.ForEachAsync(entities, async (entity, token) =>
            {
                await Task.Run(() => dataAccess.AddEntity(entity), token);
            });
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<InvalidOperationException>(caughtException);
        Assert.Contains("a second operation is started on this context instance before a previous operation completed.", caughtException.Message);
    }
}