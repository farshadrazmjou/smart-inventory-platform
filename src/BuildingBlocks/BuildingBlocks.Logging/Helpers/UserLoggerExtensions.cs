// using BuildingBlocks.Context.Interfaces;
// using BuildingBlocks.Logging.Constants;
// using Microsoft.Extensions.Logging;

// public static class UserLoggerExtensions
// {
//     public static void UserLoggedIn(this ILogger logger, IRequestContext context)
//     {
//         logger.LogInformation(LogMessages.UserLoggedIn, context.User.Username);
//     }

//     public static void UserLoginFailed(this ILogger logger, string username)
//     {
//         logger.LogWarning(LogMessages.UserLoginFailed, username);
//     }

//     public static void UserRegistered(this ILogger logger, IRequestContext context)
//     {
//         logger.LogInformation(LogMessages.UserRegistered, context.User.Username);
//     }
// }