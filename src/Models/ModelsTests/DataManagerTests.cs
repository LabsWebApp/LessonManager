using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Text;
using Models.DataProviders.Helpers;
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
    private readonly Random _r = new (DateTime.Now.Millisecond);

    [TestMethod()]
    public void DataManager_CrudSqlServer_SuccessAllOperations()
    {
        //arrange
        var (studentsRep, coursesRep, _) = DataManager.Get(Provider.SqlServer);
        //act
        studentsRep.Add(new Student { Name = "Vasya" });
        coursesRep.Add(new Course { Name = "2 курс" });
        studentsRep.SetCourse(
            studentsRep.Items.FirstOrDefault(s => s.Name == "Vasya") ?? 
            throw new Exception(),
            coursesRep.Items.FirstOrDefault(s => s.Name == "2 курс") ??
            throw new Exception());
        //assert
        Assert.IsNotNull(studentsRep.Items.FirstOrDefault(s => s.Name == "Vasya")?.
            Courses.FirstOrDefault(s => s.Name == "2 курс"));
    }

    [TestMethod()]
    public void DataManager_CrudSqLite_SuccessAllOperations()
    {
        //arrange
        var (studentsRep, coursesRep, _) = DataManager.Get(Provider.SqLite); 
        var id = new Guid("B2825809-2E3D-43C1-BFAA-8D29F6C266E7");
        //act
        studentsRep.Add(new Student { Id = id, Name = "Vasya2" });
        coursesRep.Add(new Course { Name = "2 курс" });
        studentsRep.SetCourse(
            studentsRep.GetStudentById(id) ??
            throw new Exception("Студент есть"),
            coursesRep.Items.FirstOrDefault(s => s.Name == "2 курс") ??
            throw new Exception("Курс есть"));
        //assert
        Assert.IsNotNull(studentsRep.Items.FirstOrDefault(s => s.Id == id)?.
            Courses.FirstOrDefault(s => s.Name == "2 курс"));
    }

    [TestMethod()]
    public void DataManager_10000Students1000CoursesAddSqlServer_Ok()
    {
        //arrange
        const int a = 'а', z = 'я' + 1, min = 2, max = 16;

        string Build(bool course)
        {
            var res = new StringBuilder(course ? "Курс: " : string.Empty);
            for (var i = 0; i < 3; i++)
            {
                var len = _r.Next(min, max) + 1;
                res.Capacity = res.Length + len;
                for (var j = 0; j < len; j++)
                {
                    var c = (char)_r.Next(a, z);
                    res.Append(j > 0 ? c : c.ToString().ToUpper());
                }
                res.Append(' ');
            }
            return res.ToString().TrimEnd();
        }
        var (studentsRep, coursesRep, _) = DataManager.Get(Provider.SqlServer);
        //act 
        for (var i = 0; i < 1000; i++)
            coursesRep.Add(new Course { Name = Build(true) });
        for (var i = 0; i < 10000; i++)
        {
            var student = new Student { Name = Build(false) };
            studentsRep.Add(student);

            var count = _r.Next(20) - 10;
            if (count <= 0) continue;
            var items = coursesRep.Items.ToList();

            for (int j = 0; j < count; j++)
            {
                var course = items
                    .Skip(_r.Next(items.Count))
                    .Take(1)
                    .FirstOrDefault();
                if (course != null) studentsRep.SetCourse(student, course);
            }
        }
        Assert.IsFalse(false);
    }
    [TestMethod()]
    public void DataManager_10000Students1000CoursesAddSqLite_Ok()
    {
        //arrange
        const int a = 'а', z = 'я' + 1, min = 2, max = 16;

        string Build(bool course)
        {
            var res = new StringBuilder(course ? "Курс: " : string.Empty);
            for (var i = 0; i < 3; i++)
            {
                var len = _r.Next(min, max) + 1;
                res.Capacity = res.Length + len;
                for (var j = 0; j < len; j++)
                {
                    var c = (char)_r.Next(a, z);
                    res.Append(j > 0 ? c : c.ToString().ToUpper());
                }
                res.Append(' ');
            }
            return res.ToString().TrimEnd();
        }
        var (studentsRep, coursesRep, _) = DataManager.Get(Provider.SqLite);
        //act 
        for (var i = 0; i < 50; i++)
            coursesRep.Add(new Course { Name = Build(true) });
        for (var i = 0; i < 2000; i++)
        {
            var student = new Student { Name = Build(false) };
            studentsRep.Add(student);

            var count = _r.Next(10);
            if (count <= 0) continue;
            var items = coursesRep.Items.ToList();

            for (int j = 0; j < count; j++)
            {
                var course = items
                    .Skip(_r.Next(items.Count))
                    .Take(1)
                    .FirstOrDefault();
                if (course != null) studentsRep.SetCourse(student, course);
            }
        }
        Assert.IsFalse(false);
    }

}