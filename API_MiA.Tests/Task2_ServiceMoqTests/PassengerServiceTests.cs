using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Moq;
using Xunit;
using ЛР3_МіА._АПІшка.models;

namespace ЛР3_МіА._АПІшка.Tests.Task2_ServiceMoqTests;

public class PassengerServiceTests
{
    [Fact]
    public async Task GetPassengerById_WhenPassengerExists_ReturnsPassengerDto()
    {
        // 1. Arrange (Підготовка моків)
        var mockRepo = new Mock<IPassengerRepository>();
        var mockMapper = new Mock<IMapper>();
        var mockHasher = new Mock<IPasswordHasher>();
        
        var passengerId = "test-id-123";
        
        var expectedPassenger = new Passenger 
        { 
            Id = passengerId, 
            FullName = "Кирило", 
            Email = "kirill@dot.net",
            Role = Roles.Passenger 
        };

        var expectedDto = new PassengerDTO 
        { 
            Id = passengerId, 
            FullName = "Кирило", 
            Email = "kirill@dot.net" 
        };

        mockRepo.Setup(repo => repo.GetByIdAsync(passengerId))
                .ReturnsAsync(expectedPassenger);

        mockMapper.Setup(m => m.Map<PassengerDTO>(expectedPassenger))
                  .Returns(expectedDto);

        var passengerService = new PassengerService(mockRepo.Object, mockMapper.Object, mockHasher.Object);

        // 2. Act
        var result = await passengerService.GetPassengerByIdAsync(passengerId);

        // 3. Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("kirill@dot.net");
        mockRepo.Verify(repo => repo.GetByIdAsync(passengerId), Times.Once);
    }
}
