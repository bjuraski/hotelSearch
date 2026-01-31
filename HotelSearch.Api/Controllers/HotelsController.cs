using HotelSearch.Application.Dtos;
using HotelSearch.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelSearch.Api.Controllers;

/// <summary>
/// Endpoints for hotel management and search operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class HotelsController : ControllerBase
{
    private readonly IHotelService _hotelService;

    public HotelsController(IHotelService hotelService)
    {
        _hotelService = hotelService;
    }
    
    /// <summary>
    /// Creates a new hotel
    /// </summary>
    /// <param name="createHotelDto">Hotel creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created hotel</returns>
    /// <response code="201">Hotel created successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="409">Hotel with same name and location already exists</response>
    [HttpPost]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateHotel([FromBody] CreateHotelDto createHotelDto, CancellationToken cancellationToken)
    {
        var hotel = await _hotelService.CreateAsync(createHotelDto, cancellationToken);
        return CreatedAtAction(nameof(GetHotel), new { id = hotel.Id }, hotel);
    }
    
    /// <summary>
    /// Gets a hotel by ID
    /// </summary>
    /// <param name="id">Hotel ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Hotel details</returns>
    /// <response code="200">Hotel found</response>
    /// <response code="404">Hotel not found</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHotel(Guid id, CancellationToken cancellationToken)
    {
        var hotel = await _hotelService.GetByIdAsync(id, cancellationToken);
        
        if (hotel is null)
            return NotFound(new ProblemDetails 
            { 
                Title = "Hotel not found",
                Detail = $"Hotel with ID: {id} not found",
                Status = StatusCodes.Status404NotFound,
                Instance = HttpContext.Request.Path
            });

        return Ok(hotel);
    }
    
    /// <summary>
    /// Updates an existing hotel
    /// </summary>
    /// <param name="id">Hotel ID</param>
    /// <param name="updateHotelDto">Updated hotel data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated hotel</returns>
    /// <response code="200">Hotel updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="404">Hotel not found</response>
    /// <response code="409">Hotel with same name and location already exists</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HotelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateHotel(Guid id, [FromBody] UpdateHotelDto updateHotelDto, CancellationToken cancellationToken)
    {
        var hotel = await _hotelService.UpdateAsync(id, updateHotelDto, cancellationToken);
        return Ok(hotel);
    }
    
    /// <summary>
    /// Deletes a hotel
    /// </summary>
    /// <param name="id">Hotel ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Hotel deleted successfully</response>
    /// <response code="404">Hotel not found</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHotel(Guid id, CancellationToken cancellationToken)
    {
        await _hotelService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
    
    /// <summary>
    /// Searches for hotels near a location with intelligent ranking
    /// </summary>
    /// <param name="latitude">User's latitude (-90 to 90)</param>
    /// <param name="longitude">User's longitude (-180 to 180)</param>
    /// <param name="pageNumber">Page number (minimum: 1, default: 1)</param>
    /// <param name="pageSize">Page size (1-100, default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of hotels ordered by proximity and price</returns>
    /// <response code="200">Search completed successfully</response>
    /// <response code="400">Invalid search parameters</response>
    /// <remarks>
    /// Hotels are ranked using a scoring algorithm that considers both distance and price.
    /// Lower score = better match (closer and cheaper hotels rank higher).
    /// 
    /// Sample request:
    /// 
    ///     GET /api/hotels/search?latitude=45.8150&amp;longitude=15.9819&amp;pageNumber=1&amp;pageSize=10
    /// 
    /// </remarks>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<HotelSearchResultDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchHotels(
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new HotelSearchQuery(latitude, longitude, pageNumber, pageSize);
        var result = await _hotelService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }
}