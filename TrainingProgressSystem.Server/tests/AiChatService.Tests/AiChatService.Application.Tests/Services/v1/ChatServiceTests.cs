using AiChatService.Application.Services.v1;
using AiChatService.Domain.Entities;
using AiChatService.Domain.Interfaces;
using Moq;
using Shared.Abstractions.Auth;
using Shared.Kernal.Errors;
using Shared.Kernal.Results;

namespace AiChatService.Application.Tests.Services.v1;

[TestFixture]
public class ChatServiceTests
{
    private Mock<ILlmClient> _llmClient = null!;
    private Mock<IChatRepository> _chatRepository = null!;
    private Mock<ICurrentUser> _currentUser = null!;
    private ChatService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _llmClient = new Mock<ILlmClient>(MockBehavior.Strict);
        _chatRepository = new Mock<IChatRepository>(MockBehavior.Strict);
        _currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        _service = new ChatService(_llmClient.Object, _chatRepository.Object, _currentUser.Object);
    }

    [Test]
    public async Task SendMessageStreamAsync_WhenCurrentUserFails_YieldsUnauthorizedOnly()
    {
        _currentUser
            .Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Failure(new Error(ErrorCode.Unauthorized, "Unauthorized")));

        var chunks = await ToListAsync(_service.SendMessageStreamAsync("hello", CancellationToken.None));

        Assert.That(chunks, Is.EqualTo(new[] { "[Unauthorized]" }));
        _chatRepository.VerifyNoOtherCalls();
        _llmClient.VerifyNoOtherCalls();
    }

    [Test]
    public async Task SendMessageStreamAsync_WhenAuthorized_UsesHistoryStreamsChunksAndPersistsMessages()
    {
        var userId = Guid.NewGuid();
        var history = new List<ChatMessage>
        {
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = "assistant",
                Content = "existing answer",
                CreatedAt = new DateTime(2026, 4, 20, 10, 0, 0, DateTimeKind.Utc)
            }
        };

        List<(string Role, string Content)>? capturedMessages = null;
        var persistedMessages = new List<ChatMessage>();

        _currentUser
            .Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _chatRepository
            .Setup(repository => repository.GetHistoryAsync(userId, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        _chatRepository
            .Setup(repository => repository.AddAsync(It.IsAny<ChatMessage>(), It.IsAny<CancellationToken>()))
            .Callback<ChatMessage, CancellationToken>((message, _) => persistedMessages.Add(message))
            .Returns(Task.CompletedTask);

        _llmClient
            .Setup(client => client.StreamCompletionAsync(It.IsAny<IReadOnlyList<(string Role, string Content)>>(), It.IsAny<CancellationToken>()))
            .Returns<IReadOnlyList<(string Role, string Content)>, CancellationToken>((messages, _) =>
            {
                capturedMessages = messages.ToList();
                return CreateStream("chunk-1", "chunk-2");
            });

        var chunks = await ToListAsync(_service.SendMessageStreamAsync("hello", CancellationToken.None));

        Assert.That(chunks, Is.EqualTo(new[] { "chunk-1", "chunk-2" }));
        Assert.That(capturedMessages, Is.Not.Null);
        Assert.That(capturedMessages![0].Role, Is.EqualTo("system"));
        Assert.That(capturedMessages[1], Is.EqualTo(("assistant", "existing answer")));
        Assert.That(capturedMessages[2], Is.EqualTo(("user", "hello")));

        Assert.That(persistedMessages, Has.Count.EqualTo(2));
        Assert.That(persistedMessages[0].Role, Is.EqualTo("user"));
        Assert.That(persistedMessages[0].Content, Is.EqualTo("hello"));
        Assert.That(persistedMessages[1].Role, Is.EqualTo("assistant"));
        Assert.That(persistedMessages[1].Content, Is.EqualTo("chunk-1chunk-2"));
    }

    [Test]
    public async Task GetHistoryAsync_WhenCurrentUserFails_ReturnsFailure()
    {
        _currentUser
            .Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Failure(new Error(ErrorCode.Unauthorized, "Unauthorized")));

        var result = await _service.GetHistoryAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Unauthorized));
        _chatRepository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task GetHistoryAsync_WhenAuthorized_ReturnsRepositoryHistory()
    {
        var userId = Guid.NewGuid();
        IReadOnlyList<ChatMessage> history =
        [
            new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = "assistant",
                Content = "saved reply",
                CreatedAt = DateTime.UtcNow
            }
        ];

        _currentUser
            .Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _chatRepository
            .Setup(repository => repository.GetHistoryAsync(userId, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = await _service.GetHistoryAsync();

        Assert.That(result.IsFailure, Is.False);
        Assert.That(result.Value, Is.EqualTo(history));
    }

    [Test]
    public async Task ClearHistoryAsync_WhenCurrentUserFails_ReturnsFailure()
    {
        _currentUser
            .Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Failure(new Error(ErrorCode.Unauthorized, "Unauthorized")));

        var result = await _service.ClearHistoryAsync();

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error.Code, Is.EqualTo(ErrorCode.Unauthorized));
        _chatRepository.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ClearHistoryAsync_WhenAuthorized_ClearsHistoryAndReturnsSuccess()
    {
        var userId = Guid.NewGuid();

        _currentUser
            .Setup(user => user.GetCurrentUserId())
            .Returns(ResultOfT<Guid>.Success(userId));

        _chatRepository
            .Setup(repository => repository.ClearHistoryAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ClearHistoryAsync();

        Assert.That(result.IsFailure, Is.False);
        _chatRepository.Verify(repository => repository.ClearHistoryAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    private static async Task<List<string>> ToListAsync(IAsyncEnumerable<string> source)
    {
        var items = new List<string>();
        await foreach (var item in source)
        {
            items.Add(item);
        }

        return items;
    }

    private static async IAsyncEnumerable<string> CreateStream(params string[] chunks)
    {
        foreach (var chunk in chunks)
        {
            yield return chunk;
            await Task.Yield();
        }
    }
}
