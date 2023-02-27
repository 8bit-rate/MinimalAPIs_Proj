var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});

var app = builder.Build();

if(app.Environment.IsDevelopment()) {
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}
// GET
app.MapGet("/hotels", async (HotelDbContext db) => await db.Hotels.ToListAsync());

app.MapGet("/hotels/{id}", async (HotelDbContext db, int id) =>
    await db.Hotels.FirstOrDefaultAsync(h => h.Id == id) is Hotel hotel ?
        Results.Ok(hotel)
        : Results.NotFound());

// POST
app.MapPost("/hotels", async (HotelDbContext db, [FromBody] Hotel hotel) =>
    {
        db.Hotels.Add(hotel);
        await db.SaveChangesAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    });


// PUT
app.MapPut("/hotels", async (HotelDbContext db, [FromBody] Hotel hotel) => {
    var hotelFromDb = await db.Hotels.FindAsync(new object[] {hotel.Id});

    if (hotelFromDb == null) return Results.NotFound();
    
    hotelFromDb.Name = hotel.Name;
    hotelFromDb.Latitude = hotel.Latitude;
    hotelFromDb.Longitude = hotel.Longitude;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

// DELETE
app.MapDelete("/hotels/{id}", async (HotelDbContext db, int id) => {
    var hotelFromDb = await db.Hotels.FindAsync(new object[] {id});

    if (hotelFromDb == null) return Results.NotFound();

    db.Hotels.Remove(hotelFromDb);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();

public class HotelDbContext : DbContext {
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options) {}

    public DbSet<Hotel> Hotels => Set<Hotel>();
}

public class Hotel {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}