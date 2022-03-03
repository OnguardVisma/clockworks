using System;
using FluentAssertions;
using Xunit;

namespace Onguard.TimeTracker.Model.Tests
{
    public class RevisionTests
    {
        [Fact]
        public void CanBeCreated()
        {
            // arrange
            const Revision.StateEnum state = Revision.StateEnum.ToDo;
            var dateChanged = DateTime.Now;
            const string changedBy = "culprit";
            
            // act
            var revision = new Revision(state, dateChanged, changedBy);
            
            // assert
            revision.State.Should().Be(state);
            revision.DateChanged.Should().Be(dateChanged);
            revision.ChangedBy.Should().Be(changedBy);
        }
    }
}
