
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RaceDay.API.Models;
using Xunit;

public class ModelKeyTests
{
    private static void AssertPrimaryKey(
        Type modelType,
        string propertyName)
    {
        var property = modelType.GetProperty(propertyName);

        Assert.NotNull(property);
        Assert.NotNull(
            Attribute.GetCustomAttribute(
                property!, typeof(KeyAttribute)));
    }

    private static void AssertIdentityGenerated(
        Type modelType,
        string propertyName)
    {
        var property = modelType.GetProperty(propertyName);

        Assert.NotNull(property);

        var attribute =
            Attribute.GetCustomAttribute(
                property!, typeof(DatabaseGeneratedAttribute))
            as DatabaseGeneratedAttribute;

        Assert.NotNull(attribute);

        Assert.Equal(
            DatabaseGeneratedOption.Identity,
            attribute!.DatabaseGeneratedOption);
    }

    [Fact]
    public void Event_EventId_IsPrimaryKeyAndIdentityGenerated()
    {
        AssertPrimaryKey(typeof(Event), nameof(Event.EventId));
        AssertIdentityGenerated(typeof(Event), nameof(Event.EventId));
    }

    [Fact]
    public void Category_CategoryId_IsPrimaryKeyAndIdentityGenerated()
    {
        AssertPrimaryKey(typeof(Category), nameof(Category.CategoryId));
        AssertIdentityGenerated(typeof(Category), nameof(Category.CategoryId));
    }

    [Fact]
    public void Enrolment_EnrolmentId_IsPrimaryKey()
    {
        AssertPrimaryKey(
            typeof(Enrolment), nameof(Enrolment.EnrolmentId));
    }

    [Fact]
    public void Result_ResultId_IsPrimaryKeyAndIdentityGenerated()
    {
        AssertPrimaryKey(typeof(Result), nameof(Result.ResultId));
        AssertIdentityGenerated(typeof(Result), nameof(Result.ResultId));
    }

    [Fact]
    public void Result_FinishTime_IsNullable()
    {
        var property = typeof(Result).GetProperty(
            nameof(Result.FinishTime));

        Assert.NotNull(property);
        Assert.Equal(
            typeof(TimeSpan),
            Nullable.GetUnderlyingType(property!.PropertyType));
    }

    [Fact]
    public void Result_Position_IsNullable()
    {
        var property = typeof(Result).GetProperty(
            nameof(Result.Position));

        Assert.NotNull(property);
        Assert.Equal(
            typeof(int),
            Nullable.GetUnderlyingType(property!.PropertyType));
    }

    [Fact]
    public void Category_ElevationGain_IsNullable()
    {
        var property = typeof(Category).GetProperty(
            nameof(Category.ElevationGainM));

        Assert.NotNull(property);
        Assert.Equal(
            typeof(int),
            Nullable.GetUnderlyingType(property!.PropertyType));
    }
}
