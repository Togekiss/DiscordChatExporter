using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Discord.Data;

namespace DiscordChatExporter.Core.Exporting;

public class GuildMemberCache
{
    private readonly ConcurrentDictionary<Snowflake, Member?> _membersById = new();

    public bool TryGet(Snowflake userId, out Member? member) =>
        _membersById.TryGetValue(userId, out member);

    public async ValueTask<Member?> GetOrAddAsync(
        Snowflake userId,
        Func<Snowflake, CancellationToken, ValueTask<Member?>> factory,
        CancellationToken cancellationToken = default
    )
    {
        if (_membersById.TryGetValue(userId, out var cached))
            return cached;

        var member = await factory(userId, cancellationToken);
        _membersById[userId] = member;
        return member;
    }
}
