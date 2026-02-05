using Microsoft.AspNetCore.Mvc;
using SteelWarehouse.App.DTOs;
using SteelWarehouse.App.Interfaces;
using SteelWarehouse.App.Services;
using SteelWarehouse.Domain;

namespace SteelWarehouse.Api.Controllers
{
    [ApiController]
    [Route("api/rolls")]
    public class SteelRollsController : Controller
    {
        private readonly ISteelRollService _service;
        private readonly ILogger<SteelRollsController> _logger;

        public SteelRollsController(ISteelRollService service, ILogger<SteelRollsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Добавление нового рулона на склад
        /// </summary>
        /// <param name="weight">Вес рулона</param>
        /// <param name="length">Длина рулона</param>
        /// <returns>Добавленный рулон</returns>
        /// <response code="201">Рулон успешно создан</response>
        /// <response code="400">Некорректные входные данные</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpPost]
        [ProducesResponseType(typeof(SteelRoll), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SteelRoll>> Add([FromQuery] double weight, [FromQuery] double length)
        {
            try
            {
                _logger.LogInformation($"Запрос на добавление нового рулона: Weight={weight}, Length={length}");
                var addedRoll = await _service.AddAsync(weight, length);
                return CreatedAtAction(nameof(Add), new { id = addedRoll.Id }, addedRoll);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Некорректный формат параметров: {ex.Message} \n {ex.StackTrace}");
                return BadRequest($"Некорректный формат параметров: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Внутренняя ошибка сервера при добавлении рулона: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Внутренняя ошибка сервера при добавлении рулона: {ex.Message}");
            }
        }

        /// <summary>
        /// Удаление рулона со склада по ID
        /// </summary>
        /// <param name="id">ID рулона для удаления</param>
        /// <returns>Удаленный рулон с проставленной датой удаления</returns>
        /// <response code="200">Рулон успешно удален</response>
        /// <response code="404">Рулон с указанным ID не найден или уже удален</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(SteelRoll), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SteelRoll>> Remove(int id)
        {
            try
            {
                _logger.LogInformation($"Запрос на удаление рулона с ID {id}");
                var removedRoll = await _service.RemoveAsync(id);
                return Ok(removedRoll);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError($"Рулон с ID {id} не найден: {ex.Message} \n {ex.StackTrace}");
                return NotFound($"Рулон с ID {id} не найден: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Внутренняя ошибка сервера при удалении рулона с ID {id}: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Внутренняя ошибка сервера при удалении рулона с ID {id}: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение списка рулонов с возможностью фильтрации
        /// </summary>
        /// <param name="filter">Модель фильтра (критерии поиска рулонов)</param>
        /// <returns>Список рулонов, соответствующих критериям фильтрации</returns>
        /// <response code="200">Список рулонов успешно получен</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SteelRoll>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SteelRoll>>> GetAll([FromQuery] SteelRollFilter filter)
        {
            try
            {
                _logger.LogInformation("Запрос на получение списка рулонов с фильтром");
                var rolls = await _service.GetAllAsync(filter);
                return Ok(rolls);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Внутренняя ошибка сервера при получении списка рулонов: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Внутренняя ошибка сервера при получении списка рулонов: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение информации о рулоне по ID
        /// </summary>
        /// <param name="id">ID искомого рулона</param>
        /// <returns>Найденный рулон</returns>
        /// <response code="200">Рулон найден</response>
        /// <response code="404">Рулон с указанным ID не найден</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SteelRoll), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SteelRoll>> GetById(int id)
        {
            try
            {
                _logger.LogInformation($"Запрос на получение рулона с ID {id}");
                var roll = await _service.GetByIdAsync(id);

                if (roll == null)
                {
                    _logger.LogError($"Рулон с ID: {id} не найден.");
                    return NotFound($"Рулон с ID: {id} не найден.");
                }

                return Ok(roll);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Внутренняя ошибка сервера при поиске рулона с ID {id}: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Внутренняя ошибка сервера при поиске рулона: {ex.Message}");
            }
        }

        /// <summary>
        /// Получение статистики по рулонам за период
        /// </summary>
        /// <param name="from">Дата начала периода</param>
        /// <param name="to">Дата окончания периода</param>
        /// <returns>Объект со статистикой</returns>
        /// <response code="200">Статистика успешно получена</response>
        /// <response code="400">Некорректный период дат</response>
        /// <response code="500">Внутренняя ошибка сервера</response>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Укажите тип DTO статистики вместо void, если он известен, например typeof(StatsDto)
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetStats([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            try
            {
                _logger.LogInformation($"Запрос на получение статистики за период с {from} по {to}");

                if (from > to)
                {
                    var msg = "Дата начала периода не может быть позже даты окончания.";
                    _logger.LogError(msg);
                    return BadRequest(msg);
                }

                if (from > DateTime.Now)
                {
                    var msg = "Дата начала периода не может быть в будущем";
                    _logger.LogError(msg);
                    return BadRequest(msg);
                }

                if (to > DateTime.Now)
                {
                    var msg = "Дата окончания периода не может быть больше текущей даты";
                    _logger.LogError(msg);
                    return BadRequest(msg);
                }


                var stats = await _service.GetStatsAsync(from, to);
                return Ok(stats);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError($"Некорректные параметры для статистики: {ex.Message} \n {ex.StackTrace}");
                return BadRequest($"Некорректные параметры: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Внутренняя ошибка сервера при получении статистики: {ex.Message} \n {ex.StackTrace}");
                return StatusCode(StatusCodes.Status500InternalServerError, $"Внутренняя ошибка сервера при получении статистики: {ex.Message}");
            }
        }

    }
}
