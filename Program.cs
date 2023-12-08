using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Vertragsmanagement.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

//API-Dokumentation
builder.Services.AddSwaggerGen(action => {

var xmlFile = "Vertragsmanagement.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
action.IncludeXmlComments(xmlPath);
});

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
        DataCreator.InitTestData(dbc);
        //DataCreator.ValidateDatabase(dbc);
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
