var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if(app.Environment.IsDevelopment()) {
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}
// GET
app.MapGet("/hotels", async (IHotelRepository repo) => 
    Results.Ok(await repo.GetHotelsAsync()));

app.MapGet("/hotels/{id}", async (IHotelRepository repo, int id) =>
    await repo.GetHotelAsync(id) is Hotel hotel ?
        Results.Ok(hotel)
        : Results.NotFound());

// POST
app.MapPost("/hotels", async (IHotelRepository repo, [FromBody] Hotel hotel) =>
    {
        await repo.InsertHotelAsync(hotel);
        await repo.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    });

// PUT
app.MapPut("/hotels", async (IHotelRepository repo, [FromBody] Hotel hotel) => {
    await repo.UpdateHotelAsync(hotel);
    await repo.SaveAsync();

    return Results.NoContent();
});

// DELETE
app.MapDelete("/hotels/{id}", async (IHotelRepository repo, int id) => {
    await repo.DeleteHotelAsync(id);
    await repo.SaveAsync();
    
    return Results.NoContent();
});

app.UseHttpsRedirection();

app.Run();