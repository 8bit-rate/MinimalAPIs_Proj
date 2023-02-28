public interface IHotelRepository : IDisposable
{
    Task<IList<Hotel>> GetHotelsAsync();
    Task<IList<Hotel>> GetHotelsAsync(string name);
    Task<Hotel> GetHotelAsync(int id);
    Task InsertHotelAsync(Hotel hotel);
    Task UpdateHotelAsync(Hotel hotel);
    Task DeleteHotelAsync(int id);
    Task SaveAsync();
}