using Models;
using Models.DataProviders.Helpers;
using Models.Entities;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

DataManager data = DataManager.Get(Provider.SqlServer);

var serializerSettings = new JsonSerializerSettings
{
    PreserveReferencesHandling = PreserveReferencesHandling.Objects
};

string SerializeObject(object obj, bool preserve = true) => JsonConvert.SerializeObject(
    obj,
    Formatting.Indented,
    preserve ? serializerSettings : null);

//app.MapGet("/", () => "Hello World!");
app.MapGet("/api/students", async () => await Task.Run(() =>
    SerializeObject(data.StudentsRep.ProxyItems.ToList(), false)));
app.MapGet("/api/courses", async () => await Task.Run(() =>
    SerializeObject(data.CoursesRep.ProxyItems.ToList(), false)));
app.MapGet("/api/students/{id}", async (Guid id) =>
    await Task.Run(async () =>
        SerializeObject(await data.StudentsRep.GetStudentByIdAsync(id) ??
                        new Student { Name = "default" })));
app.MapGet("/api/courses/{id}", async (Guid id) =>
    await Task.Run(async () =>
        SerializeObject(await data.CoursesRep.GetCourseByIdAsync(id) ??
                        new Course { Name = "default" })));

app.MapDelete("/api/students/{id}", async (Guid id) =>
{
    var student = await data.StudentsRep.GetStudentByIdAsync(id);
    if (student == null) return Results.NotFound();

    try
    {
        await data.StudentsRep.DeleteAsync(id);
        return Results.Bytes(new byte[]{1});
    }
    catch
    {
        return Results.Bytes(new byte[] { 0 });
    }
});
app.MapDelete("/api/courses/{id}", async (Guid id) =>
{
    var course = await data.CoursesRep.GetCourseByIdAsync(id);
    if (course == null) return Results.NotFound();

    await data.CoursesRep.DeleteAsync(id);

    return Results.NoContent();
});


app.Run();