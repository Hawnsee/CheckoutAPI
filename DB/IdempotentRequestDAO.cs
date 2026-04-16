using CheckoutAPI.Domain;
using Microsoft.EntityFrameworkCore;

namespace CheckoutAPI.DB;

public class IdempotentRequestDAO
{
    private ApplicationDBContext _context;

    public IdempotentRequestDAO(ApplicationDBContext context)
    {
        _context = context;
    }

    public async Task<IdempotentRequest?> LoadByKey(string key)
    {
        return await _context.IdempotentRequest
                        .AsNoTracking()
                        .FirstOrDefaultAsync(i => i.Key.Equals(key));
    }

    public async Task InsertIdempotentRequest(IdempotentRequest idempotentRequest)
    {
        _context.Add(idempotentRequest);
        await _context.SaveChangesAsync();
    }

    public async Task InserOrUpdatetIdempotentRequest(IdempotentRequest idempotentRequest)
    {
        var existingRequest = await _context.IdempotentRequest.FindAsync(idempotentRequest.Key);
        if (existingRequest == null)
        {
            _context.Add(idempotentRequest);
        }
        else
        {
            _context.Entry(existingRequest).CurrentValues.SetValues(idempotentRequest);
        }

        await _context.SaveChangesAsync();
    }
}