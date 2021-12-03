using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Models.DataProviders.SqLite;
using Models.DataProviders.SqLite.Repositories;
using Models.DataProviders.SqlServer;
using Models.DataProviders.SqlServer.Repositories;
using Models.Entities;

namespace Models.Tests;

[TestClass()]
public class DataManagerTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod()]
    public void DataManager_CrudSqlServer_SuccessAllOperations()
    {
        //arrange
        SqlSerDbContext context = new ();
        DataManager data = new (new SqlServerStudents(context), new SqlServerCourses(context));
        //act
        data.StudentsRep.Add(new Student { Name = "Vasya" });
        data.CoursesRep.Add(new Course { Name = "2 курс" });
        data.StudentsRep.SetCourse(
            data.StudentsRep.Items.FirstOrDefault(s => s.Name == "Vasya") ?? 
            throw new Exception(),
            data.CoursesRep.Items.FirstOrDefault(s => s.Name == "2 курс") ??
            throw new Exception());
        //assert
        Assert.IsNotNull(data.StudentsRep.Items.FirstOrDefault(s => s.Name == "Vasya")?.
            Courses.FirstOrDefault(s => s.Name == "2 курс"));
    }

    [TestMethod()]
    public void DataManager_CrudSqLite_SuccessAllOperations()
    {
        //arrange
        SqLiteDbContext context = new ();
        DataManager data = new (new SqLiteStudents(context), new SqLiteCourses(context));
        var id = new Guid("B2825809-2E3D-43C1-BFAA-8D29F6C266E7");
        //act
        data.StudentsRep.Add(new Student { Id = id, Name = "Vasya2" });
        data.CoursesRep.Add(new Course { Name = "2 курс" });
        data.StudentsRep.SetCourse(
            data.StudentsRep.GetStudentById(id) ??
            throw new Exception("Студент есть"),
            data.CoursesRep.Items.FirstOrDefault(s => s.Name == "2 курс") ??
            throw new Exception("Курс есть"));
        //assert
        Assert.IsNotNull(data.StudentsRep.Items.FirstOrDefault(s => s.Id == id)?.
            Courses.FirstOrDefault(s => s.Name == "2 курс"));
    }
}