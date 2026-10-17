
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using RaceDay.API.Models;
using Xunit;

public class ModelMappingTests
{
    private static TableAttribute? GetTableAttribute(Type modelType)
    {
        return modelType.GetCustomAttribute<TableAttribute>();
    }

    private static ColumnAttribute? GetColumnAttribute(
        Type modelType,
        string propertyName)
    {
        var property = modelType.GetProperty(propertyName);

        Assert.NotNull(property);

        return property!.GetCustomAttribute<ColumnAttribute>();
    }

    [Fact]
    public void Event_MapsToEventsTable()
    {
        var attribute = GetTableAttribute(typeof(Event));

        Assert.NotNull(attribute);
        Assert.Equal("Events", attribute!.Name);
    }

    [Fact]
    public void EventId_MapsToEventIdColumn()
    {
        var attribute = GetColumnAttribute(
            typeof(Event), nameof(Event.EventId));

        Assert.NotNull(attribute);
        Assert.Equal("Event_ID", attribute!.Name);
    }

    [Fact]
    public void Category_MapsToCategoriesTable()
    {
        var attribute = GetTableAttribute(typeof(Category));

        Assert.NotNull(attribute);
        Assert.Equal("Categories", attribute!.Name);
    }

    [Fact]
    public void CategoryDistance_UsesExpectedColumnAndPrecision()
    {
        var attribute = GetColumnAttribute(
            typeof(Category), nameof(Category.DistanceKm));

        Assert.NotNull(attribute);
        Assert.Equal("Distance_Km", attribute!.Name);
        Assert.Equal("decimal(6,2)", attribute.TypeName);
    }

    [Fact]
    public void Enrolment_MapsToEnrolmentsTable()
    {
        var attribute = GetTableAttribute(typeof(Enrolment));

        Assert.NotNull(attribute);
        Assert.Equal("Enrolments", attribute!.Name);
    }

    [Fact]
    public void EnrolmentBibNumber_MapsToBibNumberColumn()
    {
        var attribute = GetColumnAttribute(
            typeof(Enrolment), nameof(Enrolment.BibNumber));

        Assert.NotNull(attribute);
        Assert.Equal("Bib_Number", attribute!.Name);
    }

    [Fact]
    public void Result_MapsToResultsTable()
    {
        var attribute = GetTableAttribute(typeof(Result));

        Assert.NotNull(attribute);
        Assert.Equal("Results", attribute!.Name);
    }

    [Fact]
    public void ResultFinishTime_UsesSqlTimeType()
    {
        var attribute = GetColumnAttribute(
            typeof(Result), nameof(Result.FinishTime));

        Assert.NotNull(attribute);
        Assert.Equal("Finish_Time", attribute!.Name);
        Assert.Equal("time", attribute.TypeName);
    }
}
