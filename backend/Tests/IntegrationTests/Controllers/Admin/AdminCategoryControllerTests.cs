using System.Net;
 using System.Net.Http.Json;
 using backend.DTOs.Auth;
 using backend.DTOs.Categories;
 using IntegrationTests.Helpers;
 using Microsoft.AspNetCore.Mvc.Testing;
 
 namespace IntegrationTests.Controllers.Admin;
 
 public class AdminCategoryControllerTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
 {
     private readonly TestWebApplicationFactory _factory;
 
     public AdminCategoryControllerTests(TestWebApplicationFactory factory)
     {
         _factory = factory;
     }
 
     public Task InitializeAsync() => _factory.ResetDatabaseAsync();
     public Task DisposeAsync() => Task.CompletedTask;
 
     private HttpClient CreateClient()
     {
         return _factory.CreateClient(
             new WebApplicationFactoryClientOptions
             {
                 AllowAutoRedirect = false,
                 HandleCookies = true
             }
         );
     }
     
     private async Task EmployeeLogin(HttpClient client, bool isAdmin = false)
     {
         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => UserSeeder.SeedEmployeeUser(context, isAdmin: isAdmin)
         );
 
         var dto = new LoginDto()
         {
             Email = "jane@example.com",
             Password = "password1234"
         };
         
         await client.PostAsJsonAsync("/admin/auth/login", dto);
     }
     
     private async Task ClientLogin(HttpClient client)
     {
         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => UserSeeder.SeedClientUser(context)
         );
 
         var dto = new LoginDto
         {
             Email = "john@example.com",
             Password = "password1234"
         };
         
         await client.PostAsJsonAsync("auth/login", dto);
     }
     
     [Fact]
     public async Task CreateCategory_WithAdminEmployeeUserLoggedIn_ReturnsOk()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client, true);
 
         var dto = new CategoryCreateDto
         {
             CategoryName = "TestCategory"
         };
         
         var result = await client.PostAsJsonAsync("/admin/categories", dto);
         
         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
     }
     
     [Fact]
     public async Task CreateCategory_WithAdminEmployeeUserLoggedInAndDuplicateName_ReturnsConflict()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client, true);

         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var dto = new CategoryCreateDto
         {
             CategoryName = "TestCategory"
         };
         
         var result = await client.PostAsJsonAsync("/admin/categories", dto);
         
         Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
     }
 
     [Fact]
     public async Task CreateCategory_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client);
 
         var dto = new CategoryCreateDto
         {
             CategoryName = "TestCategory"
         };
         
         var result = await client.PostAsJsonAsync("/admin/categories", dto);
         
         Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
     }
 
     [Fact]
     public async Task CreateCategory_WithClientUserLoggedIn_ReturnsForbidden()
     {
         var client = CreateClient();
         
         await ClientLogin(client);
 
         var dto = new CategoryCreateDto
         {
             CategoryName = "TestCategory"
         };
         
         var result = await client.PostAsJsonAsync("/admin/categories", dto);
         
         Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
     }
 
     [Fact]
     public async Task CreateCategory_WithNoUserLoggedIn_ReturnsUnauthorized()
     {
         var client = CreateClient();
 
         var dto = new CategoryCreateDto
         {
             CategoryName = "TestCategory"
         };
 
         var result = await client.PostAsJsonAsync("/admin/categories", dto);
         
         Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategories_WithAdminEmployeeUserLoggedIn_ReturnsOk()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client, true);
         
         var result = await client.GetAsync("/admin/categories");
         
         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategories_WithNonAdminEmployeeUserLoggedIn_ReturnsOk()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client);
         
         var result = await client.GetAsync("/admin/categories");
         
         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategories_WithClientUserLoggedIn_ReturnsForbidden()
     {
         var client = CreateClient();
         
         await ClientLogin(client);
         
         var result = await client.GetAsync("/admin/categories");
         
         Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategories_WithNoUserLoggedIn_ReturnsUnauthorized()
     {
         var client = CreateClient();
         
         var result = await client.GetAsync("/admin/categories");
         
         Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategoryById_WithAdminEmployeeUserLoggedInAndValidId_ReturnsOk()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client, true);

         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}");
         
         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategoryById_WithNonAdminEmployeeUserLoggedInAndValidId_ReturnsOk()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client);

         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}");
         
         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
     }

     [Fact]
     public async Task GetCategoryById_WithValidUserLoggedInAndInvalidId_ReturnsNotFound()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client, true);
         
         var result = await client.GetAsync($"/admin/categories/{int.MaxValue}");
         
         Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategoryById_WithClientUserLoggedIn_ReturnsForbidden()
     {
         var client = CreateClient();
         
         await ClientLogin(client);

         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}");
         
         Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategoryById_WithNoUserLoggedIn_ReturnsUnauthorized()
     {
         var client = CreateClient();
         
         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}");
         
         Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
     }

     [Fact]
     public async Task GetCategoryItems_WithAdminEmployeeUserLoggedInAndValidId_ReturnsOk()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client, true);

         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}/items");
         
         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
     }

     [Fact]
     public async Task GetCategoryItems_WithNonAdminEmployeeUserLoggedInAndValidId_ReturnsOk()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client);

         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}/items");
         
         Assert.Equal(HttpStatusCode.OK, result.StatusCode);
     }

     [Fact]
     public async Task GetCategoryItems_WithValidUserLoggedInAndInvalidId_ReturnsNotFound()
     {
         var client = CreateClient();
         
         await EmployeeLogin(client, true);

         var result = await client.GetAsync($"/admin/categories/{int.MaxValue}/items");
         
         Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategoryItems_WithClientUserLoggedIn_ReturnsForbidden()
     {
         var client = CreateClient();
         
         await ClientLogin(client);

         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}/items");
         
         Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
     }
     
     [Fact]
     public async Task GetCategoryItems_WithNoUserLoggedIn_ReturnsUnauthorized()
     {
         var client = CreateClient();

         var category = await DatabaseHelper.ExecuteAsync(
             _factory,
             context => MenuSeeder.SeedCategory(context)
         );
         
         var result = await client.GetAsync($"/admin/categories/{category.CategoryId}/items");
         
         Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
     }
 }