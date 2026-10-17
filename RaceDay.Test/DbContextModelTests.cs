
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using RaceDay.API.Data;
using RaceDay.API.Models;
using Xunit;

public class DbContextModelTests
{
    private static RaceDayDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }

    private static IForeignKey GetForeignKey<TDependent, TPrincipal>(
        RaceDayDbContext context,
        string propertyName)
        where TDependent : class
        where TPrincipal : class
    {
        var entityType = context.Model.FindEntityType(
            typeof(TDependent));

        Assert.NotNull(entityType);

        var foreignKey = entityType!.GetForeignKeys()
            .SingleOrDefault(fk =>
                fk.PrincipalEntityType.ClrType == typeof(TPrincipal) &&
                fk.Properties.Any(p => p.Name == propertyName));

        Assert.NotNull(foreignKey);

        return foreignKey!;
    }

    private static void AssertUniqueIndex<TEntity>(
        RaceDayDbContext context,
        params string[] propertyNames)
        where TEntity : class
    {
        var entityType = context.Model.FindEntityType(typeof(TEntity));

        Assert.NotNull(entityType);

        var indexExists = entityType!.GetIndexes().Any(index =>
            index.IsUnique &&
            index.Properties
                .Select(property => property.Name)
                .SequenceEqual(propertyNames));

        Assert.True(
            indexExists,
            $"Expected a unique index on {typeof(TEntity).Name}: " +
            string.Join(", ", propertyNames));
    }

    [Fact]
    public void User_Email_HasUniqueIndex()
    {
        using var context = CreateContext();

        AssertUniqueIndex<User>(context, nameof(User.Email));
    }

    [Fact]
    public void Organiser_UserId_HasUniqueIndex()
    {
        using var context = CreateContext();

        AssertUniqueIndex<Organiser>(context, nameof(Organiser.UserId));
    }

    [Fact]
    public void Participant_UserId_HasUniqueIndex()
    {
        using var context = CreateContext();

        AssertUniqueIndex<Participant>(
            context, nameof(Participant.UserId));
    }

    [Fact]
    public void Event_OrganiserRelationship_HasNoActionDelete()
    {
        using var context = CreateContext();

        var foreignKey = GetForeignKey<Event, Organiser>(
            context, nameof(Event.OrganiserId));

        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Category_EventRelationship_HasNoActionDelete()
    {
        using var context = CreateContext();

        var foreignKey = GetForeignKey<Category, Event>(
            context, nameof(Category.EventId));

        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Enrolment_ParticipantRelationship_HasNoActionDelete()
    {
        using var context = CreateContext();

        var foreignKey = GetForeignKey<Enrolment, Participant>(
            context, nameof(Enrolment.ParticipantId));

        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Enrolment_CategoryRelationship_HasNoActionDelete()
    {
        using var context = CreateContext();

        var foreignKey = GetForeignKey<Enrolment, Category>(
            context, nameof(Enrolment.CategoryId));

        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Payment_EnrolmentRelationship_IsOneToOne()
    {
        using var context = CreateContext();

        var foreignKey = GetForeignKey<Payment, Enrolment>(
            context, nameof(Payment.EnrolmentId));

        Assert.True(foreignKey.IsUnique);
        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void Enrolment_BibNumber_HasUniqueIndex()
    {
        using var context = CreateContext();

        AssertUniqueIndex<Enrolment>(
            context, nameof(Enrolment.BibNumber));
    }

    [Fact]
    public void Enrolment_ParticipantAndCategory_HaveUniqueCompositeIndex()
    {
        using var context = CreateContext();

        AssertUniqueIndex<Enrolment>(
            context,
            nameof(Enrolment.ParticipantId),
            nameof(Enrolment.CategoryId));
    }

    [Fact]
    public void EventSponsor_EventAndSponsor_HaveUniqueCompositeIndex()
    {
        using var context = CreateContext();

        AssertUniqueIndex<EventSponsor>(
            context,
            nameof(EventSponsor.EventId),
            nameof(EventSponsor.SponsorId));
    }

    [Fact]
    public void Result_EnrolmentRelationship_HasNoActionDelete()
    {
        using var context = CreateContext();

        var foreignKey = GetForeignKey<Result, Enrolment>(
            context, nameof(Result.EnrolmentId));

        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }

    [Fact]
    public void WeatherLog_EventRelationship_HasNoActionDelete()
    {
        using var context = CreateContext();

        var foreignKey = GetForeignKey<WeatherLog, Event>(
            context, nameof(WeatherLog.EventId));

        Assert.Equal(DeleteBehavior.NoAction, foreignKey.DeleteBehavior);
    }
}
