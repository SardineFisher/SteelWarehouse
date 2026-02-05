using Moq;
using SteelWarehouse.App.DTOs;
using SteelWarehouse.App.Interfaces;
using SteelWarehouse.App.Services;
using SteelWarehouse.Domain;
using SteelWarehouse.Infrastructure.Repositories;

namespace SteelWarehouse.Tests.Services
{ 
    public class SteelRollServiceTests
    {
        private readonly Mock<ISteelRollRepository> _repositoryMock;
        private readonly SteelRollService _service;

        public SteelRollServiceTests()
        {
            _repositoryMock = new Mock<ISteelRollRepository>();
            _service = new SteelRollService(_repositoryMock.Object);
        }

        [Fact]
        public async Task AddAsync_WithValidData_ShouldReturnAddedRoll()
        {
            // Arrange
            var weight = 100.0;
            var length = 50.0;
            var expectedRoll = new SteelRoll { Id = 1, Weight = weight, Length = length, DateAdded = DateTime.Now };
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<SteelRoll>())).ReturnsAsync(expectedRoll);

            // Act
            var result = await _service.AddAsync(weight, length);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedRoll.Id, result.Id);
            Assert.Equal(weight, result.Weight);
            Assert.Equal(length, result.Length);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<SteelRoll>(roll => roll.Weight == weight && roll.Length == length)), Times.Once);
        }

        [Theory]
        [InlineData(0, 100)]
        [InlineData(-10, 100)]
        [InlineData(100, 0)]
        [InlineData(100, -10)]
        public async Task AddAsync_WithInvalidData_ShouldThrowArgumentException(double weight, double length)
        {
            // Act
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.AddAsync(weight, length));

            // Assert
            Assert.Equal("Вес и длина должны быть положительны (>0)", ex.Message);
        }

        [Fact]
        public async Task RemoveAsync_WithExistingId_ShouldReturnRemovedRoll()
        {
            // Arrange
            var rollId = 1;
            var removedRoll = new SteelRoll { Id = rollId, DateRemoved = DateTime.Now };
            _repositoryMock.Setup(r => r.RemoveAsync(rollId)).ReturnsAsync(removedRoll);

            // Act
            var result = await _service.RemoveAsync(rollId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(rollId, result.Id);
            Assert.NotNull(result.DateRemoved);
            _repositoryMock.Verify(r => r.RemoveAsync(rollId), Times.Once);
        }

        [Fact]
        public async Task RemoveAsync_WithNonExistingId_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var rollId = 99;
            _repositoryMock.Setup(r => r.RemoveAsync(rollId)).ThrowsAsync(new KeyNotFoundException());

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.RemoveAsync(rollId));
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRolls()
        {
            // Arrange
            var filter = new SteelRollFilter();
            var rolls = new List<SteelRoll>
            {
                new SteelRoll { Id = 1 },
                new SteelRoll { Id = 2 }
            };
            _repositoryMock.Setup(r => r.GetAllAsync(filter)).ReturnsAsync(rolls);

            // Act
            var result = await _service.GetAllAsync(filter);

            // Assert
            Assert.Equal(2, result.Count());
            _repositoryMock.Verify(r => r.GetAllAsync(filter), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldFilterByWeightAndLength()
        {
            // Arrange
            var repo = new InMemorySteelRollRepository();

            await repo.AddAsync(new SteelRoll { Id = 1, Weight = 300, Length = 10 });
            await repo.AddAsync(new SteelRoll { Id = 2, Weight = 600, Length = 50 });
            await repo.AddAsync(new SteelRoll { Id = 3, Weight = 700, Length = 200 });

            var filter = new SteelRollFilter
            {
                MinWeight = 500,
                MaxLength = 100
            };

            // Act
            var result = await repo.GetAllAsync(filter);

            // Assert
            Assert.Single(result);
            Assert.Equal(2, result.First().Id);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ShouldReturnRoll()
        {
            // Arrange
            var rollId = 1;
            var expectedRoll = new SteelRoll { Id = rollId };
            _repositoryMock.Setup(r => r.GetByIdAsync(rollId)).ReturnsAsync(expectedRoll);

            // Act
            var result = await _service.GetByIdAsync(rollId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(rollId, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
        {
            // Arrange
            var rollId = 99;
            _repositoryMock.Setup(r => r.GetByIdAsync(rollId)).ReturnsAsync((SteelRoll)null);

            // Act
            var result = await _service.GetByIdAsync(rollId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetStatsAsync_WithNoRollsInPeriod_ShouldReturnEmptyStats()
        {
            // Arrange
            var from = new DateTime(2023, 1, 1);
            var to = new DateTime(2023, 1, 31);
            _repositoryMock.Setup(r => r.GetRollsInPeriodAsync(from, to)).ReturnsAsync(new List<SteelRoll>());

            // Act
            var result = await _service.GetStatsAsync(from, to);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.AddedCount);
            Assert.Equal(0, result.RemovedCount);
            Assert.Equal(0, result.AvgWeight);
            Assert.Null(result.MinStorageDuration);
            Assert.Null(result.DayWithMaxRolls);
        }

        [Fact]
        public async Task GetStatsAsync_WithRollsInPeriod_ShouldCalculateStatsCorrectly()
        {
            // Arrange
            var from = new DateTime(2025, 10, 1);
            var to = new DateTime(2025, 10, 5);

            var rolls = new List<SteelRoll>
            {
                new SteelRoll { Id = 1, Weight = 100, Length = 10, DateAdded = new DateTime(2025, 10, 1), DateRemoved = new DateTime(2025, 10, 3) },
                new SteelRoll { Id = 2, Weight = 200, Length = 20, DateAdded = new DateTime(2025, 10, 2), DateRemoved = null },
                new SteelRoll { Id = 3, Weight = 150, Length = 15, DateAdded = new DateTime(2025, 10, 2), DateRemoved = new DateTime(2025, 10, 4) },
                new SteelRoll { Id = 4, Weight = 300, Length = 30, DateAdded = new DateTime(2025, 9, 28), DateRemoved = new DateTime(2025, 10, 2) },
                new SteelRoll { Id = 5, Weight = 250, Length = 25, DateAdded = new DateTime(2025, 10, 3), DateRemoved = new DateTime(2025, 10, 10) }
            };

            _repositoryMock.Setup(r => r.GetRollsInPeriodAsync(from, to)).ReturnsAsync(rolls);

            // Act
            var result = await _service.GetStatsAsync(from, to);

            // Assert
            Assert.Equal(4, result.AddedCount);
            Assert.Equal(3, result.RemovedCount);
            Assert.Equal(200, result.AvgWeight);
            Assert.Equal(100, result.MinWeight);
            Assert.Equal(300, result.MaxWeight);
            Assert.Equal(1000, result.TotalWeightCurrent);
            Assert.Equal(TimeSpan.FromDays(2), result.MinStorageDuration);
            Assert.Equal(TimeSpan.FromDays(4), result.MaxStorageDuration);
            Assert.Equal(new DateTime(2025, 10, 2), result.DayWithMaxRolls);
            Assert.Equal(new DateTime(2025, 10, 1), result.DayWithMinRolls);
            Assert.Equal(new DateTime(2025, 10, 2), result.DayWithMaxWeight);
            Assert.Equal(new DateTime(2025, 10, 1), result.DayWithMinWeight);
        }
    }
}
