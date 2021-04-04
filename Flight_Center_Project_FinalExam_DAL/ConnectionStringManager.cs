using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Sql;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Center_Project_FinalExam_DAL
{
    public static class ConnectionStringManager
    {


        /// <summary>
        /// this method must  be executed in scope of open connection!
        /// </summary>
        /// <param name="dbName"></param>
        public static void CreateNewDatabase(string dbName, SqlCommand command)
        {
            command.CommandType = CommandType.Text;
            command.CommandText = "Create Database " + dbName;
            command.ExecuteNonQuery();
        }


        public static async Task<bool> WriteConnectionStringToFile(string proposedDataBaseName, string defaultDataBaseName)
        {
             Task<bool> tsk = await Task.Factory.StartNew(async () =>
             {
                  string connString = await CreateNewConnectionSring(proposedDataBaseName, defaultDataBaseName);

                  File.WriteAllText("connection_string.txt", connString);
                 if (File.Exists("connection_string.txt")) return true;
                  return false;

             });
            return await tsk;
        }


        public async static Task<string> CreateNewConnectionSring(string proposedDataBaseName, string defaultDataBaseName)
        {
            return await Task.Run(async() => 
            {
                GetServerAndInstanceNames(out string serverName, out string instanceName);

                List<string> allDataBasesNames = await GetAllDataBases();
                string dataBaseName = defaultDataBaseName;
                foreach(string s in allDataBasesNames)
                {
                    if(s.Contains(proposedDataBaseName))
                    {
                        dataBaseName = s;
                    }
                }

                string connStr = $"Data Source={serverName}{instanceName};Initial Catalog={dataBaseName};Integrated Security=True";
                return connStr;
            });
        }



        public static async Task<List<string>> GetAllDataBases()
        {
            return await Task.Run(() => 
            {
                List<string> databases = new List<string>();
                GetServerAndInstanceNames(out string serverName, out string instanceName);

                //
                //the server name also represented by Environment.MachineName,
                //as: 
                //serverName = Environment.MachineName;
                //so uncommenting the previous line is a way to get this machine name for Smo object
                //if the method "GetServerAndInstanceNames" don't working
                //

                var server = new Microsoft.SqlServer.Management.Smo.Server(serverName + instanceName);
                foreach (Database db in server.Databases)
                {
                    databases.Add(db.Name);
                }
                return databases;
            });
            
        }
        private static void GetServerAndInstanceNames(out string serverName, out string instanceName)
        {
            string servername = string.Empty;
            string instancename = string.Empty;
            var table = SqlDataSourceEnumerator.Instance.GetDataSources();
            
            foreach (DataRow row in table.AsEnumerable())
            {
                if (!(row["ServerName"] is DBNull)) servername = (string)row["ServerName"];
                if (!(row["InstanceName"] is DBNull)) instancename = "\\" + (string)row["InstanceName"];
            }
            serverName = servername;
            instanceName = instancename;
        }
    }
}
