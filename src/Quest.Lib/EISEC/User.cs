using System;

namespace Quest.Lib.EISEC
{
    [Serializable]
    public struct User
    {
        public string Username { get; set; }

        public string Password { get; set; }

        public DateTime Datechanged { get; set; }
    }
}