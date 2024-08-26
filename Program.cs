using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Configurar la cadena de conexión a la base de datos PostgreSQL
var connectionString = "Host=localhost;Database=postgres;Username=angelito;Password=211125";

// Configurar el contexto de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Agregar servicios de Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

// Aplicar migraciones automáticas al inicio
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    // Habilitar middleware de Swagger solo en desarrollo
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseHttpsRedirection();

// Crear el endpoint para obtener todos los usuarios
app.MapGet("/users", async (ApplicationDbContext dbContext) =>
{
    var users = await dbContext.Users.ToListAsync();
    return Results.Ok(users);
})
.WithName("GetUsers");

// Crear el endpoint para agregar un nuevo usuario
app.MapPost("/users", async (ApplicationDbContext dbContext, UserDto userDto) =>
{
    var user = new User
    {
        Name = userDto.Name,
        Email = userDto.Email,
        Password = userDto.Password, // En un entorno real, debes hash la contraseña antes de almacenarla
        Admin = userDto.Email.EndsWith("@admin.com")
    };

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    return Results.Created($"/users/{user.Id}", user);
})
.WithName("CreateUser");

app.Run();

// Definir el modelo de usuario
public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public bool Admin { get; set; } // Nuevo parámetro
}

// DTO para recibir datos del usuario
public class UserDto
{
    public string Name { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

// Configurar el contexto de la base de datos
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración adicional del modelo si es necesario
    }
}
