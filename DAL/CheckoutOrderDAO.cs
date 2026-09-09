using Microsoft.EntityFrameworkCore;

namespace DAL;

public class CheckoutOrderDAO
{
    private ApplicationDBContext _context;

    public CheckoutOrderDAO(ApplicationDBContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Load checkout order without tracking
    /// </summary>
    /// <param name="id"></param>
    /// <returns>CheckoutOrder</returns>
    public async Task<CheckoutOrder?> LoadById(string id)
    {
        return await _context.CheckoutOrder
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.Id.Equals(id));
    }

    /// <summary>
    /// Load checkout order for update with tracking
    /// </summary>
    /// <param name="id"></param>
    /// <returns>CheckoutOrder</returns>
    public async Task<CheckoutOrder?> LoadByIdForUpdate(string id)
    {
        return await _context.CheckoutOrder
                        .FirstOrDefaultAsync(o => o.Id.Equals(id));
    }

    public async Task InsertCheckoutOrder(CheckoutOrder order)
    {
        _context.Add(order);
        await _context.SaveChangesAsync();
    }

    public async Task InsertCheckoutOrder_NoCommit(CheckoutOrder order)
    {
        _context.Add(order);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task InsertOrUpdateCheckoutOrder(CheckoutOrder order)
    {
        var existingOrder = await _context.CheckoutOrder.FindAsync(order.Id);
        if (existingOrder == null)
        {
            _context.Add(order);
        }
        else
        {
            _context.Entry(existingOrder).CurrentValues.SetValues(order);
        }

        await _context.SaveChangesAsync();
    }
}
