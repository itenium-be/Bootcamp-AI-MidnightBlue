using Itenium.SkillForge.Entities;
using Itenium.SkillForge.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Itenium.SkillForge.WebApi.Tests;

/// <summary>
/// Regression tests capturing current CourseController and CourseEntity behaviour.
/// </summary>
[TestFixture]
public class CourseRegressionTests : DatabaseTestBase
{
    private CourseController _sut = null!;

    [SetUp]
    public void Setup()
    {
        _sut = new CourseController(Db);
    }

    // --- CourseEntity ---

    [Test]
    public void CourseEntity_ToString_ReturnsNameAndCategory()
    {
        var course = new CourseEntity { Name = "C# Basics", Category = "Programming" };
        Assert.That(course.ToString(), Is.EqualTo("C# Basics (Programming)"));
    }

    [Test]
    public void CourseEntity_ToString_WithNullCategory_ShowsEmptyParens()
    {
        var course = new CourseEntity { Name = "C# Basics" };
        Assert.That(course.ToString(), Is.EqualTo("C# Basics ()"));
    }

    // --- CreateCourse ---

    [Test]
    public async Task CreateCourse_SetsCreatedAtToCurrentUtcTime()
    {
        var before = DateTime.UtcNow;
        var request = new CreateCourseRequest("Test Course", null, null, null);

        var result = await _sut.CreateCourse(request);

        var after = DateTime.UtcNow;
        var createdResult = result.Result as CreatedAtActionResult;
        var course = createdResult!.Value as CourseEntity;
        Assert.That(course!.CreatedAt, Is.GreaterThanOrEqualTo(before));
        Assert.That(course.CreatedAt, Is.LessThanOrEqualTo(after));
    }

    [Test]
    public async Task CreateCourse_ReturnsCreatedAtActionWithCorrectRouteName()
    {
        var request = new CreateCourseRequest("Route Test", null, null, null);

        var result = await _sut.CreateCourse(request);

        var createdResult = result.Result as CreatedAtActionResult;
        Assert.That(createdResult!.ActionName, Is.EqualTo(nameof(_sut.GetCourse)));
        Assert.That(createdResult.RouteValues!["id"], Is.EqualTo((createdResult.Value as CourseEntity)!.Id));
    }

    // --- UpdateCourse ---

    [Test]
    public async Task UpdateCourse_PreservesCreatedAt()
    {
        var originalCreatedAt = new DateTime(2025, 1, 15, 10, 0, 0, DateTimeKind.Utc);
        var course = new CourseEntity { Name = "Original", CreatedAt = originalCreatedAt };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var request = new UpdateCourseRequest("Updated Name", "New Desc", null, null);
        await _sut.UpdateCourse(course.Id, request);

        var updated = await Db.Courses.FindAsync(course.Id);
        Assert.That(updated!.CreatedAt, Is.EqualTo(originalCreatedAt));
    }

    [Test]
    public async Task UpdateCourse_CanClearNullableFields()
    {
        var course = new CourseEntity { Name = "Test", Description = "Old Desc", Category = "Dev", Level = "Beginner" };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var request = new UpdateCourseRequest("Test", null, null, null);
        var result = await _sut.UpdateCourse(course.Id, request);

        var okResult = result.Result as OkObjectResult;
        var updated = okResult!.Value as CourseEntity;
        Assert.That(updated!.Description, Is.Null);
        Assert.That(updated.Category, Is.Null);
        Assert.That(updated.Level, Is.Null);
    }

    [Test]
    public async Task UpdateCourse_PersistsChangesToDatabase()
    {
        var course = new CourseEntity { Name = "Before", Description = "Old" };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        var request = new UpdateCourseRequest("After", "New", "DevOps", "Advanced");
        await _sut.UpdateCourse(course.Id, request);

        var reloaded = await Db.Courses.FindAsync(course.Id);
        Assert.That(reloaded!.Name, Is.EqualTo("After"));
        Assert.That(reloaded.Description, Is.EqualTo("New"));
        Assert.That(reloaded.Category, Is.EqualTo("DevOps"));
        Assert.That(reloaded.Level, Is.EqualTo("Advanced"));
    }

    // --- DeleteCourse ---

    [Test]
    public async Task DeleteCourse_RemovedCourseIsNoLongerReturnedByGetCourses()
    {
        var course = new CourseEntity { Name = "Ephemeral" };
        Db.Courses.Add(course);
        await Db.SaveChangesAsync();

        await _sut.DeleteCourse(course.Id);

        var listResult = await _sut.GetCourses();
        var okResult = listResult.Result as OkObjectResult;
        var courses = okResult!.Value as List<CourseEntity>;
        Assert.That(courses!.Select(c => c.Id), Does.Not.Contain(course.Id));
    }
}
