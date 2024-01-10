using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;

using PizzaStore.Models;
// using PizzaStore.DB;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("PizzaDbConnection") ?? "Data Source=Pizzas.db";

builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddDbContext<PizzaDb>(options => options.UseInMemoryDatabase("items"));
builder.Services.AddDbContext<PizzaDb>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new OpenApiInfo
  {
    Title = "PizzaStore API",
    Description = "Making the Pizzas you love",
    Version = "v1"
  });
});

// CORS
// 1) define a unique string
string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

// 2) define allowed domains, in this case "http://example.com" and "*" = all
//    domains, for testing purposes only.
builder.Services.AddCors(options =>
{
  options.AddPolicy(name: MyAllowSpecificOrigins,
    builder =>
    {
      builder.WithOrigins(
          "http://example.com", "*");
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
  c.SwaggerEndpoint("/swagger/v1/swagger.json", "PizzaStore API V1");
});

// use the capability
app.UseCors(MyAllowSpecificOrigins);
app.MapGet("/", () => "Hello World!");
app.MapGet("/pizzas", async (PizzaDb db) => await db.Pizzas.ToListAsync());
app.MapPost("/pizzas", async (PizzaDb db, Pizza pizza) =>
{
  await db.Pizzas.AddAsync(pizza);
  await db.SaveChangesAsync();
  return Results.Created($"/pizzas/{pizza.Id}", pizza);
});
app.MapGet("/pizzas/{id}", async (PizzaDb db, int id) => await db.Pizzas.FindAsync(id));
app.MapPut("/pizzas/{id}", async (PizzaDb db, Pizza updatepizza, int id) =>
{
  var pizza = await db.Pizzas.FindAsync(id);
  if (pizza is null) return Results.NotFound();
  pizza.Name = updatepizza.Name;
  pizza.Description = updatepizza.Description;
  await db.SaveChangesAsync();
  return Results.NoContent();
});
app.MapDelete("/pizzas/{id}", async (PizzaDb db, int id) =>
{
  var pizza = await db.Pizzas.FindAsync(id);
  if (pizza is null)
  {
    return Results.NotFound();
  }
  db.Pizzas.Remove(pizza);
  await db.SaveChangesAsync();
  return Results.Ok();
});

app.Run();