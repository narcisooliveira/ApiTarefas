using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TarefasContext>(options =>
{
    options.UseInMemoryDatabase("Tarefas");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/tarefas", async (TarefasContext db) =>
{
    return await db.Tarefas.ToListAsync();
});

app.MapGet("/tarefas/{id}", async (TarefasContext db, int id) =>
{
    return await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound();
});

app.MapGet("/tarefas/concluidas", async (TarefasContext db) =>
{
    return await db.Tarefas.Where(t => t.Concluida).ToListAsync();
});

app.MapPost("/tarefas", async (TarefasContext db, Tarefa tarefa) =>
{
    await db.Tarefas.AddAsync(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

app.MapPut("/tarefas/{id}", async (TarefasContext db, int id, Tarefa tarefa) =>
{
    if (id != tarefa.Id)
    {
        return Results.BadRequest();
    }

    var tarefaExistente = await db.Tarefas.FindAsync(id);
    if (tarefaExistente is null)
    {
        return Results.NotFound();
    }

    tarefaExistente.Titulo = tarefa.Titulo;
    tarefaExistente.Descricao = tarefa.Descricao;
    tarefaExistente.DataConclusao = tarefa.DataConclusao;
    tarefaExistente.Concluida = tarefa.Concluida;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/tarefas/{id}", async (TarefasContext db, int id) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);
    if (tarefa is null)
    {
        return Results.NotFound();
    }

    db.Tarefas.Remove(tarefa);
    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();

class Tarefa
{
    [Key]
    public int Id { get; set; }
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public DateTime DataCriacao { get; set; } = DateTime.Now;
    public DateTime DataConclusao { get; set; } = DateTime.Now.AddDays(7);
    public bool Concluida { get; set; } = false;
}

class TarefasContext : DbContext
{
    public TarefasContext(DbContextOptions<TarefasContext> options) : base(options) { }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();

}