using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Data.SqlClient;
using System.Data;

#if DEBUG
using System.Diagnostics;
#endif

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
#if DEBUG
            var stopwatch = Stopwatch.StartNew();
            var overall = Stopwatch.StartNew();
#endif
            var teams = await Models.VexDb.Team.Download();
#if DEBUG
            stopwatch.Stop();
            Debug.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
#endif
            //var teams = await Models.VexDb.Downloader.Download<Models.VexDb.Team>("http://api.vex.us.nallen.me/get_teams?region=Indiana");

            using (SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["CoreConnection"].ConnectionString))
            {
                con.Open();
                using (SqlCommand command = new SqlCommand("", con))
                {
#if DEBUG
                    stopwatch.Restart();
#endif
                    command.CommandText = "SELECT * FROM dbo.Teams;";

                    DataTable table = new DataTable();
                    table.Load(command.ExecuteReader());
                    table.Rows.Clear();

                    foreach (var team in teams)
                    {
                        DataRow row = table.NewRow();
                        row["Number"] = team.Number;
                        row["Name"] = team.Name;
                        row["RobotName"] = team.RobotName;
                        row["Organization"] = team.Organization;
                        row["City"] = team.City;
                        row["Region"] = team.Region;
                        row["Country"] = team.Country;
                        row["IsRegistered"] = team.IsRegistered;
                        row["Level"] = team.Level;
                        row["Program"] = team.Program;
                        table.Rows.Add(row);
                    }

                    command.CommandText =
                        @"IF OBJECT_ID('dbo._Teams', 'U') IS NOT NULL DROP TABLE dbo._Teams; SELECT * INTO dbo._Teams FROM dbo.Teams WHERE 1 = 0;";
                    command.ExecuteNonQuery();

                    using (SqlBulkCopy bulk = new SqlBulkCopy(con))
                    {
                        bulk.DestinationTableName = "dbo._Teams";

                        bulk.WriteToServer(table);
                    }

#if DEBUG
                    stopwatch.Stop();
                    Debug.WriteLine($"BULK INSERT time: {stopwatch.ElapsedMilliseconds} ms");
                    stopwatch.Restart();
#endif

                    command.CommandText =
@"MERGE INTO dbo.Teams AS T
USING dbo._Teams AS S
ON (T.Number = S.Number)
WHEN NOT MATCHED BY TARGET
	THEN INSERT (Number, Name, RobotName, Organization, City, Region, Country, IsRegistered, [Level], Program) VALUES (S.Number, S.Name, S.RobotName, S.Organization, S.City, S.Region, S.Country, S.IsRegistered, S.[Level], S.Program)
WHEN MATCHED
	THEN UPDATE SET T.Name = S.Name, T.RobotName = S.RobotName, T.Organization = S.Organization, T.City = S.City, T.Region = S.Region, T.Country = S.Country, T.[Level] = S.[Level], T.Program = S.Program
WHEN NOT MATCHED BY SOURCE
	THEN DELETE;

DROP TABLE dbo._Teams;";
                    command.ExecuteNonQuery();
                }
#if DEBUG
                overall.Stop();
                Debug.WriteLine($"Overall execution time: {overall.ElapsedMilliseconds} ms");
#endif
            }
            return;
        }
    }
}
