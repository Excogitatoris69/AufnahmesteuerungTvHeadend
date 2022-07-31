using CommandLine;

namespace AufnahmesteuerungTvHeadend
{
    public class CommandLineOptions
    {
        [Option(shortName: 'a', longName: "command", HelpText = "Command", Required = true)]
        public string Command { get; set; }

        [Option(shortName: 's', longName: "serverurl", HelpText = "Server-URL", Required = true)]
        public string Serverurl { get; set; }

        [Option(shortName: 'u', longName: "username", HelpText = "Username", Required = true)]
        public string Username { get; set; }

        [Option(shortName: 'p', longName: "password", HelpText = "Password", Required = true)]
        public string Password { get; set; }

        [Option(shortName: 'b', longName: "starttime", HelpText = "Starttime (Unixtime)", Required = false)]
        public long Starttime { get; set; }

        [Option(shortName: 'e', longName: "endtime", HelpText = "Endtime (Unixtime)", Required = false)]
        public long Endtime { get; set; }

        [Option(shortName: 'c', longName: "channel", HelpText = "Channelname", Required = false)]
        public string Channelname { get; set; }

        [Option(shortName: 't', longName: "title", HelpText = "Title", Required = false)]
        public string Title { get; set; }

        [Option(shortName: 'd', longName: "description", HelpText = "Description", Required = false)]
        public string Description { get; set; }

        [Option(shortName: 'i', longName: "uuid", HelpText = "UUID", Required = false)]
        public string Uuid { get; set; }

        [Option(shortName: 'l', longName: "language", HelpText = "Language", Required = false)]
        public string Language { get; set; }

        [Option(shortName: 'f', longName: "config", HelpText = "DVR-Configname", Required = false)]
        public string DvrProfileName { get; set; }

        [Option(shortName: 'o', longName: "comment", HelpText = "DVR-Comment", Required = false)]
        public string DvrComment { get; set; }

        [Option(shortName: 'x', longName: "streamplayer", HelpText = "Streamplayer path", Required = false)]
        public string StreamplayerPath { get; set; }

    }
}
