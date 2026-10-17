using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit;

namespace RaceDay.Tests
{
    public class SponsorTests
    {
        [Fact]
        public void Sponsor_HasCorrectTableMapping()
        {
            // Arrange
            var tableAttribute = typeof(Sponsor)
                .GetCustomAttributes(typeof(TableAttribute), false)
                .FirstOrDefault() as TableAttribute;

            // Assert
            Assert.NotNull(tableAttribute);
            Assert.Equal("Sponsors", tableAttribute.Name);
        }

        [Fact]
        public void Sponsor_RequiredFieldsAreValidated()
        {
            // Arrange
            var sponsor = new Sponsor
            {
                SponsorName = "",
                SponsorType = ""
            };

            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(sponsor);

            // Act
            bool isValid = Validator.TryValidateObject(
                sponsor,
                context,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, result =>
                result.MemberNames.Contains(nameof(Sponsor.SponsorName)));
            Assert.Contains(validationResults, result =>
                result.MemberNames.Contains(nameof(Sponsor.SponsorType)));
        }

        [Fact]
        public void Sponsor_ValidDataPassesValidation()
        {
            // Arrange
            var sponsor = new Sponsor
            {
                SponsorName = "RaceDay Test Sponsor",
                SponsorType = "Financial",
                ContactPerson = "Test Contact",
                Email = "sponsor@example.com",
                Phone = "0123456789",
                CreatedAt = DateTime.UtcNow
            };

            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(sponsor);

            // Act
            bool isValid = Validator.TryValidateObject(
                sponsor,
                context,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Sponsor_SponsorIdIsConfiguredAsIdentity()
        {
            // Arrange
            var property = typeof(Sponsor).GetProperty(nameof(Sponsor.SponsorId));

            var column = property?
                .GetCustomAttributes(typeof(ColumnAttribute), false)
                .FirstOrDefault() as ColumnAttribute;

            var key = property?
                .GetCustomAttributes(typeof(KeyAttribute), false)
                .FirstOrDefault() as KeyAttribute;

            var identity = property?
                .GetCustomAttributes(
                    typeof(DatabaseGeneratedAttribute), false)
                .FirstOrDefault() as DatabaseGeneratedAttribute;

            // Assert
            Assert.NotNull(property);
            Assert.NotNull(key);
            Assert.NotNull(identity);
            Assert.Equal("Sponsor_ID", column?.Name);
            Assert.Equal(
                DatabaseGeneratedOption.Identity,
                identity.DatabaseGeneratedOption);
        }
    }
}