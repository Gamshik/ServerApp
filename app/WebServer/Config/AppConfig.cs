using DbAccess.Entities;
using DbAccess.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace WebServer.Config
{
    public static class AppConfig
    {
        private static string GetCarsTable(IEnumerable<Car> cars)
        {
            var carsHtmlTable = new StringBuilder("");

            carsHtmlTable.Append("<table border=1>");
            carsHtmlTable.Append("<td>Id</td>");
            carsHtmlTable.Append("<td>Brand</td>");
            carsHtmlTable.Append("<td>LoadCapacity</td>");
            carsHtmlTable.Append("<td>RegistrationNumber</td>");
            foreach (var car in cars)
            {
                carsHtmlTable.Append("<tr>");
                carsHtmlTable.Append("<td>" + car.Id + "</td>");
                carsHtmlTable.Append("<td>" + car.Brand + "</td>");
                carsHtmlTable.Append("<td>" + car.LoadCapacity + "</td>");
                carsHtmlTable.Append("<td>" + car.RegistrationNumber + "</td>");
                carsHtmlTable.Append("</tr>");
            }
            carsHtmlTable.Append("</table>");

            return carsHtmlTable.ToString();
        }
        private static string GetCargosTable(IEnumerable<Cargo> cargos)
        {
            var carsHtmlTable = new StringBuilder("");

            carsHtmlTable.Append("<table border=1>");
            carsHtmlTable.Append("<td>Id</td>");
            carsHtmlTable.Append("<td>Title</td>");
            carsHtmlTable.Append("<td>Weight</td>");
            carsHtmlTable.Append("<td>RegistrationNumber</td>");
            foreach (var car in cargos)
            {
                carsHtmlTable.Append("<tr>");
                carsHtmlTable.Append("<td>" + car.Id + "</td>");
                carsHtmlTable.Append("<td>" + car.Title + "</td>");
                carsHtmlTable.Append("<td>" + car.Weight + "</td>");
                carsHtmlTable.Append("<td>" + car.RegistrationNumber + "</td>");
                carsHtmlTable.Append("</tr>");
            }
            carsHtmlTable.Append("</table>");

            return carsHtmlTable.ToString();
        }
        public static void DbTablesCacheMiddleware(this IApplicationBuilder app, DbService dbService, IMemoryCache cache)
        {
            app.Use(async (context, next) =>
            {
                var tablesName = dbService.GetTablesName();

                var requestedTableName = tablesName.FirstOrDefault(name => context.Request.Path.StartsWithSegments($"/{name}"));

                if (!string.IsNullOrEmpty(requestedTableName))
                {
                    string cacheKey = $"table_{requestedTableName}";

                    if (!cache.TryGetValue(cacheKey, out IEnumerable<object>? tableData) || tableData == null)
                    {
                        var n = 14;
                        var cacheExpiration = TimeSpan.FromSeconds(2 * n + 240);

                        tableData = dbService.GetEntriesByTableName(requestedTableName)?.Take(20);

                        cache.Set(cacheKey, tableData, cacheExpiration);

                        return;
                    }

                    var entries = new StringBuilder("");

                    foreach (var entry in tableData) entries.Append($"{entry.ToString()}\n");

                    await context.Response.WriteAsync($"Data for table {requestedTableName}:\n" + entries);
                    return;
                }

                await next.Invoke(context);
            });
        }
        public static void InfoAboutClientRoute(this IApplicationBuilder app)
        {
            app.Map("/info", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    var firstName = context.Request.Query["firstname"];
                    var lastName = context.Request.Query["lastname"];
                    string response = "<html><body><form action = / >" +
                    "<br>First name:<br>" + firstName +
                    "<br>Last name:<br>" + lastName +
                    "<br>Client connection id:<br>" + context.Connection.Id +
                    "<br>Client IP:<br>" + context.Connection.RemoteIpAddress +
                    "<br>Client local port:<br>" + context.Connection.LocalPort +
                    "</body></html>";

                    await context.Response.WriteAsync(response);
                });
            });
        }
        public static void SearchFormsRoutes(this IApplicationBuilder app, DbService dbService)
        {
            app.Map("/searchform1", appBuilder =>
              {
                  appBuilder.Run(async context =>
                  {
                      const string OPTION_QUERY_NAME = "car_option";
                      const string VALUE_QUERY_NAME = "car_value";

                      const string CAR_BRAND_OPTION_NAME = "car-brand";
                      const string CAR_REGISTER_NUMBER_OPTION_NAME = "car-register-number";

                      var option = context.Request.Query[OPTION_QUERY_NAME];

                      var requestedValue = context.Request.Query[VALUE_QUERY_NAME];

                      var foundCarsHtmlTable = GetCarsTable([]);

                      var cookieOption = "";
                      var cookieValue = "";

                      if (!string.IsNullOrEmpty(option))
                      {
                          context.Response.Cookies.Append(OPTION_QUERY_NAME, option.ToString());
                          context.Response.Cookies.Append(VALUE_QUERY_NAME, requestedValue.ToString());

                          switch (option.ToString().ToLower())
                          {
                              case (CAR_BRAND_OPTION_NAME):
                                  var foundCarsByBrand = dbService.FindCars(c => c.Brand == requestedValue);

                                  if (foundCarsByBrand == null) break;

                                  foundCarsHtmlTable = GetCarsTable(foundCarsByBrand);
                                  break;

                              case (CAR_REGISTER_NUMBER_OPTION_NAME):
                                  var foundCarsByRegisterNumber = dbService.FindCars(c => c.RegistrationNumber == requestedValue);

                                  if (foundCarsByRegisterNumber == null) break;

                                  foundCarsHtmlTable = GetCarsTable(foundCarsByRegisterNumber);
                                  break;
                          }
                      }
                      else context.Request.Cookies.TryGetValue(OPTION_QUERY_NAME, out cookieOption);

                      if (string.IsNullOrEmpty(requestedValue)) context.Request.Cookies.TryGetValue(VALUE_QUERY_NAME, out cookieValue);

                      var isBrandSelected = (!string.IsNullOrEmpty(cookieOption) && cookieOption == CAR_BRAND_OPTION_NAME) || (!string.IsNullOrEmpty(option) && option == CAR_BRAND_OPTION_NAME);
                      var isRegistrationNumberSelected = (!string.IsNullOrEmpty(cookieOption) && cookieOption == CAR_REGISTER_NUMBER_OPTION_NAME) || (!string.IsNullOrEmpty(option) && option == CAR_REGISTER_NUMBER_OPTION_NAME);

                      var isHaveRequestedValue = !string.IsNullOrEmpty(requestedValue);

                      await context.Response.WriteAsync(
                          @$"<html><body>
                        <h1>Search cars</h1>
                        <form action='/searchform1' method='get'>
                            <input type='text' name='{VALUE_QUERY_NAME}' placeholder='Search' value='" + (isHaveRequestedValue ? requestedValue : cookieValue) + @$"'>
                            <select name='{OPTION_QUERY_NAME}'>
                                <option value='{CAR_BRAND_OPTION_NAME}' "
                                + (isBrandSelected ? "selected" : "")
                                + @$">Brand</option>
                                <option value='{CAR_REGISTER_NUMBER_OPTION_NAME}' "
                                + (isRegistrationNumberSelected ? "selected" : "")
                                + @$">Registration number</option>
                            </select>
                            <button type='submit'>Search</button>
                        </form>
                        <br>
                        <h2>Found cars</h2>
                        {foundCarsHtmlTable}
                        </body></html>");
                  });
              });

            app.Map("/searchform2", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    const string OPTION_QUERY_NAME = "cargo_option";
                    const string VALUE_QUERY_NAME = "cargo_value";

                    const string CARGO_TITLE_OPTION_NAME = "cargo-brand";
                    const string CARGO_REGISTER_NUMBER_OPTION_NAME = "cargo-register-number";

                    var option = context.Request.Query[OPTION_QUERY_NAME];

                    var foundCargosHtmlTable = GetCargosTable([]);

                    var requestedValue = context.Request.Query[VALUE_QUERY_NAME];

                    var sessionOption = "";
                    var sessionValue = "";

                    if (!string.IsNullOrEmpty(option))
                    {
                        context.Session.SetString(OPTION_QUERY_NAME, option.ToString());
                        context.Session.SetString(VALUE_QUERY_NAME, requestedValue.ToString());

                        switch (option.ToString().ToLower())
                        {
                            case (CARGO_TITLE_OPTION_NAME):
                                var foundCargosByTitle = dbService.FindCargos(c => c.Title == requestedValue);

                                if (foundCargosByTitle == null) break;

                                foundCargosHtmlTable = GetCargosTable(foundCargosByTitle);
                                break;

                            case (CARGO_REGISTER_NUMBER_OPTION_NAME):
                                var foundCargosByRegisterNumber = dbService.FindCargos(c => c.RegistrationNumber == requestedValue);

                                if (foundCargosByRegisterNumber == null) break;

                                foundCargosHtmlTable = GetCargosTable(foundCargosByRegisterNumber);
                                break;
                        }
                    }
                    else sessionOption = context.Session.GetString(OPTION_QUERY_NAME);

                    if (string.IsNullOrEmpty(requestedValue)) sessionValue = context.Session.GetString(VALUE_QUERY_NAME);

                    var isBrandSelected = (!string.IsNullOrEmpty(sessionOption) && sessionOption == CARGO_TITLE_OPTION_NAME) || (!string.IsNullOrEmpty(option) && option == CARGO_TITLE_OPTION_NAME);
                    var isRegistrationNumberSelected = (!string.IsNullOrEmpty(sessionOption) && sessionOption == CARGO_REGISTER_NUMBER_OPTION_NAME) || (!string.IsNullOrEmpty(option) && option == CARGO_REGISTER_NUMBER_OPTION_NAME);

                    var isHaveRequestedValue = !string.IsNullOrEmpty(requestedValue);

                    await context.Response.WriteAsync(
                        @$"<html><body>
                        <h1>Search cargos</h1>
                        <form action='/searchform2' method='get'>
                            <input type='text' name='{VALUE_QUERY_NAME}' placeholder='Search' value='" + (isHaveRequestedValue ? requestedValue : sessionValue) + @$"'>
                            <select name='{OPTION_QUERY_NAME}'>
                                <option value='{CARGO_TITLE_OPTION_NAME}' "
                              + (isBrandSelected ? "selected" : "")
                              + @$">Title</option>
                                <option value='{CARGO_REGISTER_NUMBER_OPTION_NAME}' "
                              + (isRegistrationNumberSelected ? "selected" : "")
                              + @$">Registration number</option>
                            </select>
                            <button type='submit'>Search</button>
                        </form>
                        <br>
                        <h2>Found cargos</h2>
                        {foundCargosHtmlTable}
                        </body></html>");
                });
            });
        }
    }
}
