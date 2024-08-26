using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "Host=localhost;Database=postgres;Username=angelito;Password=211125";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();

app.MapGet("/users", async (ApplicationDbContext dbContext) =>
{
    var users = await dbContext.Users.ToListAsync();
    return Results.Ok(users);
})
.WithName("GetUsers");

app.MapPost("/users", async (ApplicationDbContext dbContext, UserDto userDto) =>
{
    var user = new User
    {
        Name = userDto.Name,
        Email = userDto.Email,
        Password = userDto.Password, 
        Admin = userDto.Email.EndsWith("@admin.com")
    };

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser");

app.Run();

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool Admin { get; set; } 
}

public class UserDto
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    }
}
