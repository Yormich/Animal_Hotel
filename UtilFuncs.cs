using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Animal_Hotel
{
    public static class UtilFuncs
    {
        public static string DateToHtmlFormatter(DateTime date)
        {
            date = date == default(DateTime) ? DateTime.Now : date;
            string year = date.Year.ToString();
            _ = year.PadLeft(4, '0');
            string month = date.Month.ToString().PadLeft(2, '0');
            string day = date.Day.ToString().PadLeft(2, '0');
            string htmlDate = $"{year}-{month}-{day}";
            return htmlDate;
        }

        public static string Sha256_Hash(string value)
        {
            var sb = new StringBuilder();
            using SHA256 hash = SHA256.Create();
            Byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
            return sb.GetString(result);
        }

        public static Task<Dictionary<string, (string Controller, string Display)>> CreateUserActionsList(string role,
            IMemoryCache cache)
        {
            return Task.Run(() =>
            {
                string key = $"{role}Actions";
                cache.TryGetValue(key, out Dictionary<string, (string, string)>? cachedActions);

                if (cachedActions == null)
                {
                    var controller = typeof(Controller);

                    //get controllers that inherit from Controller class
                    var controllerDescendants = Assembly.GetExecutingAssembly().GetTypes()
                        .Where(t => controller.IsAssignableFrom(t));

                    //for each descendant
                    Dictionary<string, (string, string)> actions = new();
                    foreach (var descendant in controllerDescendants)
                    {
                        var methods = descendant
                            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                            .Where(method =>
                            {
                                var authorize = method.GetCustomAttribute<AuthorizeAttribute>();
                                bool isActionMapped = method.GetCustomAttribute<ActionMapperAttribute>() != null;

                                return authorize != null && isActionMapped
                                    && (authorize.Roles?.Contains(role, StringComparison.CurrentCulture) ?? false);
                            });
                        foreach (var method in methods)
                        {
                            var toAction = method.GetCustomAttribute<ActionMapperAttribute>()!;
                            actions.Add(toAction.ActionName, (toAction.ControllerName, toAction.DisplayName));
                        }
                    }

                    cache.Set(key, actions, new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)));
                    return actions;
                }
                return cachedActions;

            });
        }
    }
}
