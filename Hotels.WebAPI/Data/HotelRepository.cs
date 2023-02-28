public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public HotelRepository(HotelDbContext context) => _context = context;
    public async Task<IList<Hotel>> GetHotelsAsync() => await _context.Hotels.ToListAsync();
    public async Task<Hotel> GetHotelAsync(int id) =>
        await _context.Hotels.FindAsync(new object[] {id});
        
    public async Task InsertHotelAsync(Hotel hotel) => await _context.AddAsync(hotel);
    public async Task UpdateHotelAsync(Hotel hotel) 
    {
        var hotelFromDb = await _context.Hotels.FindAsync(new object[] {hotel.Id});

        if(hotelFromDb == null) return;

        hotelFromDb.Name = hotel.Name;
        hotelFromDb.Latitude = hotel.Latitude;
        hotelFromDb.Longitude = hotel.Longitude;          
    }
    public async Task DeleteHotelAsync(int id)
    {
        var hotelFromDb = await _context.Hotels.FindAsync(new object[] {id});

        if(hotelFromDb == null) return;

        _context.Hotels.Remove(hotelFromDb);
    } 
    public async Task SaveAsync() => await _context.SaveChangesAsync();

    private bool _disposed = false;
    protected virtual void Dispose(bool disposing)
    {
        if(!_disposed)
            if(disposing) _context.Dispose();
        
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}