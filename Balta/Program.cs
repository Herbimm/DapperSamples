using Balta.Models;
using Dapper;
using System.Data;
using System.Data.SqlClient;

const string connectionString = "Server=localhost,1433;Database=baltaDapper;User ID=sa;Password=1q2w3e4r@#$;TrustServerCertificate=True;Encrypt=True";

using (var connection = new SqlConnection(connectionString))
{
    //CreateManyCategory(connection);
    //DeleteCategory(connection);
    //ExecuteProcedure(connection);
    //ExecuteReadProcedure(connection);
    //UpdateCategory(connection);   
    //CreateCategory(connection);
    //ListCategories(connection);
    //OneToOne(connection);
    //OneToMany(connection);
    //QueryMultiple(connection);
    //SelectIn(connection);
    Like(connection);
};

// QUERY
static void ListCategories(SqlConnection connection)
{
    var categories = connection.Query<Category>("Select [Id], [Title] FROM [Category]");
    foreach (var categ in categories)
    {
        Console.WriteLine($"{categ.Id} - {categ.Title}");
    }
}

// INSERT
static void UpdateCategory(SqlConnection connection) 
{
    var updateQuery = "UPDATE [Category] SET [Title] = @title WHERE [Id] = @id";
    var rows = connection.Execute(updateQuery, new
    {
        id = new Guid("af3407aa-11ae-4621-a2ef-2028b85507c4"),
        title = "Frontend 2023"
    });
    Console.WriteLine($"{rows} registros atualizados");
}
static void CreateCategory(SqlConnection connection)
{
    var category = new Category();    
    category.Title = "Amazon AWS 3";
    category.Url = "amazon 3";
    category.Description = "Categoria destinada a serviços nuvem aws 3";
    category.Order = 9;
    category.Featured = false;
    category.Summary = "AWS CLOUD 3";

    var insertSql = "INSERT INTO [Category] OUTPUT inserted.[Id] VALUES(NEWID(), @title, @url, @summary, @order, @description, @featured) SELECT SCOPE_IDENTITY()";

    var categoryId = connection.ExecuteScalar<Guid>(insertSql, new
    {        
        category.Title,
        category.Url,
        category.Summary,
        category.Order,
        category.Description,
        category.Featured
    });
    Console.WriteLine($"{categoryId} foi inserida em categoria!");    
}

static void CreateManyCategory(SqlConnection connection)
{
    var category = new Category();
    category.Id = Guid.NewGuid();
    category.Title = "Amazon AWS";
    category.Url = "amazon";
    category.Description = "Categoria destinada a serviços nuvem aws";
    category.Order = 8;
    category.Featured = false;
    category.Summary = "AWS CLOUD";

    var category2 = new Category();
    category2.Id = Guid.NewGuid();
    category2.Title = "Categoria Nova 2";
    category2.Url = "Categori 2";
    category2.Description = "Categoria NOVA destinada a serviços nuvem aws 2";
    category2.Order = 9;
    category2.Featured = false;
    category2.Summary = "Categoria NOVA 2";

    var insertSql = "INSERT INTO [Category] VALUES(@id, @title, @url, @summary, @order, @description, @featured)";

    var rows = connection.Execute(insertSql, new[] {
        new
        {            
            category.Title,
            category.Url,
            category.Summary,
            category.Order,
            category.Description,
            category.Featured
        },
        new
        {
            
            category2.Title,
            category2.Url,
            category2.Summary,
            category2.Order,
            category2.Description,
            category2.Featured
        }
        });
    Console.WriteLine($"{rows} linhas inserida!");
}

static void DeleteCategory(SqlConnection connection)
{
    var deleteQuery = "DELETE [Category] WHERE [Id]=@id";
    var rows = connection.Execute(deleteQuery, new
    {
        id = new Guid("ea8059a2-e679-4e74-99b5-e4f0b310fe6f"),
    });

    Console.WriteLine($"{rows} registros excluídos");
}

static void ExecuteProcedure(SqlConnection connection)
{
    var sqlProcedure = "[spDeleteStudent]";
    var pars = new { StudentId = "d93abf71-e7e8-4aca-b6a2-f3d7ee095f32" };

    var executeRows = connection.Execute(
        sqlProcedure,
        pars,
        commandType: CommandType.StoredProcedure
        );

    Console.WriteLine($"{executeRows} linhas afetadas");
}

