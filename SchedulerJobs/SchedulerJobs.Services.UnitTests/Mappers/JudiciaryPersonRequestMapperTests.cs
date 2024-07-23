using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.Mappers;

namespace SchedulerJobs.Services.UnitTests.Mappers
{
    public class JudiciaryPersonRequestMapperTests
    {
        [Test]
        public void should_map_model_to_request()
        {
            // Arrange
            var model = new JudiciaryPersonModel
            {
                Id = "123",
                Email = "email@email.com",
                WorkPhone = "WorkPhone",
                Fullname = "FullName",
                Surname = "Surname",
                Title = "Title",
                KnownAs = "KnownAs",
                PersonalCode = "PersonalCode",
                PostNominals = "PostNominals",
                HasLeft = true,
                Leaver = true,
                LeftOn = "2023-01-01",
                Deleted = true,
                DeletedOn = "2023-01-01"
            };
            
            // Act
            var result = JudiciaryPersonRequestMapper.MapTo(model);

            // Assert
            result.Id.Should().Be(model.Id);
            result.Email.Should().Be(model.Email);
            result.WorkPhone.Should().Be(model.WorkPhone);
            result.Fullname.Should().Be(model.Fullname);
            result.Surname.Should().Be(model.Surname);
            result.Title.Should().Be(model.Title);
            result.KnownAs.Should().Be(model.KnownAs);
            result.PersonalCode.Should().Be(model.PersonalCode);
            result.PostNominals.Should().Be(model.PostNominals);
            result.HasLeft.Should().Be(model.HasLeft);
            result.Leaver.Should().Be(model.Leaver);
            result.LeftOn.Should().Be(model.LeftOn);
            result.Deleted.Should().Be(model.Deleted);
            result.DeletedOn.Should().Be(model.DeletedOn);
        }
    }
}
