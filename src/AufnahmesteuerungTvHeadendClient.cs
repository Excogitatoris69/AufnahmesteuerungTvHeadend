using CommandLine;
using System;
using System.IO;
using System.Text;
using TvHeadendRestApiClientLibrary;
using System.Diagnostics;

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
        private static int necessaryApiVersion = 19;
        public static void Main(string[] args)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            
            Console.OutputEncoding = Encoding.Latin1;
            Console.WriteLine("AufnahmesteuerungTvHeadendClient. Release:{0}, TvHeadendLibrary-Release:{1}", fvi.FileVersion, TvHeadendLibrary.RELEASESTRING); 
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
                requestData.StreamplayerPath = cmdParam.StreamplayerPath;
            });
            if (parseResult.Tag == ParserResultType.NotParsed)
            {
                //Environment.Exit(1);
            }

            try
            {
                CleanParams(requestData);
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
                else if (requestData.Command == Command.LIVESTREAM)
                {
                    TvHeadendLibrary aTvHeadendLibrary = new TvHeadendLibrary();
                    string streamUrl = aTvHeadendLibrary.getLivestreamUrl(requestData);
                    string[] streamplayerData = splitStreamplayerParams(requestData.StreamplayerPath);

                    if (streamUrl != null)
                    {
                        if (!File.Exists(streamplayerData[0]))
                        {
                            Console.WriteLine("Not Successful. Videoplayer not found in path: " + streamplayerData[0]);
                            throw new TvHeadendException(Messages.MESSAGE_PATH_NOT_FOUND + ": " + streamplayerData[0]);
                        }
                        else
                        {
                            StringBuilder strbuf = new StringBuilder();
                            strbuf.Append(streamUrl);
                            if (streamplayerData[0] != null)
                            {
                                strbuf.Append(" ");
                                strbuf.Append(streamplayerData[1]);
                            }

                            ProcessStartInfo startInfo = new ProcessStartInfo();
                            startInfo.Arguments = strbuf.ToString();
                            startInfo.FileName = streamplayerData[0];
                            startInfo.WindowStyle = ProcessWindowStyle.Normal;
                            startInfo.UseShellExecute = false;
                            Process exeProcess = Process.Start(startInfo);
                            Console.WriteLine("Successful.");

                        }
                    }
                    else
                    {
                        Console.WriteLine("Not Successful. StreamUrl is null. ");
                    }
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
        /// Im Parameter --streamplayer können folgende Parameter enthalten sein.
        /// Fall 1: nur der Pfad
        /// Fall 2: Pfad und sonstige VLC-Parameter
        /// Diese Funktion trennt die Zeichenfolge auf.
        /// Im Array an Pos. 1 sthet immer der Pfad
        /// Im Array an Pos. 2 steht immer der Rest, also die zusätzlichen VLC-Parameter
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string[] splitStreamplayerParams(string param)
        {
            string[] result = new string[2];
            string param1 = param.Trim();
            string param2;

            int pos1 = param1.IndexOf(',', 0); // 123, --vlc-option  -> pos1=3
            if (pos1 < 0) // Fall 1
            {
                result[0] = param1;
                result[1] = null;
            }
            else // Fall 2
            {
                result[0] = param1.Substring(0, pos1).Trim();
                param2 = param1.Substring(pos1 + 1).Trim();
                if (param2.Length > 0)
                    result[1] = param2;
                else
                    result[1] = null;
            }

            return result;
        }

        /// <summary>
        /// Im Parameter --streamplayer können folgende Parameter enthalten sein.
        /// Fall 1: nur der Pfad
        /// Fall 2: nur der Pfad , jedoch mit Leerzeichen und deshalb vorne und hinten durch Doppelhochkomma umschlossen
        /// Fall 3: Pfad ohne Leerzeichen und sonstige VLC-Parameter
        /// Fall 4: Pfad mit Leerzeichen, mit Hochkommas und sonstige VLC-Parameter.
        /// Diese Funktion trennt die Zeichenfolge auf.
        /// Im Array an Pos. 1 sthet immer der Pfad ohne Hochkomma
        /// Im Array an Pos. 2 steht immer der Rest, also die zusätzlichen VLC-Parameter
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public static string [] splitStreamplayerParams1(string param)
        {
            string[] result = new string[2];
            string param1 = param.Trim();
            string param2;

            if (param1.StartsWith("\"")) // Fall 2 oder 4
            {
                int pos1 = param1.IndexOf('"', 1); // "123" --vlc-option  -> pos1=4
                result[0] = param1.Substring(1, pos1-1);
                param2 = param1.Substring(pos1 + 1).Trim();
                if (param2.Length > 0)
                    result[1] = param2;
                else
                    result[1] = null;
            }
            else// Fall 1 oder 3
            {
                int pos1 = param1.IndexOf(' ', 0); // 123 --vlc-option  -> pos1=3
                if (pos1 < 0)
                {
                    result[0] = param1;
                    result[1] = null;
                }
                else
                {
                    result[0] = param1.Substring(0, pos1);
                    param2 = param1.Substring(pos1).Trim();
                    if (param2.Length > 0)
                        result[1] = param2;
                    else
                        result[1] = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Cleans the parameters and adds default values if they are incomplete.
        /// </summary>
        /// <param name="requestData"></param>
        private static void CleanParams(RequestData requestData)
        {
            if(requestData.Title == null || requestData.Title.Trim().Length == 0)
            {
                //requestData.Title = "not available";
                // title is channel and timestamp
                requestData.Title = string.Format("{0}_{1}", requestData.ChannelName, FormatUnixtimestamp(requestData.Starttime));
            }
            //if (requestData.Description == null || requestData.Description.Trim().Length == 0)
            //{
            //    requestData.Description = "";
            //}
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
