
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Database connection
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString)
    ));

    builder.Services.AddScoped<IAnimalService, AnimalService>();
    builder.Services.AddScoped<IAnimalRepository, AnimalRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// We are using HTTP for now while learning.
// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing",
    "Bracing",
    "Chilly",
    "Cool",
    "Mild",
    "Warm",
    "Balmy",
    "Hot",
    "Sweltering",
    "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5)
        .Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");


app.MapGet("/hello", () =>
{
    return "Hello from my Animal Shelter!";
});


app.MapGet("/animals", () =>
{
    var animals = new List<Animal>
    {
        new Animal
        {
            Id = 1,
            Name = "Misho",
            Age = 2,
            Type = "Cat"
        },

        new Animal
        {
            Id = 2,
            Name = "Rocky",
            Age = 4,
            Type = "Dog"
        },

        new Animal
        {
            Id = 3,
            Name = "Luna",
            Age = 1,
            Type = "Cat"
        },

        new Animal
        {
            Id = 4,
            Name = "Max",
            Age = 6,
            Type = "Dog"
        }
    };

    return animals;
});


/*app.MapGet("/animals-db", async (AppDbContext db) =>
{
    var animals = await db.Animals.ToListAsync();

    return animals;
});*/
//new version using service
app.MapGet("/animals-db", async (IAnimalService service) =>
{
    return await service.GetAllAsync();
});

/*app.MapGet("/animals/{id}", async (int id, AppDbContext db) => {
    var animal = await db.Animals
        .FirstOrDefaultAsync(a => a.Id == id);

    if(animal == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(animal);
});*/
//new vwrsion using service 
app.MapGet("/animals/{id}", async (
    int id,
    IAnimalService service) =>
{
    var animal = await service.GetByIdAsync(id);

    if (animal == null)
        return Results.NotFound();

    return Results.Ok(animal);
});

/*app.MapPost("/animals", async (Animal animal, AppDbContext db) =>
{
    db.Animals.Add(animal);

    await db.SaveChangesAsync();

    return Results.Created($"/animals/{animal.Id}", animal);
});*/

/*app.MapPut("/animals/{id}", async (int id, Animal updatedAnimal, AppDbContext db) =>
{
    var animal = await db.Animals
        .FirstOrDefaultAsync(a => a.Id == id);

    if (animal == null)
    {
        return Results.NotFound();
    }

    animal.Name = updatedAnimal.Name;
    animal.Age = updatedAnimal.Age;
    animal.Type = updatedAnimal.Type;

    await db.SaveChangesAsync();

    return Results.Ok(animal);
});*/



app.MapPost("/animals", async (
    CreateAnimalDto dto,
    IAnimalService service) =>
{
    if (!Validator.TryValidateObject(
        dto,
        new ValidationContext(dto),
        null,
        true))
    {
        return Results.BadRequest("Invalid animal data.");
    }

    var animal = new Animal
    {
        Name = dto.Name,
        Age = dto.Age,
        Type = dto.Type
    };

    var createdAnimal = await service.CreateAsync(animal);

    return Results.Created(
        $"/animals/{createdAnimal.Id}",
        createdAnimal);
});

/*app.MapPut("/animals/{id}", async (
    int id,
    UpdateAnimalDto dto,
    AppDbContext db) =>
{
    var animal = await db.Animals
        .FirstOrDefaultAsync(a => a.Id == id);

    if (animal == null)
    {
        return Results.NotFound();
    }

    animal.Name = dto.Name;
    animal.Age = dto.Age;
    animal.Type = dto.Type;

    await db.SaveChangesAsync();

    return Results.Ok(animal);
});*/



app.MapPut("/animals/{id}", async (
    int id,
    UpdateAnimalDto dto,
    IAnimalService service) =>
{
    if (!Validator.TryValidateObject(
        dto,
        new ValidationContext(dto),
        null,
        true))
    {
        return Results.BadRequest("Invalid animal data.");
    }

    var animal = new Animal
    {
        Name = dto.Name,
        Age = dto.Age,
        Type = dto.Type
    };

    var updated = await service.UpdateAsync(id, animal);

    if (!updated)
        return Results.NotFound();

    return Results.Ok(animal);
});

/*app.MapDelete("/animals/{id}", async (
    int id,
    AppDbContext db) =>
{
    var animal = await db.Animals
        .FirstOrDefaultAsync(a => a.Id == id);

    if (animal == null)
    {
        return Results.NotFound();
    }

    db.Animals.Remove(animal);

    await db.SaveChangesAsync();

    return Results.NoContent();
});*/

app.MapDelete("/animals/{id}", async (
    int id,
    IAnimalService service) =>
{
    var deleted = await service.DeleteAsync(id);

    if (!deleted)
        return Results.NotFound();

    return Results.NoContent();
});


/*app.MapGet("/test-error", () =>
{
    throw new Exception("This is a test exception.");
});*/

app.Run();


record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}

