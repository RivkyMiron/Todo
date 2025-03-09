using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using TodoApi;
using Microsoft.AspNetCore.Mvc;

// const app = express()
// //const port = 3001

// app.use(bodyParser.json())
// app.use(cors())
var builder = WebApplication.CreateBuilder(args);

//פתרון בעיית הCORS
builder.Services.AddCors(option => option.AddPolicy("AllowAll",//נתינת שם להרשאה
    p => p.AllowAnyOrigin()//מאפשר כל מקור
    .AllowAnyMethod()//כל מתודה - פונקציה
    .AllowAnyHeader()));//וכל כותרת פונקציה

builder.Services.AddDbContext<ToDoDbContext>(options => options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"),
new MySqlServerVersion(new Version(8, 0, 21))));

builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen((c)=>c.SwaggerDoc("v1",new OpenApiInfo{Title="My Api",Version="v1"}));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//CORS
app.UseCors("AllowAll");

app.MapGet("/", async (ToDoDbContext db) =>
{
    return await db.Items.ToListAsync();
});

// app.MapGet("/get", () => "getAll");

app.MapPost("/post", async (ToDoDbContext db, Item item) =>
{  
    db.Items.Add(item);
    await db.SaveChangesAsync();
    return Results.Created($"/items/{item.Name}", item);
});

app.MapDelete("/delete/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item != null) 
    db.Items.Remove(item);
    await db.SaveChangesAsync();
});

app.MapPut("/update", async (ToDoDbContext db,Item updatedItem) =>
{
    var item = await db.Items.FirstOrDefaultAsync(i => i.Id==updatedItem.Id);
    if (item != null) 
    item.IsComplete = updatedItem.IsComplete; // עדכן שדות לפי הצורך
    await db.SaveChangesAsync();
});

app.Run();
