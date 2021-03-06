using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stl.Fusion.Server;
using HBHplayground.Abstractions;
using Stl.Fusion.Authentication;

namespace HBHplayground.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController, JsonifyErrors]
    public class ChatController : ControllerBase, IChatService
    {
        private readonly IChatService _chat;
        private readonly ISessionResolver _sessionResolver;

        public ChatController(IChatService chat, ISessionResolver sessionResolver)
        {
            _chat = chat;
            _sessionResolver = sessionResolver;
        }

        // Commands

        [HttpPost("setUserName")]
        public Task<ChatUser> SetUserNameAsync([FromBody] IChatService.SetUserNameCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(_sessionResolver);
            return _chat.SetUserNameAsync(command, cancellationToken);
        }

        [HttpPost("postMessage")]
        public Task<ChatMessage> PostMessageAsync([FromBody] IChatService.PostMessageCommand command, CancellationToken cancellationToken = default)
        {
            command.UseDefaultSession(_sessionResolver);
            return _chat.PostMessageAsync(command, cancellationToken);
        }

        // Queries

        [HttpGet("getCurrentUser"), Publish]
        public Task<ChatUser?> GetCurrentUserAsync(Session? session, CancellationToken cancellationToken = default)
        {
            session ??= _sessionResolver.Session;
            return _chat.GetCurrentUserAsync(session, cancellationToken);
        }

        [HttpGet("getUserCount"), Publish]
        public Task<long> GetUserCountAsync(CancellationToken cancellationToken = default)
            => _chat.GetUserCountAsync(cancellationToken);

        [HttpGet("getActiveUserCount"), Publish]
        public Task<long> GetActiveUserCountAsync(CancellationToken cancellationToken = default)
            => _chat.GetActiveUserCountAsync(cancellationToken);

        [HttpGet("getUser"), Publish]
        public Task<ChatUser> GetUserAsync(long id, CancellationToken cancellationToken = default)
            => _chat.GetUserAsync(id, cancellationToken);

        [HttpGet("getChatTail"), Publish]
        public Task<ChatPage> GetChatTailAsync(int length, CancellationToken cancellationToken = default)
            => _chat.GetChatTailAsync(length, cancellationToken);

        [HttpGet("getChatPage"), Publish]
        public Task<ChatPage> GetChatPageAsync(long minMessageId, long maxMessageId, CancellationToken cancellationToken = default)
            => _chat.GetChatPageAsync(minMessageId, maxMessageId, cancellationToken);
    }
}
