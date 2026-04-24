using HostelListing.Api.Data;
using Microsoft.AspNetCore.Mvc;

namespace HostelListing.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class HotelsController : ControllerBase
{
    private static List<Hotel> _hotels = new List<Hotel>
    {
        new Hotel{ Id = 1, Name = "Grand Plaza", Address = "123 Main St", Rating = 4.5},
        new Hotel{ Id = 2, Name = "Ocean View", Address = "453 Beach St", Rating = 4.8}

    };

    [HttpGet]
    public ActionResult<IEnumerable<Hotel>> Get()
    {
        return Ok(_hotels);
    }

    [HttpGet("{id}")]
    public ActionResult<Hotel> Get(int id)
    {
        var hotel = _hotels.FirstOrDefault(hotel => hotel.Id == id);
        if (hotel == null)
        {
            return NotFound();
        }
        return Ok(hotel);
    }

    [HttpPost]
    public ActionResult Post([FromBody] Hotel newHotel)
    {
        if (_hotels.Any(hotel => hotel.Id == newHotel.Id))
        {

            return BadRequest($"Hotel with ID:{newHotel.Id} is already present");
        }

        _hotels.Add(newHotel);
        return CreatedAtAction(nameof(Get), new { id = newHotel.Id }, newHotel);
    }

    [HttpPut("{id}")]
    public ActionResult Put(int id, [FromBody] Hotel updatedHotel)
    {
        var existingHotel = _hotels.FirstOrDefault(hotel => id == updatedHotel.Id);
        if (existingHotel == null)
        {
            return NotFound("Hotel is not found");
        }

        existingHotel.Id = updatedHotel.Id;
        existingHotel.Name = updatedHotel.Name;
        existingHotel.Address = updatedHotel.Address;
        existingHotel.Rating = updatedHotel.Rating;

        return NoContent();

    }

    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var hotel = _hotels.FirstOrDefault(h => h.Id == id);
        if (hotel == null)
        {
            return NotFound("Hotel is not found");
        }
        _hotels.Remove(hotel);
        return NoContent();
    }
}

