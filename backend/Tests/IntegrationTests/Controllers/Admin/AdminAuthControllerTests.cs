using System.Net;
 using System.Net.Http.Json;
 using backend.DTOs.Auth;
 using IntegrationTests.Helpers;
 using Microsoft.AspNetCore.Mvc.Testing;
 
 namespace IntegrationTests.Controllers.Admin;
 
 public class AdminAuthControllerTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
 {
     private readonly TestWebApplicationFactory _factory;
 
     public AdminAuthControllerTests(TestWebApplicationFactory factory)
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
                 HandleCookies = true,
                 AllowAutoRedirect = false
             });
     }
     
     private static string GetCookieValue(IEnumerable<string> cookies, string name)
     {
         var cookie = cookies.Single(x => x.StartsWith($"{name}="));
         return cookie.Split(';')[0].Substring(name.Length + 1);
     }
     
     [Fact]
     public async Task Login_WithValidEmployeeCredentials_ReturnsNoContentAndSetsCookies()
     {
         var client = CreateClient();

         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => EmployeeUserSeeder.SeedEmployeeUser(context)
         );
         
         var dto = new LoginDto
         {
             Email = "jane@example.com",
             Password = "password1234",
         };
         
         var response = await client.PostAsJsonAsync("/admin/auth/login", dto);
         
         Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
         Assert.True(response.Headers.TryGetValues("Set-Cookie", out var cookies));
 
         var setCookies = cookies.ToList();
         
         Assert.Contains(setCookies, x => x.StartsWith("admin_access_token="));
         Assert.Contains(setCookies, x => x.StartsWith("admin_refresh_token="));
     }
 
     [Fact]
     public async Task Login_WithInvalidAdminCredentials_ReturnsUnauthorized()
     {
         var client = CreateClient();

         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => EmployeeUserSeeder.SeedEmployeeUser(context)
         );
         
         var dto = new LoginDto
         {
             Email = "worngemail@example.net",
             Password = "worngpass",
         };
         
         var response = await client.PostAsJsonAsync("/admin/auth/login", dto);
         
         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
     }
     
     [Fact]
     public async Task Refresh_WithValidRefreshToken_ReturnsNoContentAndSetsCookies()
     {
         var client = CreateClient();

         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => EmployeeUserSeeder.SeedEmployeeUser(context)
         );
         
         var dto = new LoginDto
         {
             Email = "jane@example.com",
             Password = "password1234",
         };
         
         var loginResponse = await client.PostAsJsonAsync("/admin/auth/login", dto);
         
         Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
         Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
         
         var setCookies = cookies.ToList();
         Assert.Contains(setCookies, x => x.StartsWith("admin_access_token="));
         Assert.Contains(setCookies, x => x.StartsWith("admin_refresh_token="));
         
         var oldAccessToken = GetCookieValue(setCookies, "admin_access_token");
         var oldRefreshToken = GetCookieValue(setCookies, "admin_refresh_token");
         
         var refreshResponse = await client.PostAsync("/admin/auth/refresh", null);
         
         Assert.Equal(HttpStatusCode.NoContent, refreshResponse.StatusCode);
         Assert.True(refreshResponse.Headers.TryGetValues("Set-Cookie", out var cookies2));
         
         var setCookies2 = cookies2.ToList();
         
         Assert.Contains(setCookies2, x => x.StartsWith("admin_access_token="));
         Assert.Contains(setCookies2, x => x.StartsWith("admin_refresh_token="));
         
         var newAccessToken = GetCookieValue(setCookies2, "admin_access_token");
         var newRefreshToken = GetCookieValue(setCookies2, "admin_refresh_token");
         
         Assert.NotEqual(oldAccessToken, newAccessToken);
         Assert.NotEqual(oldRefreshToken, newRefreshToken);
     }

     [Fact]
     public async Task Refresh_WithMissingRefreshToken_ReturnsUnauthorized()
     {
         var client = CreateClient();
         
         var response = await client.PostAsync("/admin/auth/refresh", null);
         
         Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
     }
     
     [Fact]
     public async Task Logout_WithLoggedInUser_ReturnsNoContentAndClearsCookies()
     {
         var client = CreateClient();

         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => EmployeeUserSeeder.SeedEmployeeUser(context)
         );
         
         var dto = new LoginDto
         {
             Email = "jane@example.com",
             Password = "password1234",
         };
         
         var loginResponse = await client.PostAsJsonAsync("/admin/auth/login", dto);
         
         Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);
         Assert.True(loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies));
         
         var setCookies = cookies.ToList();
         
         Assert.Contains(setCookies, x => x.StartsWith("admin_access_token="));
         Assert.Contains(setCookies, x => x.StartsWith("admin_refresh_token="));
         
         var logoutResponse = await client.PostAsync("/admin/auth/logout", null);
         
         Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
         
         Assert.True(logoutResponse.Headers.TryGetValues("Set-Cookie", out var cookies2));
         
         var setCookies2 = cookies2.ToList();
         
         Assert.Contains(setCookies2, x => x.StartsWith("admin_access_token="));
         Assert.Contains(setCookies2, x => x.StartsWith("admin_refresh_token="));
         
         var accessToken = GetCookieValue(setCookies2, "admin_access_token");
         var refreshToken = GetCookieValue(setCookies2, "admin_refresh_token");
         
         Assert.Equal("", accessToken);
         Assert.Equal("", refreshToken);
     }

     [Fact]
     public async Task Me_WithLoggedInUser_ReturnsOk()
     {
         var client = CreateClient();

         await DatabaseHelper.ExecuteAsync(
             _factory,
             context => EmployeeUserSeeder.SeedEmployeeUser(context)
         );

         var dto = new LoginDto
         {
             Email = "jane@example.com",
             Password = "password1234",
         };

         var loginResponse = await client.PostAsJsonAsync("/admin/auth/login", dto);

         Assert.Equal(HttpStatusCode.NoContent, loginResponse.StatusCode);

         var meResponse = await client.GetAsync("/admin/auth/me");
         
         Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
     }
 }