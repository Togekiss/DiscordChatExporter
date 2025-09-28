using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Attributes;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Core.Discord;
using DiscordChatExporter.Core.Utils.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("channels", Description = "Get the list of channels in a server.")]
public class GetChannelsCommand : DiscordCommandBase
{
    [CommandOption("guild", 'g', Description = "Server ID.")]
    public required Snowflake GuildId { get; init; }

    [CommandOption(
        "relative-positions",
        Description = "Sort channels in the order they appear in Discord."
    )]
    public bool RelativePositions { get; init; } = false;

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; init; } = true;

    [CommandOption(
        "include-threads",
        Description = "Which types of threads should be included.",
        Converter = typeof(ThreadInclusionModeBindingConverter)
    )]
    public ThreadInclusionMode ThreadInclusionMode { get; init; } = ThreadInclusionMode.None;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        // Improve compatibility with feat/include-categories
        var allChannels = await Discord.GetGuildChannelsAsync(GuildId, RelativePositions, cancellationToken);

        // We have to split the query in two parts:
        var query = allChannels
            .Where(c => !c.IsCategory)
            .Where(c => IncludeVoiceChannels || !c.IsVoice)
            .OrderBy(c => c.Parent?.Position);

        // Sort by position if --relative-positions, else sort by name as usual
        var channels = (
            RelativePositions ? query.ThenBy(c => c.Position) : query.OrderBy(c => c.Name)
        ).ToArray();

        var channelIdMaxLength = channels
            .Select(c => c.Id.ToString().Length)
            .OrderDescending()
            .FirstOrDefault();

        var threads =
            ThreadInclusionMode != ThreadInclusionMode.None
                ? (
                    await Discord.GetGuildThreadsAsync(
                        GuildId,
                        ThreadInclusionMode == ThreadInclusionMode.All,
                        null,
                        null,
                        RelativePositions,
                        cancellationToken
                    )
                )
                    .Pipe(q =>
                        RelativePositions ? q.OrderBy(t => t.Position) : q.OrderBy(t => t.Name)
                    )
                    .ToArray()
                : [];

        foreach (var channel in channels)
        {
            // Channel ID
            await console.Output.WriteAsync(
                channel.Id.ToString().PadRight(channelIdMaxLength, ' ')
            );

            // Separator
            using (console.WithForegroundColor(ConsoleColor.DarkGray))
                await console.Output.WriteAsync(" | ");

            // Channel name
            using (console.WithForegroundColor(ConsoleColor.White))
                await console.Output.WriteLineAsync(channel.GetHierarchicalName());

            var channelThreads = threads.Where(t => t.Parent?.Id == channel.Id).ToArray();
            var channelThreadIdMaxLength = channelThreads
                .Select(t => t.Id.ToString().Length)
                .OrderDescending()
                .FirstOrDefault();

            foreach (var channelThread in channelThreads)
            {
                // Indent
                await console.Output.WriteAsync(" * ");

                // Thread ID
                await console.Output.WriteAsync(
                    channelThread.Id.ToString().PadRight(channelThreadIdMaxLength, ' ')
                );

                // Separator
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" | ");

                // Thread name
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteAsync($"Thread / {channelThread.Name}");

                // Separator
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" | ");

                // Thread status
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteLineAsync(
                        channelThread.IsArchived ? "Archived" : "Active"
                    );
            }
        }
    }
}
