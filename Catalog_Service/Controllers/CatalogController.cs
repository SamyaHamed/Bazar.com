using CATALOGSERVICE.Data;
using CATALOGSERVICE.dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CATALOGSERVICE.Controller
{
    [ApiController]
    [Route("api/catalog/book")]
    public class CatalogController(CatalogDbContext _context ) : ControllerBase
    {
         private const int MaxRetries = 2;

        [HttpGet("search/{topic}")]
        public async Task<IActionResult> Search(string topic)
        {
            Console.WriteLine("\n==============================");
            Console.WriteLine(" SEARCH REQUEST");
            Console.WriteLine($"Topic: {topic}");
            Console.WriteLine("------------------------------");


            var response = await _context.Books
            .Where(b => b.Topic.ToLower() == topic.Trim().ToLower())
            .Select(b => new SearchResponseDto
            {
                Id = b.Id,
                Title = b.Title

            }).ToListAsync();


            if (response.Count() == 0)
            {
                Console.WriteLine($" No books found for topic: {topic}");
                Console.WriteLine("==============================\n");
                return NotFound(new { message = $"No books found for this topic '{topic}'" });

            }

            Console.WriteLine($"Found {response.Count} book(s):");
            foreach (var r in response)
                Console.WriteLine($"   - [{r.Id}] {r.Title}");

            Console.WriteLine("==============================\n");
            return Ok(response);

        }
        

        [HttpGet("info/{id}")]
        public async Task<IActionResult> GetInformation(int id)
        {
            Console.WriteLine("\n==============================");
            Console.WriteLine("INFO REQUEST");
            Console.WriteLine($"Book ID: {id}");
            Console.WriteLine("------------------------------");
    
            var book = await _context.Books.FindAsync(id);
            if (book == null)
            {
                Console.WriteLine($"Book with ID {id} not found.");
                Console.WriteLine("==============================\n");
                return NotFound(new { message = "Book not found" });
        
            }

            var response = new InfoResponseDto
            {
                Title = book.Title,
                Price = book.Price,
                Quantity =book.Quantity
            };
            
            Console.WriteLine($" Book found:");
            Console.WriteLine($"   - Title: {response.Title}");
            Console.WriteLine($"   - Quantity: {response.Quantity}");
            Console.WriteLine($"   - Price: {response.Price}");
            Console.WriteLine("==============================\n");
    
            return Ok(response);
            


        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, UpdateQuantityOfBook dto)
        {
            for (int i = 1; i <= MaxRetries; i++)
            {
                using var TX = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
                var book = await _context.Books
                .Where(b => b.Id == id)
                .FirstOrDefaultAsync();

                if (book == null)
                    return NotFound(new { message = "Book not found" });

                int newQuantity = book.Quantity + dto.QuantityDelta;
                if (newQuantity < 0)
                    return BadRequest(new { message = "Available quantity is zero" });
                book.Quantity = newQuantity;

                try
                {
                    await _context.SaveChangesAsync();
                    await TX.CommitAsync();

                    return Ok(new
                    {
                        message = "Quantity updated successfully",
                        id = book.Id,
                        title = book.Title,
                        quantity = book.Quantity
                    });
                }
                catch (DbUpdateConcurrencyException)
                {
                    await TX.RollbackAsync();

                    if (i == MaxRetries)
                        return Conflict(new { message = "Conflict occurred, please try again later" });

                    await Task.Delay(50);
                }
                catch
                {
                    await TX.RollbackAsync();
                    return StatusCode(500, new { message = "Failed to update Quantity" });
                }
            }

            return StatusCode(500, new { message = "error" });
 
        }


    }
        
}