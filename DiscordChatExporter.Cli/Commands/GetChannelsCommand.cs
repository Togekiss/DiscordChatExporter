using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx.Binding;
using CliFx.Infrastructure;
using DiscordChatExporter.Cli.Commands.Base;
using DiscordChatExporter.Cli.Commands.Converters;
using DiscordChatExporter.Cli.Commands.Shared;
using DiscordChatExporter.Core.Discord;
using PowerKit.Extensions;

namespace DiscordChatExporter.Cli.Commands;

[Command("channels", Description = "Get the list of channels in a server.")]
public partial class GetChannelsCommand : DiscordCommandBase
{
    [CommandOption("guild", 'g', Description = "Server ID.")]
    public required Snowflake GuildId { get; set; }

    [CommandOption("show-positions", Description = "Show position numbers in the channel list.")]
    public bool ShowPositions { get; set; } = false;

    [CommandOption(
        "relative-positions",
        Description = "Sort channels in the order they appear in Discord, and reset positions for each category."
    )]
    public bool RelativePositions { get; set; } = false;

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; set; } = true;

    [CommandOption(
        "include-threads",
        Description = "Which types of threads should be included.",
        Converter = typeof(ThreadInclusionModeInputConverter)
    )]
    public ThreadInclusionMode ThreadInclusionMode { get; set; } = ThreadInclusionMode.None;

    [CommandOption(
        "include-categories",
        Description = "Include categories and their IDs in the output."
    )]
    public bool IncludeCategories { get; set; } = false;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        // Get list of channels
        var unsortedChannels = await Discord.GetGuildChannelsAsync(
            GuildId,
            RelativePositions,
            cancellationToken
        );

        var categories = unsortedChannels
            .Where(c => c.IsCategory)
            .OrderBy(c => c.Position)
            .ToArray();

        // We have to split the query in two parts, this is the shared one
        var sortedChannels = unsortedChannels
            .Where(c => !c.IsCategory)
            .Where(c => IncludeVoiceChannels || !c.IsVoice)
            .OrderBy(c => c.Parent?.Position);

        // Sort by position if --relative-positions, else sort by name as usual
        var channels = (
            RelativePositions
                ? sortedChannels.ThenBy(c => c.Position)
                : sortedChannels.ThenBy(c => c.Name)
        ).ToArray();

        var channelIdMaxLength = channels
            .Select(c => c.Id.ToString().Length)
            .OrderDescending()
            .FirstOrDefault();

        var maxChannelPositionLength = channels
            .Select(c => c.Position?.ToString().Length ?? 0)
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

        // Extra intent for channels and threads when categories are included
        var indent = IncludeCategories ? "   " : "";

        foreach (var category in categories)
        {
            if (IncludeCategories)
            {
                // Category ID
                await console.Output.WriteAsync(
                    category.Id.ToString().PadRight(channelIdMaxLength, ' ')
                );

                // Separator - uses \ instead of | to differentiate between categories and channels
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" \\ ");

                if (ShowPositions)
                {
                    // Category position
                    using (console.WithForegroundColor(ConsoleColor.DarkGray))
                        await console.Output.WriteAsync(
                            (category.Position.ToString() + "#").PadRight(
                                maxChannelPositionLength + 2,
                                ' '
                            )
                        );
                }

                // Category name
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteLineAsync($"{category.Name}");
            }

            var categoryChannels = channels.Where(c => c.Parent?.Id == category.Id).ToArray();

            foreach (var channel in categoryChannels)
            {
                // Indent
                await console.Output.WriteAsync(indent);

                // Channel ID
                await console.Output.WriteAsync(
                    channel.Id.ToString().PadRight(channelIdMaxLength, ' ')
                );

                // Separator
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" | ");

                if (ShowPositions)
                {
                    // Channel position
                    using (console.WithForegroundColor(ConsoleColor.DarkGray))
                        await console.Output.WriteAsync(
                            (channel.Position.ToString() + "#").PadRight(
                                maxChannelPositionLength + 2,
                                ' '
                            )
                        );
                }

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
                    await console.Output.WriteAsync(indent + " * ");

                    // Thread ID
                    await console.Output.WriteAsync(
                        channelThread.Id.ToString().PadRight(channelThreadIdMaxLength, ' ')
                    );

                    // Separator
                    using (console.WithForegroundColor(ConsoleColor.DarkGray))
                        await console.Output.WriteAsync(" | ");

                    if (ShowPositions)
                    {
                        // Thread position
                        using (console.WithForegroundColor(ConsoleColor.DarkGray))
                            await console.Output.WriteAsync(
                                (channelThread.Position.ToString() + "#").PadRight(
                                    maxChannelPositionLength + 2,
                                    ' '
                                )
                            );
                    }

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
}
