public class HotelApi : IApi
{
    public void Register(WebApplication app)
    {
        // GET
        // Get all
        app.MapGet("/hotels", Get)
            .Produces<IList<Hotel>>(StatusCodes.Status200OK)
            .WithName("GetAllHotels")
            .WithTags("Getters");
            
        // Search by name
        app.MapGet("/hotels/search/name/{query}", SearchByName)    
            .Produces<IList<Hotel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("SearchHotelsByName")
            .WithTags("Getters")
            .ExcludeFromDescription();

        // Search by coorditanes
        app.MapGet("/hotels/search/location/{coordinate}", SearchByLocation)
            .Produces<IList<Hotel>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .WithName("SearchHotelsByCoordinates")
            .WithTags("Getters")
            .ExcludeFromDescription();

        // Search by id
        app.MapGet("/hotels/{id}", GetById)
            .Produces<Hotel>(StatusCodes.Status200OK)
            .WithName("GetHotel")
            .WithTags("Getters");;

        // POST
        app.MapPost("/hotels", Post)
            .Accepts<Hotel>("application/json")
            .Produces<Hotel>(StatusCodes.Status201Created)
            .WithName("CreateHotel")
            .WithTags("Creators");

        // PUT
        app.MapPut("/hotels", Put)
            .Accepts<Hotel>("application/json")
            .WithName("UpdateHotel")
            .WithTags("Updaters");

        // DELETE
        app.MapDelete("/hotels/{id}", Delete)
            .WithName("DeleteHotel")
            .WithTags("Deleters");
    }

    [Authorize]
    private async Task<IResult> Get(IHotelRepository repo) =>
        Results.Extensions.Xml(await repo.GetHotelsAsync());

    [Authorize]
    private async Task<IResult> SearchByName(IHotelRepository repo, string name) => 
        await repo.GetHotelsAsync(name) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>());

    [Authorize]
    private async Task<IResult> GetById(IHotelRepository repo, int id) =>
        await repo.GetHotelAsync(id) is Hotel hotel
            ? Results.Ok(hotel)
            : Results.NotFound();

    [Authorize]
    private async Task<IResult> SearchByLocation(IHotelRepository repo, Coordinate coordinate) =>
                await repo.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
                    ? Results.Ok(hotels)
                    : Results.NotFound(Array.Empty<Hotel>);

    [Authorize]
    private async Task<IResult> Post(IHotelRepository repo, [FromBody] Hotel hotel)
    {
        await repo.InsertHotelAsync(hotel);
        await repo.SaveAsync();
        return Results.Created($"/hotels/{hotel.Id}", hotel);
    }

    [Authorize]
    private async Task<IResult> Put(IHotelRepository repo, [FromBody] Hotel hotel) 
    {
        await repo.UpdateHotelAsync(hotel);
        await repo.SaveAsync();
        return Results.NoContent();
    }

    [Authorize]
    private async Task<IResult> Delete(IHotelRepository repo, int id)
    {
        await repo.DeleteHotelAsync(id);
        await repo.SaveAsync();
        return Results.NoContent();
    }
}