namespace EuroTrail.Services
{
    public class Session
    {
        public string? Username { get; private set; }

        private Session() { }

        private static Session? currentInstance;

        public static Session GetInstance()
        {
            if (currentInstance == null)
            {
                currentInstance = new Session();
            }
            return currentInstance;
        }

        public void LogIn(string username)
        {
            Username = username;
        }

        public void LogOut()
        {
            Username = null;
        }

        public bool IsLoggedIn()
        {            
            return !string.IsNullOrEmpty(Username);
        }
    }
}
