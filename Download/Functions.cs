using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using System.Configuration;
using Download.Models;
using System;
using System.Globalization;
using System.Diagnostics;

namespace Download
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        {
            log.WriteLine(message);
        }

        public async static Task DownloadAll([QueueTrigger("downloadallrequest")] string message)
        {
            /*
            Console.WriteLine("Downloading teams...");
            ICollection<dynamic> teams = await Models.VexDb.Downloader.Download<dynamic>("http://api.vex.us.nallen.me/get_teams");
            Console.WriteLine("Download complete.");
            */

            Console.WriteLine("Downloading events...");
            ICollection<dynamic> events = await Models.VexDb.Downloader.Download<dynamic>("http://api.vex.us.nallen.me/get_events");
            Console.WriteLine("Download complete");

            using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["CoreConnection"].ConnectionString))
            {
                Console.WriteLine("Opening db connection");
                connection.Open();
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    /*
                    // TEAMS UPSERT
                    cmd.CommandText = "SELECT * FROM dbo.Teams;";
                    DataTable teamTable = new DataTable();
                    teamTable.Load(cmd.ExecuteReader());
                    teamTable.Rows.Clear();
                    Console.WriteLine("Configured Teams data table, filling with downloaded data");
                    foreach (var team in teams)
                    {
                        var row = teamTable.NewRow();
                        row["Number"] = team.number;
                        row["Name"] = team.team_name;
                        row["RobotName"] = team.robot_name;
                        row["Organization"] = team.organisation;
                        row["City"] = team.city;
                        row["Region"] = team.region;
                        row["Country"] = team.country;
                        row["IsRegistered"] = ((string)team.is_registered).ToBoolean("1", "0");
                        row["Level"] = ((string)team.grade).ToLevel();
                        row["Program"] = ((string)team.program).ToProgram();
                        teamTable.Rows.Add(row);
                    }
                    Console.WriteLine("Data filled");
                    Console.WriteLine("Create/replacing teams temp table on db");
                    cmd.CommandText = "IF OBJECT_ID('dbo._Teams', 'U') IS NOT NULL DROP TABLE dbo._Teams; SELECT * INTO dbo._Teams FROM dbo.TEAMS WHERE 1 = 0;";
                    cmd.ExecuteNonQuery();

                    using (SqlBulkCopy bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = "dbo._Teams";
                        bulk.WriteToServer(teamTable);
                    }
                    Console.WriteLine("Bulk insert of teams complete.");
                    Console.WriteLine("Merging temp teams table with primary teams table");
                    cmd.CommandText =
@"MERGE INTO dbo._Teams AS T using dbo.Teams AS S ON T.Number = S.Number AND T.Name = S.Name AND T.RobotName = S.RobotName AND T.Organization = S.Organization AND T.City = S.City AND T.Region = S.Region AND T.Country = S.Country AND T.[Level] = S.[Level] AND T.Program = S.Program
WHEN MATCHED THEN DELETE;

MERGE INTO dbo.Teams AS T USING dbo._Teams AS S ON (T.Number = S.Number)
WHEN NOT MATCHED BY TARGET
    THEN INSERT (Number, Name, RobotName, Organization, City, Region, Country, IsRegistered, [Level], Program) VALUES (S.Number, S.Name, S.RobotName, S.Organization, S.City, S.Region, S.Country, S.IsRegistered, S.[Level], S.Program)
WHEN MATCHED
    THEN UPDATE SET 
		T.Name = S.Name, T.RobotName = S.RobotName, T.Organization = S.Organization, T.City = S.City, T.Region = S.Region, T.Country = S.Country, T.[Level] = S.[Level], T.Program = S.Program
WHEN NOT MATCHED BY SOURCE
    THEN DELETE;

DROP TABLE dbo._Teams;";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Teams upsert complete.");
                    */

                    // EVENTS UPSERT
                    cmd.CommandText = "SELECT * FROM dbo.Events;";
                    DataTable eventTable = new DataTable();
                    eventTable.Load(cmd.ExecuteReader());
                    eventTable.Rows.Clear();
                    Console.WriteLine("Events data table configured.");
                    cmd.CommandText = "SELECT * FROM dbo.Divisions;";
                    DataTable divTable = new DataTable();
                    divTable.Load(cmd.ExecuteReader());
                    divTable.Rows.Clear();
                    Console.WriteLine("Divisions data table configured, filling events and divisions tables.");
                    foreach (var e in events)
                    {
                        var row = eventTable.NewRow();
                        row["Sku"] = e.sku;
                        row["Program"] = ((string)e.program).ToProgram();
                        row["Name"] = e.name;
                        if (!e.start.ToString().Equals("0000-00-00 00:00:00"))
                            row["Start"] = DateTime.ParseExact(e.start.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        if (!e.end.ToString().Equals("0000-00-00 00:00:00"))
                            row["Finish"] = DateTime.ParseExact(e.end.ToString(), "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        row["Season"] = e.season;
                        row["Venue_Name"] = e.loc_venue;
                        row["Venue_Address"] = string.IsNullOrEmpty(e.loc_address2) ? e.loc_addres1 : $"{e.loc_address1}\n{e.loc_address2}";
                        row["Venue_City"] = e.loc_city;
                        row["Venue_Region"] = e.loc_region;
                        row["Venue_Country"] = e.loc_country;
                        row["Levels"] = VexTeamNetwork.Models.Level.Unknown;
                        foreach (var d in e.divisions)
                        {
                            var divRow = divTable.NewRow();
                            divRow["EventSku"] = e.sku;
                            divRow["Name"] = d;
                            divTable.Rows.Add(divRow);
                        }
                        eventTable.Rows.Add(row);
                    }
                    Console.WriteLine("Tables filled.");
                    cmd.CommandText = "IF OBJECT_ID('dbo._Events', 'U') IS NOT NULL DROP TABLE dbo._Events; SELECT * INTO dbo._Events FROM dbo.Events WHERE 1 = 0;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "IF OBJECT_ID('dbo._Divisions', 'U') IS NOT NULL DROP TABLE dbo._Divisions; SELECT * INTO dbo._Divisions FROM dbo.Divisions WHERE 1 = 0;";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Create/replacing temp events table and temp divisions table.");
                    using (SqlBulkCopy bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = "dbo._Events";
                        bulk.WriteToServer(eventTable);
                    }
                    Console.WriteLine("Events bulk inserted");
                    using (SqlBulkCopy bulk = new SqlBulkCopy(connection))
                    {
                        bulk.DestinationTableName = "dbo._Divisions";
                        bulk.WriteToServer(divTable);
                    }
                    Console.WriteLine("Divisions bulk inserted");

                    Console.WriteLine("Merging events and divisions temp tables with their respective primary tables");
                    cmd.CommandText =
@"MERGE INTO dbo._Events AS T USING dbo.Events AS S ON S.Sku = T.Sku AND S.Name = T.Name AND S.Program = T.Levels AND T.Start = S.Start AND T.Finish = S.Finish AND T.Season = S.Season AND T.[Description] = S.[Description] AND T.Agenda = S.Agenda AND T.Venue_Name = S.Venue_Name AND T.Venue_Address = S.Venue_Address AND T.Venue_City = S.Venue_City AND T.Venue_Region = S.Venue_Region AND T.Venue_Country = S.Venue_Country AND T.Contact_Name = S.Contact_Name AND T.Contact_Title = S.Contact_Title AND T.Contact_Email = S.Contact_Email AND T.Contact_Phone = S.Contact_Phone
WHEN MATCHED THEN DELETE;

MERGE INTO dbo.Events AS T USING dbo._Events AS S ON (T.Sku = S.Sku)
WHEN NOT MATCHED BY TARGET THEN INSERT (Sku, Program, Levels, Name, Start, Finish, Season, Venue_Name, Venue_Address, Venue_City, Venue_Region, Venue_Country) VALUES (S.Sku, S.Program, S.Levels, S.Name, S.Start, S.Finish, S.Season, S.Venue_Name, S.Venue_Address, S.Venue_City, S.Venue_Region, S.Venue_Country)
WHEN MATCHED THEN UPDATE SET T.Program = S.Program, T.Levels = S.Levels, T.Name = S.Name, T.Start = S.Finish, T.Season = S.Season, T.Venue_Name = S.Venue_Name, T.Venue_Address = S.Venue_Address, T.Venue_City = S.Venue_City, T.Venue_Region = S.Venue_Region, T.Venue_Country = S.Venue_Country
WHEN NOT MATCHED BY SOURCE THEN DELETE;

DROP TABLE dbo._Events;

MERGE INTO dbo.Divisions AS T USING dbo._Divisions AS S on (T.EventSku = S.EventSku AND T.Name = S.Name)
WHEN NOT MATCHED BY TARGET THEN INSERT (EventSku, Name) VALUES (S.EventSku, S.Name)
WHEN NOT MATCHED BY SOURCE THEN DELETE;

DROP TABLE dbo._Divisions;";

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Events and divisions upsert complete.");
                }
            }
        }
    }
}
