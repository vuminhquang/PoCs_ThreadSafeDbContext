using MultiThreadDbContext;
using System.Diagnostics;
using Xunit.Abstractions;

namespace TestProject1;

public class PerformanceTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PerformanceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private const int NumberOfEntities = 1000;

    [Fact]
    public void CompareSingleThreadAndMultiThreadInsert()
    {
        // Arrange
        var singleThreadDataAccess = new MyDataAccessWithoutThreadLocal();
        var multiThreadDataAccess = new MyDataAccess();

        // Act
        var entitiesSet1 = GenerateEntities(NumberOfEntities);
        var singleThreadTime = MeasureInsertTime(singleThreadDataAccess, entitiesSet1, useParallel: false);
        var entitiesSet2 = GenerateEntities(NumberOfEntities);
        var multiThreadTime = MeasureInsertTime(multiThreadDataAccess, entitiesSet2, useParallel: true);

        // Assert
        _testOutputHelper.WriteLine($"Single-threaded insert time: {singleThreadTime} ms");
        _testOutputHelper.WriteLine($"Multi-threaded insert time: {multiThreadTime} ms");

        Assert.True(singleThreadTime > multiThreadTime, "Expected multi-threaded insert to be faster than single-threaded insert.");
    }

    private List<MyEntity> GenerateEntities(int count)
    {
        return Enumerable.Range(0, count).Select(i => new MyEntity { Id = Guid.NewGuid(), Name = $"Test Entity {i}" }).ToList();
    }

    private long MeasureInsertTime(dynamic dataAccess, List<MyEntity> entities, bool useParallel)
    {
        var stopwatch = Stopwatch.StartNew();

        if (useParallel)
        {
            Parallel.ForEach(entities, entity =>
            {
                dataAccess.AddEntity(entity);
            });
        }
        else
        {
            foreach (var entity in entities)
            {
                dataAccess.AddEntity(entity);
            }
        }

        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}