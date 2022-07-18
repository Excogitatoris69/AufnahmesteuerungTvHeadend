using CommandLine;
using System;
using System.Text;
using TvHeadendRestApiClientLibrary;

namespace AufnahmesteuerungTvHeadend
{
    /// <summary>
    /// Cli-Client for control, add, remove recordings in TvHeadend. For use with TV-Browser.
    /// This is free software that I made for myself in my spare time. I offer these freely, without financial intentions. 
    /// Author: Oliver Matle
    /// Date: August, 2021
    /// </summary>
    /// <seealso cref="https://github.com/Excogitatoris69/TvHeadendRestApiClientLibrary"/>
    /// <seealso cref="https://tvheadend.org/"/>
    /// <seealso cref="https://www.tvbrowser.org/"/>
    /// <seealso cref="https://github.com/commandlineparser/commandline"/>
    /// 
    public class AufnahmesteuerungTvHeadendClient
    {
        public static readonly string releaseString = "1.1 , Juli 2022";
        private static int necessaryApiVersion = 19;
        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Latin1;
            Console.WriteLine("AufnahmesteuerungTvHeadendClient. Release:{0}, TvHeadendLibrary-Release:{1}", releaseString, TvHeadendLibrary.RELEASESTRING); 
            Execute(args);
        }


        private static void Execute(string[] args)
        {
            RequestData requestData = new RequestData();
            ParserResult<CommandLineOptions> result = Parser.Default.ParseArguments<CommandLineOptions>(args);

            ParserResult<CommandLineOptions> parseResult = result.WithParsed<CommandLineOptions>(cmdParam =>
            {
                requestData.Command = cmdParam.Command;
                requestData.ServerUrl = cmdParam.Serverurl;
                requestData.UserName = cmdParam.Username;
                requestData.Password = cmdParam.Password;
                requestData.Starttime = cmdParam.Starttime;
                requestData.Endtime = cmdParam.Endtime;
                requestData.ChannelName = cmdParam.Channelname;
                requestData.Title = cmdParam.Title;
                requestData.Description = cmdParam.Description;
                requestData.Uuid = cmdParam.Uuid;
                requestData.Language = cmdParam.Language;
                requestData.DvrProfileName = cmdParam.DvrProfileName;
                requestData.Comment = cmdParam.DvrComment;
                requestData.Priority = Priority.Unknown;
            });
            if (parseResult.Tag == ParserResultType.NotParsed)
            {
                //Environment.Exit(1);
            }

            try
            {
                
                CheckApiVersion(requestData);

                if (requestData.Command == Command.DVRCREATE)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    TvHeadendResponseData aTvHeadendResponseData = aTvHeadendLibrary.AddDvrEntry(requestData);

                    if (aTvHeadendResponseData != null)
                    {
                        Console.WriteLine("Successful. UUID:{0}", aTvHeadendResponseData.Uuid);
                    }
                }
                else if (requestData.Command == Command.DVRREMOVE)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    TvHeadendResponseData aTvHeadendResponseData = aTvHeadendLibrary.RemoveDvrEntry(requestData);
                    if (aTvHeadendResponseData != null)
                    {
                        Console.WriteLine("Successful. UUID:{0}", aTvHeadendResponseData.Uuid);
                    }
                }
                else if (requestData.Command == Command.DVRLIST)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    DvrUpcomingEntryList dvrUpcomingEntryList = aTvHeadendLibrary.GetDvrUpcominglist(requestData);
                    if (dvrUpcomingEntryList.Entries != null && dvrUpcomingEntryList.Entries.Count > 0)
                    {
                        foreach (DvrUpcomingEntry elem in dvrUpcomingEntryList.Entries)
                        {
                            Console.WriteLine("\"{0}\",{1},{2},{3},{4},{5}", elem.Title.GetFirstTitle(), elem.Channelname, 
                                FormatUnixtimestamp(elem.Start), FormatUnixtimestamp(elem.Stop), elem.Owner, elem.Uuid);
                        }
                    }
                }
                else if (requestData.Command == Command.CHANNELLIST)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    ChannelEntryList channelEntryList = aTvHeadendLibrary.GetChannellist(requestData);
                    if (channelEntryList.Entries != null && channelEntryList.Entries.Count > 0)
                    {
                        foreach (ChannelEntry elem in channelEntryList.Entries)
                        {
                            Console.WriteLine("{0}", elem.Name);
                        }
                    }
                }
                else if (requestData.Command == Command.DVRCONFIGLIST)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    DvrConfigEntryList dvrConfigEntryList = aTvHeadendLibrary.GetDvrConfiglist(requestData);
                    if (dvrConfigEntryList.Entries != null && dvrConfigEntryList.Entries.Count > 0)
                    {
                        foreach (DvrConfigEntry elem in dvrConfigEntryList.Entries)
                        {
                            Console.WriteLine("{0}", elem.Name);
                        }
                    }
                }
                else if (requestData.Command == Command.LANGUAGELIST)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    LanguageEntryList languageEntryList = aTvHeadendLibrary.GetLanguagelist(requestData);
                    if (languageEntryList.Entries != null && languageEntryList.Entries.Count > 0)
                    {
                        foreach (LanguageEntry elem in languageEntryList.Entries)
                        {
                            Console.WriteLine("{0}: {1}", elem.Shortname, elem.Longname);
                        }
                    }
                }
                else if (requestData.Command == Command.SERVERINFO)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    Serverinfo serverinfo = aTvHeadendLibrary.GetServerinfo(requestData);
                    Console.WriteLine("API:{0},TvHeadend:{1}", serverinfo.VersionApi, serverinfo.VersionSoftware);
                    //Environment.Exit(0);
                }
                else
                {
                    throw new TvHeadendException(Messages.MESSAGE_INVALID_COMMAND + ": " + requestData.Command);
                }

            }
            catch (TvHeadendException e)
            {
                Console.Error.WriteLine(e.Message);
                //Environment.Exit(1);
            }

        }

        /// <summary>
        /// Compare Api-Version of TvHeadend with Necessary Api-Version.
        /// </summary>
        private static void CheckApiVersion(RequestData requestData)
        {
            TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
            Serverinfo serverinfo = aTvHeadendLibrary.GetServerinfo(requestData);
            if (necessaryApiVersion != serverinfo.VersionApi)
            {
                Console.WriteLine("Warning of possible incompatibility due to different API-Versions.  Necessary Api-Version:{0}, Available Version:{1}", necessaryApiVersion, serverinfo.VersionApi);
            }
        }

        private static string FormatUnixtimestamp(double unixtimestamp)
        {
            return UnixTimeStampToDateTime(unixtimestamp).ToString("HH:mm dd.MM.yyyy");
        }

        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
