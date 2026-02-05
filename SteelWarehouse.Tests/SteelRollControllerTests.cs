using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SteelWarehouse.Api.Controllers;
using SteelWarehouse.App.DTOs;
using SteelWarehouse.App.Interfaces;
using SteelWarehouse.Domain;

namespace SteelWarehouse.Tests.Controllers
{
    public class SteelRollsControllerTests
    {
        private readonly Mock<ISteelRollService> _serviceMock;
        private readonly Mock<ILogger<SteelRollsController>> _loggerMock;
        private readonly SteelRollsController _controller;

        public SteelRollsControllerTests()
        {
            _serviceMock = new Mock<ISteelRollService>();
            _loggerMock = new Mock<ILogger<SteelRollsController>>();
            _controller = new SteelRollsController(_serviceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Add_WithValidData_ReturnsCreatedAtAction()
        {
            // Arrange
            var weight = 100.0;
            var length = 50.0;
            var addedRoll = new SteelRoll { Id = 1, Weight = weight, Length = length };
            _serviceMock.Setup(s => s.AddAsync(weight, length)).ReturnsAsync(addedRoll);

            // Act
            var result = await _controller.Add(weight, length);

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(StatusCodes.Status201Created, actionResult.StatusCode);
            Assert.Equal(addedRoll, actionResult.Value);
        }

        [Fact]
        public async Task Add_WithServiceThrowingArgumentException_ReturnsBadRequest()
        {
            // Arrange
            _serviceMock.Setup(s => s.AddAsync(It.IsAny<double>(), It.IsAny<double>())).ThrowsAsync(new ArgumentException("Invalid"));

            // Act
            var result = await _controller.Add(0, 0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
        }

        [Fact]
        public async Task Remove_WithExistingId_ReturnsOk()
        {
            // Arrange
            var rollId = 1;
            var removedRoll = new SteelRoll { Id = rollId, DateRemoved = DateTime.Now };
            _serviceMock.Setup(s => s.RemoveAsync(rollId)).ReturnsAsync(removedRoll);

            // Act
            var result = await _controller.Remove(rollId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(removedRoll, okResult.Value);
        }

        [Fact]
        public async Task Remove_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            var rollId = 99;
            _serviceMock.Setup(s => s.RemoveAsync(rollId)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.Remove(rollId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithRolls()
        {
            // Arrange
            var filter = new SteelRollFilter();
            var rolls = new List<SteelRoll> { new SteelRoll { Id = 1 } };
            _serviceMock.Setup(s => s.GetAllAsync(filter)).ReturnsAsync(rolls);

            // Act
            var result = await _controller.GetAll(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(rolls, okResult.Value);
        }

        [Fact]
        public async Task GetById_WithExistingId_ReturnsOk()
        {
            // Arrange
            var rollId = 1;
            var roll = new SteelRoll { Id = rollId };
            _serviceMock.Setup(s => s.GetByIdAsync(rollId)).ReturnsAsync(roll);

            // Act
            var result = await _controller.GetById(rollId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(roll, okResult.Value);
        }

        [Fact]
        public async Task GetById_WithNonExistingId_ReturnsNotFound()
        {
            // Arrange
            var rollId = 99;
            _serviceMock.Setup(s => s.GetByIdAsync(rollId)).ReturnsAsync((SteelRoll)null);

            // Act
            var result = await _controller.GetById(rollId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetStats_WithInvalidDateRange_ReturnsBadRequest()
        {
            // Arrange
            var from = DateTime.Now;
            var to = DateTime.Now.AddDays(-1);

            // Act
            var result = await _controller.GetStats(from, to);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
