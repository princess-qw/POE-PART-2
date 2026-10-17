
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RaceDay.API.Models;
using Xunit;

public class ModelRuleTests
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

    // Event model tests

    [Fact]
    public void Event_EmptyName_IsInvalid()
    {
        var model = new Event { Event_Name = string.Empty };

        Assert.False(IsValidProperty(model, nameof(Event.Event_Name)));
    }

    [Fact]
    public void Event_ValidName_IsValid()
    {
        var model = new Event { Event_Name = "Community Run" };

        Assert.True(IsValidProperty(model, nameof(Event.Event_Name)));
    }

    [Fact]
    public void Event_EmptyLocation_IsInvalid()
    {
        var model = new Event { Event_Location = string.Empty };

        Assert.False(IsValidProperty(model, nameof(Event.Event_Location)));
    }

    // Category model tests

    [Fact]
    public void Category_ZeroDistance_IsInvalid()
    {
        var model = new Category { DistanceKm = 0 };

        Assert.False(IsValidProperty(model, nameof(Category.DistanceKm)));
    }

    [Fact]
    public void Category_NegativeEntryFee_IsInvalid()
    {
        var model = new Category { EntryFee = -1 };

        Assert.False(IsValidProperty(model, nameof(Category.EntryFee)));
    }

    [Fact]
    public void Category_ZeroMaximumParticipation_IsInvalid()
    {
        var model = new Category { MaximumParticipation = 0 };

        Assert.False(IsValidProperty(
            model, nameof(Category.MaximumParticipation)));
    }

    // Enrolment model tests

    [Fact]
    public void Enrolment_EmptyStatus_IsInvalid()
    {
        var model = new Enrolment { Status = string.Empty };

        Assert.False(IsValidProperty(model, nameof(Enrolment.Status)));
    }

    [Fact]
    public void Enrolment_PendingStatus_IsValid()
    {
        var model = new Enrolment { Status = "Pending" };

        Assert.True(IsValidProperty(model, nameof(Enrolment.Status)));
    }

    [Fact]
    public void Enrolment_InvalidBibNumber_IsInvalid()
    {
        var model = new Enrolment { BibNumber = "BIB 001!" };

        Assert.False(IsValidProperty(model, nameof(Enrolment.BibNumber)));
    }

    // Result model tests

    [Fact]
    public void Result_ZeroPosition_IsInvalid()
    {
        var model = new Result { Position = 0 };

        Assert.False(IsValidProperty(model, nameof(Result.Position)));
    }

    [Fact]
    public void Result_ValidPosition_IsValid()
    {
        var model = new Result { Position = 1 };

        Assert.True(IsValidProperty(model, nameof(Result.Position)));
    }

    [Fact]
    public void Result_InvalidStatus_IsInvalid()
    {
        var model = new Result { ResultStatus = "Unknown" };

        Assert.False(IsValidProperty(model, nameof(Result.ResultStatus)));
    }
}
