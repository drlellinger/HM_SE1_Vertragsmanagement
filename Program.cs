using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Vertragsmanagement.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<Vertragsmanagement.DataAccess.DatabaseContext>(options =>
{
    options.UseInMemoryDatabase("MyInMemDatabase");
});


var app = builder.Build();

//Init test data if solution is in debug mode
#if DEBUG

using (var scope = app.Services.CreateScope())
{
    using (var dbc = scope.ServiceProvider.GetRequiredService<DatabaseContext>())
    {
        //In DataAccess, dort können Sie Testdaten zur In-Memory-Datenbank hinzufügen
        TestDataCreator.InitTestData(dbc);
    }
}

#endif


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();