static void ExecuteReadProcedure(SqlConnection connection)
{
    var sqlProcedure = "[spGetCoursesByCategory]";
    var pars = new { CategoryId = "af3407aa-11ae-4621-a2ef-2028b85507c4" };

    var courses = connection.Query(
        sqlProcedure,
        pars,
        commandType: CommandType.StoredProcedure
        );

    foreach (var item in courses)
    {
        Console.WriteLine(item.Id);
    }
}

static void ReadView(SqlConnection connection)
{
    var sql = "SELECT * FROM [vwCourses]";
    var courses = connection.Query(sql);

    foreach (var item in courses)
    {
        Console.WriteLine($"{item.Id} - {item.Title}");
    }
}

static void OneToOne(SqlConnection connection)
{
    var sql = "SELECT * FROM [CareerItem] Inner JOIN [Course] on [CareerItem].[CourseId] = [Course].[Id]";

    var items = connection.Query<CareerItem, Course, CareerItem>(sql, (careerItem, course) => 
    {
        careerItem.Course = course;
        return careerItem;
    }, splitOn: "Id");

    foreach (var item in items)
    {
        Console.WriteLine($"{item.Title} - Curso:{item.Course.Title}");
    }
}

static void OneToMany(SqlConnection connection)
{
    var sql = "SELECT [Career].[Id],[Career].[Title], " +
        "[CareerItem].[CareerId], [CareerItem].[Title]" +
        " FROM [Career] INNER JOIN [CareerItem] ON[CareerItem].[CareerId] = " +
        "[Career].[Id] ORDER BY [Career].[Title]";

    var careers = new List<Career>();

    var items = connection.Query<Career, CareerItem, Career>(sql, (career, item) => 
    {
        var car = careers.Where(x => x.Id == career.Id).FirstOrDefault();
        if (car == null)
        {
            car = career;
            car.Items.Add(item);
            careers.Add(car);
        }
        else
        {
            car.Items.Add(item);
        }

        return career;
    }, splitOn: "CareerId");

    foreach (var carrer in careers)
    {
        Console.WriteLine(carrer.Title);

        foreach (var item in carrer.Items)
        {
            Console.WriteLine(" - " + item.Title);
        }
    }
}

static void QueryMultiple(SqlConnection connection)
{
    var query = "SELECT * FROM [Category]; SELECT * FROM [Course];";

    using (var multi = connection.QueryMultiple(query))
    {
        var categories = multi.Read<Category>();
        var course = multi.Read<Course>();

        foreach (var item in categories)
        {
            Console.WriteLine(item.Title);

        }
        foreach (var item in course)
        {
            Console.WriteLine(item.Title);
        }
    }
}

static void SelectIn(SqlConnection connection)
{    
    var query = @"select * from Career where [Id] IN @id";
   
    var items = connection.Query<Career>(query, new
    {
        Id = new[]
        {
            "01ae8a85-b4e8-4194-a0f1-1c6190af54cb",
            "e6730d1c-6870-4df3-ae68-438624e04c72"
        }
    });

    foreach (var item in items)
    {
        Console.WriteLine(item.Title);
    }      

}

static void Like(SqlConnection connection)
{
    var term = "api";

    var query = @"select * from Course where [Title] Like @exp";
    var items = connection.Query<Course>(query, new
    {
        exp = $"%{term}"
    });

    foreach (var item in items)
    {
        Console.WriteLine(item.Title);
    }
}
static void Transaction(SqlConnection connection)
{
    var category = new Category();
    category.Title = "Amazon AWS 3";
    category.Url = "amazon 3";
    category.Description = "Categoria destinada a serviços nuvem aws 3";
    category.Order = 9;
    category.Featured = false;
    category.Summary = "AWS CLOUD 3";

    var insertSql = "INSERT INTO [Category] OUTPUT inserted.[Id] VALUES(NEWID(), @title, @url, @summary, @order, @description, @featured) SELECT SCOPE_IDENTITY()";

    using (var transaction = connection.BeginTransaction())
    {
        var categoryId = connection.ExecuteScalar<Guid>(insertSql, new
        {
            category.Title,
            category.Url,
            category.Summary,
            category.Order,
            category.Description,
            category.Featured
        }, transaction);

        transaction.Commit();
        //transaction.Rollback();

        Console.WriteLine($"{categoryId} foi inserida em categoria!");
    }    
}