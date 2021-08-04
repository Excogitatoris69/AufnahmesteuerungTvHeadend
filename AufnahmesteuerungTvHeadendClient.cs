using CommandLine;
using System;
using TvHeadendRestApiClientLibrary;

namespace AufnahmesteuerungTvHeadend
{
    class AufnahmesteuerungTvHeadendClient
    {
        static void Main(string[] args)
        {
            Execute(args);
        }


        static private void Execute(string[] args)
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
                Environment.Exit(1);
            }

            try
            {
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
                            Console.WriteLine("{1}: {0}", elem.Name, elem.Uuid);
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
                    Environment.Exit(0);
                }
                else
                {
                    throw new TvHeadendException(Messages.MESSAGE_INVALID_COMMAND + ": " + requestData.Command);
                }

            }
            catch (TvHeadendException e)
            {
                Console.Error.WriteLine(e.Message);
                Environment.Exit(1);
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
