var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<HotelDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddSingleton<ITokenService>(new TokenService());
builder.Services.AddSingleton<IUserRepository>(new UserRepository());
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/login", [AllowAnonymous] (HttpContext context,
    ITokenService TokenService, IUserRepository UserRepository) =>
        {
            UserModel userModel = new() 
            {
                UserName = context.Request.Query["username"]!,
                Password = context.Request.Query["password"]!
            };

            var userDto = UserRepository.GetUser(userModel);
            if(userDto == null) return Results.Unauthorized();

            var token = TokenService.BuildToken(builder.Configuration["Jwt:Key"]!,
                builder.Configuration["Jwt:Issuer"]!, userDto);
            
            return Results.Ok(token);
        });
// GET
// Get all
app.MapGet("/hotels", [Authorize] async (IHotelRepository repo) => 
    Results.Extensions.Xml(await repo.GetHotelsAsync()))
    .Produces<IList<Hotel>>(StatusCodes.Status200OK)
    .WithName("GetAllHotels")
    .WithTags("Getters");
    
// Search by name
app.MapGet("/hotels/search/name/{query}", 
    [Authorize] async (IHotelRepository repo, string name) => 
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
    [Authorize] async (IHotelRepository repo, Coordinate coordinate) =>
        await repo.GetHotelsAsync(coordinate) is IEnumerable<Hotel> hotels
            ? Results.Ok(hotels)
            : Results.NotFound(Array.Empty<Hotel>))
    .Produces<IList<Hotel>>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound)
    .WithName("SearchHotelsByCoordinates")
    .WithTags("Getters")
    .ExcludeFromDescription();

// Search by id
app.MapGet("/hotels/{id}", [Authorize] async (IHotelRepository repo, int id) =>
    await repo.GetHotelAsync(id) is Hotel hotel
        ? Results.Ok(hotel)
        : Results.NotFound())
    .Produces<Hotel>(StatusCodes.Status200OK)
    .WithName("GetHotel")
    .WithTags("Getters");;

// POST
app.MapPost("/hotels", [Authorize] async (IHotelRepository repo, [FromBody] Hotel hotel) =>
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
app.MapPut("/hotels", [Authorize] async (IHotelRepository repo, [FromBody] Hotel hotel) => 
    {
        await repo.UpdateHotelAsync(hotel);
        await repo.SaveAsync();
        return Results.NoContent();
    })
    .Accepts<Hotel>("application/json")
    .WithName("UpdateHotel")
    .WithTags("Updaters");

// DELETE
app.MapDelete("/hotels/{id}", [Authorize] async (IHotelRepository repo, int id) => 
    {
        await repo.DeleteHotelAsync(id);
        await repo.SaveAsync();

        return Results.NoContent();
    })
    .WithName("DeleteHotel")
    .WithTags("Deleters");

app.UseHttpsRedirection();

app.Run();