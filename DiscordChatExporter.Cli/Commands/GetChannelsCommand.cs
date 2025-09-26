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

    [CommandOption("include-vc", Description = "Include voice channels.")]
    public bool IncludeVoiceChannels { get; init; } = true;

    [CommandOption(
        "include-threads",
        Description = "Which types of threads should be included.",
        Converter = typeof(ThreadInclusionModeBindingConverter)
    )]
    public ThreadInclusionMode ThreadInclusionMode { get; init; } = ThreadInclusionMode.None;

    [CommandOption(
        "preserve-order",
        Description = "Show channels as they are sorted in Discord instead of alphabetically."
    )]
    public bool PreserveOrder { get; init; } = false;

    [CommandOption(
        "include-categories",
        Description = "Include categories and their IDs in the output."
    )]
    public bool IncludeCategories { get; init; } = false;

    public override async ValueTask ExecuteAsync(IConsole console)
    {
        await base.ExecuteAsync(console);

        var cancellationToken = console.RegisterCancellationHandler();

        // Fetch channels once
        var allChannels = await Discord.GetGuildChannelsAsync(GuildId, cancellationToken);

        var categories = allChannels.Where(c => c.IsCategory).OrderBy(c => c.Position).ToArray();

        // Split channel filtering into two parts, the common one and the one that depends on `PreserveOrder`
        var query = allChannels
            .Where(c => !c.IsCategory)
            .Where(c => IncludeVoiceChannels || !c.IsVoice)
            .OrderBy(c => c.Parent?.Position);

        var channels = (
            PreserveOrder ? query.ThenBy(c => c.Position) : query.ThenBy(c => c.Name)
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
                        cancellationToken
                    )
                )
                    .Pipe(q => PreserveOrder ? q.OrderBy(c => c.Id) : q.OrderBy(c => c.Name))
                    .ToArray()
                : [];

        // Print channels
        if (IncludeCategories)
        {
            foreach (var category in categories)
            {
                // Category ID
                await console.Output.WriteAsync(category.Id.ToString());

                // Separator - uses \ instead of | to differentiate between categories and channels
                using (console.WithForegroundColor(ConsoleColor.DarkGray))
                    await console.Output.WriteAsync(" \\ ");

                // Category name
                using (console.WithForegroundColor(ConsoleColor.White))
                    await console.Output.WriteLineAsync($"{category.Name}");

                var categoryChannels = channels.Where(c => c.Parent?.Id == category.Id).ToArray();

                foreach (var channel in categoryChannels)
                {
                    // Even if channel 'indent' is now set to the default value,
                    // I'm leaving the variable here for readability and ease of editing if desired
                    var indent = "";
                    await PrintChannelAndThreads(
                        console,
                        channel,
                        channelIdMaxLength,
                        threads,
                        indent
                    );
                }
            }
        }
        else
        {
            foreach (var channel in channels)
            {
                // Prints the results as usual
                await PrintChannelAndThreads(console, channel, channelIdMaxLength, threads);
            }
        }
    }

    // Separated this logic to avoid code duplication
    private static async Task PrintChannelAndThreads(
        IConsole console,
        Core.Discord.Data.Channel channel,
        int channelIdMaxLength,
        Core.Discord.Data.Channel[] threads,
        string indent = ""
    )
    {
        // Indent
        await console.Output.WriteAsync(indent);

        // Channel ID
        await console.Output.WriteAsync(channel.Id.ToString().PadRight(channelIdMaxLength, ' '));

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
            await console.Output.WriteAsync(indent + " * ");

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
