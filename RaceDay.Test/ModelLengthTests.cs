
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDay.API.Models;
using Xunit;

public class ModelLengthTests
{
    private static bool IsValidProperty(
        object model,
        string propertyName)
    {
        var property = model.GetType().GetProperty(propertyName);

        Assert.NotNull(property);

        var context = new ValidationContext(model)
        {
            MemberName = propertyName
        };

        var results = new List<ValidationResult>();

        return Validator.TryValidateProperty(
            property!.GetValue(model),
            context,
            results);
    }

    [Fact]
    public void Event_NameOver100Characters_IsInvalid()
    {
        var model = new Event
        {
            Event_Name = new string('A', 101)
        };

        Assert.False(
            IsValidProperty(model, nameof(Event.Event_Name)));
    }

    [Fact]
    public void Event_TypeOver50Characters_IsInvalid()
    {
        var model = new Event
        {
            Event_Type = new string('A', 51)
        };

        Assert.False(
            IsValidProperty(model, nameof(Event.Event_Type)));
    }

    [Fact]
    public void Event_DescriptionOver255Characters_IsInvalid()
    {
        var model = new Event
        {
            Description = new string('A', 256)
        };

        Assert.False(
            IsValidProperty(model, nameof(Event.Description)));
    }

    [Fact]
    public void Category_NameOver100Characters_IsInvalid()
    {
        var model = new Category
        {
            CategoryName = new string('A', 101)
        };

        Assert.False(
            IsValidProperty(model, nameof(Category.CategoryName)));
    }

    [Fact]
    public void Enrolment_BibNumberOver20Characters_IsInvalid()
    {
        var model = new Enrolment
        {
            BibNumber = new string('A', 21)
        };

        Assert.False(
            IsValidProperty(model, nameof(Enrolment.BibNumber)));
    }

    [Fact]
    public void Result_StatusOver20Characters_IsInvalid()
    {
        var model = new Result
        {
            ResultStatus = new string('A', 21)
        };

        Assert.False(
            IsValidProperty(model, nameof(Result.ResultStatus)));
    }

    [Fact]
    public void Event_NameAtMaximumLength_IsValid()
    {
        var model = new Event
        {
            Event_Name = new string('A', 100)
        };

        Assert.True(
            IsValidProperty(model, nameof(Event.Event_Name)));
    }

    [Fact]
    public void Enrolment_BibNumberAtMaximumLength_IsValid()
    {
        var model = new Enrolment
        {
            BibNumber = new string('A', 20)
        };

        Assert.True(
            IsValidProperty(model, nameof(Enrolment.BibNumber)));
    }
}
