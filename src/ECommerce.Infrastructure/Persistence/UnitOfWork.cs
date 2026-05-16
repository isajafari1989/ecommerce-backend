using ECommerce.Application.Interfaces;
using ECommerce.Infrastructure.Persistence;
using System.Threading.Tasks;


namespace ECommerce.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}