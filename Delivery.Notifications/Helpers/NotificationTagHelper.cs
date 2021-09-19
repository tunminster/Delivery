using System.Text.RegularExpressions;

namespace Delivery.Notifications.Helpers
{
    public static class NotificationTagHelper
    {
        public static string GetTag(string username)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            var name = username.Split('@');
            var tag = rgx.Replace(name[0], "");
            return tag;
        }
    }
}