var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();

var app = builder.Build();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}

// GET
// Get all
app.MapGet("/hotels", async (IHotelRepository repo) => 
    Results.Extensions.Xml(await repo.GetHotelsAsync()))
    .Produces<IList<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");
    
// Search by name
app.MapGet("/hotels/search/name/{query}", 
    async (IHotelRepository repo, string name) => 
        await repo.GetHotelsAsync(name) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>()))
    .Produces<IList<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotelsByName")
    .WithTags("Getters")
    .ExcludeFromDescription();

// Search by coorditanes
app.MapGet("/hotels/search/location/{coordinate}", 
    async (IHotelRepository repo, Coordinate coordinate) =>
        await repo.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>))
    .Produces<IList<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotelsByCoordinates")
    .WithTags("Getters")
    .ExcludeFromDescription();

// Search by id
app.MapGet("/hotels/{id}", async (IHotelRepository repo, int id) =>
    await repo.GetHotelAsync(id) is Hotel hotel
        ? Results.Ok(hotel)
        : Results.NotFound())
    .Produces<Hotel>(StatusCodes.Status200OK)
    .WithName("GetHotel")
    .WithTags("Getters");;

// POST
app.MapPost("/hotels", async (IHotelRepository repo, [FromBody] Hotel hotel) =>
    {
        await repo.InsertHotelAsync(hotel);
        await repo.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    })
    .Accepts<Hotel>("application/json")
    .Produces<Hotel>(StatusCodes.Status201Created)
    .WithName("CreateHotel")
    .WithTags("Creators");

// PUT
app.MapPut("/hotels", async (IHotelRepository repo, [FromBody] Hotel hotel) => 
    {
        await repo.UpdateHotelAsync(hotel);
        await repo.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .WithName("UpdateHotel")
    .WithTags("Updaters");

// DELETE
app.MapDelete("/hotels/{id}", async (IHotelRepository repo, int id) => 
    {
        await repo.DeleteHotelAsync(id);
        await repo.SaveAsync();

        return Results.NoContent();
    })
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.UseHttpsRedirection();

app.Run();