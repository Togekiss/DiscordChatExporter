# Using the CLI

## Guide

### Step 1

After extracting the `.zip` archive, open your preferred terminal.

### Step 2

Change the current directory to DCE's folder with `cd C:\path\to\DiscordChatExporter` (`cd /path/to/DiscordChatExporter` on **MacOS** and **Linux**), then press ENTER to run the command.

**Windows** users can quickly get the folder's path by clicking the address bar while inside the folder.

![Copy path from Explorer](https://i.imgur.com/XncnhC2.gif)

**macOS** users can press Command+Option+C (⌘⌥C) while inside the folder (or selecting it) to copy its path to the clipboard.

You can also drag and drop the folder on **every platform**.

![Drag and drop folder](https://i.imgur.com/sOpZQAb.gif)

### Step 3

Now we're ready to run the commands.

Type the following command in your terminal of choice, then press ENTER to run it. This will list all available subcommands and options.

```console
./DiscordChatExporter.Cli
```

> **Note**:
> On Windows, if you're using the default Command Prompt (`cmd`), omit the leading `./` at the start of the command.


> **Docker** users, please refer to the [Docker usage instructions](Docker.md).

## CLI commands

| Command                 | Description                                          |
|-------------------------|------------------------------------------------------|
| export                  | Exports a channel                                    |
| exportdm                | Exports all direct message channels                  |
| exportguild             | Exports all channels within the specified server     |
| exportall               | Exports all accessible channels                      |
| channels                | Outputs the list of channels in the given server     |
| dm                      | Outputs the list of direct message channels          |
| guilds                  | Outputs the list of accessible servers               |
| guide                   | Explains how to obtain token, server, and channel ID |

To use the commands, you'll need a token. For the instructions on how to get a token, please refer to [this page](Token-and-IDs.md), or run `./DiscordChatExporter.Cli guide`.

To get help with a specific command, run:

```console
./DiscordChatExporter.Cli command --help
```

For example, to figure out how to use the `export` command, run:

```console
./DiscordChatExporter.Cli export --help
```

## Selecting what to export

### Export channels or DMs individually

You can quickly export with DCE's default settings by using just `-t token` and `-c channelid`. Using a DM ID instead of a channel ID also works.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555
```

### Export more than one channel

You can export multiple channels at once by using `-c channelid1 channelid2 channelid3`, regardless of if they're channels, threads, or DMs.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 53556 53557
```

### Export an entire category

You can export all the channels in a category by using the category ID instead of a channel's ID with `-c categoryid`.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555
```

### Export an entire server

To export all channels in a specific server, use the `exportguild` command and provide the server ID through the `-g|--guild` option:

```console
./DiscordChatExporter.Cli exportguild -t "mfa.Ifrn" -g 21814
```

### Including threads in the exports

By default, threads are not included in exports, unless they are being exported directly with `-c threadid`. 
You can change this behavior by using `--include-threads active` to include only active threads, or `--include-threads all` to include both active and inactive threads.

```console
./DiscordChatExporter.Cli exportguild -t "mfa.Ifrn" -g 21814 --include-threads all
```

If this option is used with `export -c [channelid or categoryid]`, it will export the threads associated with the channel or category.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --include-threads all
```

### Excluding voice channels from exports

By default, voice channels are included in the export. You can change this behavior by using `--include-vc false`.

```console
./DiscordChatExporter.Cli exportguild -t "mfa.Ifrn" -g 21814 --include-vc false
```

### Export everything

To export all accessible channels, use the `exportall` command:

```console
./DiscordChatExporter.Cli exportall -t "mfa.Ifrn"
```

### Excluding voice channels, DMs, and/or servers

When using `exportall`, `exportguild` or `export -c categoryid`, voice channels are included by default. 
You can change this behavior by using `--include-vc false`.

```console
./DiscordChatExporter.Cli exportguild -t "mfa.Ifrn" -g 21814 --include-vc false
```

When using `exportall`, you can exclude DMs and servers by using `--include-dm false` and `--include-guilds false` respectively.

For example, this command would export all servers and ignore DMs:

```console
./DiscordChatExporter.Cli exportall -t "mfa.Ifrn" --include-dm false
```

## Configuring the export

### Changing the format

You can change the export format to `HtmlDark`, `HtmlLight`, `PlainText` `Json` or `Csv` with `-f format`. The default format is `HtmlDark`.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -f Json
```

### Changing the output filename

You can change the filename by using `-o name.ext`. e.g. for the `HTML` format:

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -o myserver.html
```

### Changing the output directory

You can change the export directory by using `-o` and providing a path that ends with a slash or does not have a file extension. 
If any of the folders in the path have a space in its name, escape them with quotes (").

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports"
```

### Changing the filename and output directory

You can change both the filename and export directory by using `-o directory\name.ext`. 
Note that the filename must have an extension, otherwise it will be considered a directory name. 
If any of the folders in the path have a space in its name, escape them with quotes (").

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\myserver.html"
```

### Generating the filename and output directory dynamically

You can use template tokens to generate the output file path based on the server and channel metadata.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\%G\%T\%C.html"
```

Assuming you are exporting a channel named `"my-channel"` in the `"Text channels"` category from a server called `"My server"`, you will get the following output file
path: `C:\Discord Exports\My server\Text channels\my-channel.html`

Here is the full list of supported template tokens:

- `%g` - server ID
- `%G` - server name
- `%t` or `%m` - category ID
- `%T` or `%M` - category name
- `%c` - channel ID
- `%C` - channel name
- `%p` - channel position
- `%P` or `%N` - category position
- `%a` - the "after" date
- `%b` - the "before" date
- `%d` - the current date
- `%%` - escapes `%`

### Setting a different output directory and/or filenames for threads

By default, threads will follow the same output path as all channels, either following the default or the path set with `-o`.

But note that if the channel being imported is a thread, some template tokens will resolve differently:

- `%m` - category ID
- `%M` - category name
- `%t` - parent channel ID
- `%T` - parent channel name
- `%c` - thread ID
- `%C` - thread name
- `%p` - thread position
- `%P` - parent channel position
- `%N` - category position

If you want to define a different output directory and/or filename template for threads, you can use `--threads-output` and provide a path.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\%G\%T\%C.html --threads-output "C:\Discord Exports\%G\%M\Threads\%C.html"
```


### Choosing global or relative positions for channels

By default, the 'position' value of channels reflects their global position inside the server, and threads have no position.
You can change this behavior by using `--relative-positions`. This will make channel positions start counting from '1' for each category, following the order they appear in Discord, and thread positions start counting from '1' for each channel, sorted by creation date.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\%T\%p#%C.html"

> Output: C:\Discord Exports\Art\23#art-general.html
          C:\Discord Exports\Art\24#digital-art.html
          C:\Discord Exports\Art\25#tradtional-art.html
```
```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -o "C:\Discord Exports\%T\%p#%C.html --relative-positions"

> Output: C:\Discord Exports\Art\1#art-general.html
          C:\Discord Exports\Art\2#digital-art.html
          C:\Discord Exports\Art\3#tradtional-art.html
```

**Note:** This option might not work correctly when exporting DMs, or when exporting threads with a User token.

### Partitioning

You can use partitioning to split files after a given number of messages or file size. 
For example, a channel with 36 messages set to be partitioned every 10 messages will output 4 files.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -p 10
```

A 45 MB channel set to be partitioned every 20 MB will output 3 files.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 -p 20mb
```

### Downloading assets

If this option is set, the export will include additional files such as user avatars, attached files, images, etc.
Only files that are referenced by the export are downloaded, which means that, for example, user avatars will not be downloaded when using the plain text (TXT) export format.
A folder containing the assets will be created along with the exported chat. They must be kept together.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --media
```

### Reusing assets

Previously downloaded assets can be reused to skip redundant downloads as long as the chat is always exported to the same folder. 
Using this option can speed up future exports. This option requires the `--media` option.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --media --reuse-media
```

### Changing the media directory

By default, the media directory is created alongside the exported chat. You can change this by using `--media-dir` and providing a path that ends with a slash. 
All of the exported media will be stored in this directory.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --media --media-dir "C:\Discord Media"
```

### Changing the date format

You can customize how dates are formatted in the exported files by using `--locale` and inserting one of Discord's locales. 
The default locale is `en-US`.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --locale "de-DE"
```

### Date ranges

#### Messages sent before a date
Use `--before` to export messages sent before the provided date. E.g. messages sent before September 18th, 2019:

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --before 2019-09-18
```

#### Messages sent after a date
Use `--after` to export messages sent after the provided date. E.g. messages sent after September 17th, 2019 11:34 PM:

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --after "2019-09-17 23:34"
```

#### Messages sent in a date range
Use `--before` and `--after` to export messages sent during the provided date range. E.g. messages sent between September 17th, 2019 11:34 PM and September 18th:

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --after "2019-09-17 23:34" --before "2019-09-18"
```

You can try different formats like `17-SEP-2019 11:34 PM` or even refine your ranges down to milliseconds `17-SEP-2019 23:45:30.6170`!
Don't forget to quote (") the date if it has spaces!

More info about .NET date formats [here](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings).

### Filtering messages

Use `--filter` to filter what messages are included in the export.

```console
./DiscordChatExporter.Cli export -t "mfa.Ifrn" -c 53555 --filter "from:Tyrrrz has:image"
```

These filters function mostly like Discord's search query syntax. More detailed documentation on message filter syntax can be found [here](https://github.com/Tyrrrz/DiscordChatExporter/blob/master/.docs/Message-filters.md).


## Getting information

### List channels in a server

To list the channels available in a specific server, use the `channels` command and provide the server ID through the `-g|--guild` option:

```console
./DiscordChatExporter.Cli channels -t "mfa.Ifrn" -g 21814
```

This command accepts more parameters:

- `--include-vc` - whether to include voice channels. Defaults to `true`.
- `--include-threads` - whether to include all or active threads. Defaults to `none`.
- `--include-categories` - whether to include categories and their IDs to the list. Defaults to `false`
- `--relative-positions` - sort channels like they would in Discord, instead of alphabetically. Defaults to `false`.


### List direct message channels

To list all DM channels accessible to the current account, use the `dm` command:

```console
./DiscordChatExporter.Cli dm -t "mfa.Ifrn"
```

### List servers

To list all servers accessible by the current account, use the `guilds` command:

```console
./DiscordChatExporter.Cli guilds -t "mfa.Ifrn" > C:\path\to\output.txt
```



