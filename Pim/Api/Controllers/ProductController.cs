using Microsoft.AspNetCore.Mvc;
using Pim.Application.Services;
using Shared.Contracts.Products;

namespace Pim.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        var created = await _service.Create(request);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAll());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _service.Get(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPut("{id}/name")]
    public async Task<IActionResult> UpdateName(
        Guid id,
        UpdateProductNameRequest request)
    {
        var updated = await _service.UpdateName(id, request.Name);

        return updated ? Ok() : NotFound();
    }

    [HttpPut("{id}/image")]
    public async Task<IActionResult> UpdateImage(
        Guid id,
        UpdateProductImageRequest request)
    {
        var updated = await _service.UpdateImage(id, request.ImageUrl);

        return updated ? Ok() : NotFound();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        UpdateProductStatusRequest request)
    {
        var updated = await _service.UpdateStatus(id, request.Status);

        return updated ? Ok() : NotFound();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateProductRequest request)
    {
        var updated = await _service.Update(id, request);

        return updated ? Ok() : NotFound();
    }
}
