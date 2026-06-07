namespace ECommerecAPI.Services
{
    // Only do this if you have special repeated logic
    public class Logging
    {
        private readonly ILogger<Logging> _logger;

        public Logging(ILogger<Logging> logger)
        {
            _logger = logger;
        }

        // Example: you want to always log user + timestamp together
        public void LogUserAction(string email, string action)
        {
            _logger.LogInformation("[{Time}] User {Email} performed {Action}",
                DateTime.UtcNow, email, action);
        }

        public void LogFailedLogin(string email)
        {
            _logger.LogWarning("Failed login attempt for {Email} at {Time}",
                email, DateTime.UtcNow);
        }
    }
}
