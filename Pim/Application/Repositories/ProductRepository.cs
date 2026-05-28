using Pim.Domain.Entities;

namespace Pim.Application.Repositories;

public interface IProductRepository
{
    Task<Product?> Get(Guid id);

    Task<List<Product>> GetAll();

    Task Create(Product product);

    Task Update(Product product);
}