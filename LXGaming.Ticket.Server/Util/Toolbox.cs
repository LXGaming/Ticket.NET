using System;

namespace LXGaming.Ticket.Server.Util {

    public static class Toolbox {

        public static bool ParseIdentifier(string identifier, out string key, out string value) {
            var strings = identifier?.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            if (strings?.Length == 2) {
                key = strings[0];
                value = strings[1];
                return true;
            }

            key = null;
            value = null;
            return false;
        }
    }
}