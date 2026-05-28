using Ecom.Application.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.Products;

namespace Ecom.Application.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _db.Products
            .Where(product => product.Status == ProductStatuses.Published)
            .ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Status == ProductStatuses.Published);

        if (product == null)
            return NotFound();

        return Ok(product);
    }
}
