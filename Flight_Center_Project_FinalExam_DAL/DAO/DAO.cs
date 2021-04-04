using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Dmf;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Flight_Center_Project_FinalExam_DAL
{
    public delegate void ExecuteCurrentMethosProcedure(out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters);

    public abstract class DAO<T> : DaoBase<T>, IBasicDB<T> where T : class, IPoco, new()
    {
        #region Members
        protected List<Dictionary<string, string>> _dataAboutForeignKeysOfAllTables;

        protected SqlConnection _connection = new SqlConnection();
        protected SqlCommand _command = new SqlCommand();

        protected AirlineDAOMSSQL<AirlineCompany> _currentAirlineDAOMSSQL;
        protected CountryDAOMSSQL<Country> _currentCountryDAOMSSQL;
        protected CustomerDAOMSSQL<Customer> _currentCustomerDAOMSSQL;
        protected FlightDAOMSSQL<Flight> _currentFlightDAOMSSQL;
        protected TicketDAOMSSQL<Ticket> _currentTicketDAOMSSQL;
        protected AdministratorDAOMSSQL<Administrator> _currentAdministratorDAOMSSQL;
        protected Utility_class_UserDAOMSSQL<Utility_class_User> _currentUserDAOMSSQL;

        private delegate void createDAOInstance();        

        #endregion Members
        #region Constructor
        public DAO()
        {
            _command.CommandType = System.Data.CommandType.StoredProcedure;
            _command.Connection = _connection;

            this.SetConnectionString();

            //information about all the foreign keys of all the tables in the current DB (relying on Poco classes definitions)
            _dataAboutForeignKeysOfAllTables = RetriveForeignKeysOfAllTables();
        }
        #endregion Constructor
        #region private and protected methods for DAL internal usage

        protected System.Configuration.Configuration UseConfigFile()
        {
            System.Configuration.Configuration config = null;
            try
            {
                lock (this)
                {
                    if (System.Web.HttpContext.Current == null)
                        config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    else
                        config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
                }
            }
            catch(Exception ex)
            {
                Thread.Sleep(50);
                config = UseConfigFile();
            }
            //IOException
            //

            return config;
        }
        protected void SaveConfiguration(System.Configuration.Configuration config)
        {
            try
            {
                lock (this)
                {
                    config.Save(ConfigurationSaveMode.Modified);
                }
            }
            catch(Exception ex)
            {
                Thread.Sleep(50);
                SaveConfiguration(config);
            }
            //IOException
            //ConfigurationErrorsException
            }

        protected void SetConnectionString()
        {
            /*System.Configuration.Configuration config = null;
            if (System.Web.HttpContext.Current == null)
                config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            else
                config = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("~");
            */
            System.Configuration.Configuration config =  UseConfigFile();

            //string connStrAddition = @"\SQLEXPRESS"; - for class machines
            string connStrAddition = "";
            //if (Environment.MachineName.ToLower().Contains("muzika")) connStrAddition = string.Empty;
            //string connStr = $"Data Source={Environment.MachineName}{connStrAddition};Initial Catalog=The_very_important_Flight_Center_Project;Integrated Security=True";

            //GetServerAndInstanceNames(out string sqlServerName, out string SqlServerInstanceName);
            string connStr = string.Empty;
            if (System.Web.HttpContext.Current != null)
                connStr = $"Data Source={Environment.MachineName}{connStrAddition};Initial Catalog=The_very_important_Flight_Center_Project;Integrated Security=SSPI;Asynchronous Processing=true";
            //else connStr = File.ReadAllText("connection_string.txt");
            //Data Source=MUZIKAFAMILYPC;Initial Catalog=The_very_important_Flight_Center_Project;Integrated Security=True
            else connStr = $"Data Source={Environment.MachineName};Initial Catalog=The_very_important_Flight_Center_Project;Integrated Security=True";
            lock (this)
            {
                config.ConnectionStrings.ConnectionStrings[0].ConnectionString = connStr;
                config.ConnectionStrings.ConnectionStrings[0].ProviderName = "System.Data.SqlClient";
                //config.Save(ConfigurationSaveMode.Modified);
                SaveConfiguration(config);
            }
            //IOException

            _connection.ConnectionString = config.ConnectionStrings.ConnectionStrings[0].ConnectionString;

        }
        /// <summary>
        /// Retriving information about all the foreign keys of all the tables in the current DB.
        /// Relies on Poco classes definitions.
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<string, string>> RetriveForeignKeysOfAllTables()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                List<Type> classes = assembly.GetExportedTypes().Where(x => typeof(IPoco).IsAssignableFrom(x) && !x.IsInterface).ToList();

                _command.CommandType = CommandType.Text;
                List<Dictionary<string, string>> foreign_key_tables_dataLst = new List<Dictionary<string, string>>();

                _connection.Open();
                foreach (Type classType in classes)
                {
                    _command.CommandText = "SELECT OBJECT_NAME(parent_object_id) AS [FK Table], name AS [Foreign Key], " +
                                           "OBJECT_NAME(referenced_object_id) AS [PK Table] FROM sys.foreign_keys " +
                                           $"WHERE parent_object_id = OBJECT_ID('{this.GetTableName(classType)}');";


                    string[] fk_data_table_namesArr = new string[] { "Foreign_Key_Table", "Foreign_Key_Name", "Primary_Key_Table" }; //there are keys of the dictionaries, they're constatnt and used as labels
                    using (SqlDataReader reader = _command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Dictionary<string, string> dataDict = new Dictionary<string, string>();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                object value = reader[i];
                                dataDict.Add(fk_data_table_namesArr[i], value.ToString());
                            }
                            foreign_key_tables_dataLst.Add(dataDict);
                        }
                    }
                }

                return foreign_key_tables_dataLst;
            }
            finally { _connection.Close(); }
        }

        /// <summary>
        /// Gets all the table names.
        /// because this function is intended to be used bu other functions, 
        /// it's don't need to acsess the connection.
        /// </summary>  
        private List<string> GetAllTableNames()
        {
            List<string> tableNames = new List<string>();
            var schema = _connection.GetSchema("Tables");
            foreach (DataRow s in schema.Rows)
            {
                string tablename = (string)s[2];
                tableNames.Add(tablename);
            }
            return tableNames;
        }
        internal string GetTableName(Type pocoType)
        {
            string tableName = string.Empty;
            foreach (var s in this.GetAllTableNames())
            {
                //if (s.Contains(pocoType.Name.ChopCharsFromTheEnd(1))) tableName = s;
                string pocoType_Name = string.Empty;
                if (pocoType.Name.Contains("History")) pocoType_Name = pocoType.Name;
                else pocoType_Name = pocoType.Name.PluraliseNoun();

                if (s.Equals(pocoType_Name)) tableName = s;
            }
            return tableName;
        }

        /// <summary>
        /// This function is reading from tha DataBase and returns one poco object
        /// </summary>
        /// <param name="reader">Initialized SqlDataReader</param>
        /// <returns></returns>
        private async Task<T> ReadFromDbAndFillPocoAsync(SqlDataReader reader)
        {
            T poco = new T();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                object value = await reader.GetFieldValueAsync<object>(i);
                if (reader[i] is DBNull && typeof(T).GetProperties()[i].GetType().Name.ToLower().Equals("string"))
                {
                    typeof(T).GetProperties()[i].SetValue(poco, string.Empty);
                }
                if (reader[i] is DBNull && typeof(T).GetProperties()[i].GetType().Name.ToLower().Contains("int"))
                {
                    typeof(T).GetProperties()[i].SetValue(poco, 0);
                }

                if (!(reader[i] is DBNull)) { typeof(T).GetProperties()[i].SetValue(poco, value); }

            }
            return poco;
        }
        private T ReadFromDbAndFillPoco(SqlDataReader reader)
        {
            T poco = new T();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                object value = reader.GetValue(i);
                if (reader[i] is DBNull && typeof(T).GetProperties()[i].GetType().Name.ToLower().Equals("string"))
                {
                    typeof(T).GetProperties()[i].SetValue(poco, string.Empty);
                }
                if (reader[i] is DBNull && typeof(T).GetProperties()[i].GetType().Name.ToLower().Contains("int"))
                {
                    typeof(T).GetProperties()[i].SetValue(poco, 0);
                }

                if (!(reader[i] is DBNull)) { typeof(T).GetProperties()[i].SetValue(poco, value); }

            }
            return poco;
        }

        protected void SetAnotherDAOInstance(Type daoType)
        {
            Dictionary<Type, createDAOInstance> instanceTypeCorrelation = new Dictionary<Type, createDAOInstance>();
            instanceTypeCorrelation.Add(typeof(AirlineCompany), () => { _currentAirlineDAOMSSQL = new AirlineDAOMSSQL<AirlineCompany>(); });
            instanceTypeCorrelation.Add(typeof(Country), () => { _currentCountryDAOMSSQL = new CountryDAOMSSQL<Country>(); });
            instanceTypeCorrelation.Add(typeof(Customer), () => { _currentCustomerDAOMSSQL = new CustomerDAOMSSQL<Customer>(); });
            instanceTypeCorrelation.Add(typeof(Flight), () => { _currentFlightDAOMSSQL = new FlightDAOMSSQL<Flight>(); });
            instanceTypeCorrelation.Add(typeof(Ticket), () => { _currentTicketDAOMSSQL = new TicketDAOMSSQL<Ticket>(); });
            instanceTypeCorrelation.Add(typeof(Utility_class_User), () => { _currentUserDAOMSSQL = new Utility_class_UserDAOMSSQL<Utility_class_User>(); });
            instanceTypeCorrelation.Add(typeof(Administrator), () => { _currentAdministratorDAOMSSQL = new AdministratorDAOMSSQL<Administrator>(); });

            instanceTypeCorrelation[daoType]();
        }

        #endregion private and protected methods for DAL internal usage

        #region template_methods

        /// <summary>
        /// Open the SQL connection
        /// </summary>
        protected override void OpenConnection()
        {
            _connection.Open();
        }
        protected async override Task OpenConnectionAsync()
        {
            await _connection.OpenAsync();
        }
        /// <summary>
        /// Close the SQL connection
        /// </summary>
        protected override void Close()
        {
            _connection.Close();           
        }

        /// <summary>
        /// Set the SQL command
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="storedProcedureParameters">Dictionary<string, string> that contains names and values of the stored proceture parameters</param>
        protected override void SetCommand(CommandType commandType, string commandText, Dictionary<string, object> storedProcedureParameters)
        {
            //in this overload, CommandType will be "StoredProcedure"
            _command.CommandType = commandType;
            ////in this overload, CommandText will be the stored prosedure name.
            _command.CommandText = commandText;

            _command.Parameters.Clear();
            foreach (var s in storedProcedureParameters)
            {
                _command.Parameters.AddWithValue(s.Key, s.Value);
                //_command.Parameters.Add(new SqlParameter(s.Key, s.Value));
            }


        }
        /// <summary>
        /// Set the SQL command
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        protected override void SetCommand(CommandType commandType, string commandText)
        {
            //in this overload, CommandType will be "Text"
            _command.CommandType = commandType;
            ////in this overload, CommandText will be the command text string.
            _command.CommandText = commandText;
        }  


        protected async override Task<List<T>> ReadFromDbAsync()
        {
            List<T> emptyValueHolder = new List<T>();
            using (SqlDataReader reader = await _command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {                    
                    emptyValueHolder.Add(await ReadFromDbAndFillPocoAsync(reader));
                }
            }
            return emptyValueHolder;
        }
        protected override List<T> ReadFromDb()
        {
            List<T> emptyValueHolder = new List<T>();
            using (SqlDataReader reader = _command.ExecuteReader())
            {
                while (reader.Read())
                {
                    emptyValueHolder.Add(ReadFromDbAndFillPoco(reader));
                }
            }
            return emptyValueHolder;
        }


        protected override IEnumerable<T> ReadFromDbAsIEnumerable()
        {
            try
            {
                _connection.Open();
                using (SqlDataReader reader = _command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return ReadFromDbAndFillPoco(reader);
                    }
                }
            }
            finally { _connection.Close(); }
        }

        protected override long AddToDb()
        {
            return Convert.ToInt64(_command.ExecuteScalar());
        }

        /// <summary>
        /// This method is used to update ot remove values in the DB
        /// </summary>
        protected override void UpdateInDb()
        {
            _command.ExecuteNonQuery();
        }

        #endregion template_methods

        #region Public service methods

        /// <summary>
        /// This method lets get a "poco" object of one type (say "A" type) by some property value
        /// of a poco object of another type (say "B" type). 
        /// For instance, with this method, you can get an "AirlineCompany" object by the value of the property 
        /// "AIRLINECOMPANY_ID" of a "Flight poco object", without creating an actual "Flight"  object.
        /// ("AIRLINECOMPANY_ID" of a "Flight" type is an "ID" of an "AirlineCompany" type).
        /// The method needs parameters as such:
        /// "byWhatInOneTable": the value of a property of "B" type by which an appropriate "poco" object of "A"
        /// type will be found.May be of any type.
        /// "byWhatInOneTable_columnName": The name of the property of the "A" type by which value an appropriate
        /// "poco" object of "A" type will be found.
        /// "byWahatInAnotherTable_columnName": The name of the property of the "B" type by which value an
        /// appropriate "poco" object of "A" type will be found.
        /// These two parameters represent the same value in the two objects.
        ///
        /// "anotherPocoTypePropertyNumber": The serial number (in the sequence from 0 to "n", where "n" is the
        /// actual amount of properties in the type minus 1) in the "B" type.
        /// "anotherPocoType": The actual "B" type.
        /// </summary>
        /// <param name="byWhatInOneTable"></param>
        /// <param name="byWhatInOneTable_columnName"></param>
        /// <param name="byWahatInAnotherTable_columnName"></param>
        /// <param name="anotherPocoTypePropertyNumber"></param>
        /// <param name="anotherPocoType"></param>
        /// <returns>The generic type of the class in which the method is placed.</returns>
        public T GetSomethingInOneTableBySomethingInAnother(object byWhatInOneTable, string byWhatInOneTable_columnName, string byWahatInAnotherTable_columnName, int anotherPocoTypePropertyNumber, Type anotherPocoType)
        {
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));
                string anothertablename = this.GetTableName(anotherPocoType);                
                string relevantColumnNameInAnotherTable = anotherPocoType.GetProperties()[anotherPocoTypePropertyNumber].Name;
                string identifyer_ByWhatInQMarks = string.Empty;
                if (byWhatInOneTable.GetType().Name == "String") identifyer_ByWhatInQMarks = $"'{byWhatInOneTable}'";
                else identifyer_ByWhatInQMarks = byWhatInOneTable.ToString();

                string leftSideOfONStatement = byWhatInOneTable_columnName;
                string rightSideOfONStatement = byWahatInAnotherTable_columnName;

                commandTextForTextMode = $"SELECT {tableName}.* FROM {tableName} JOIN {anothertablename} ON {tableName}.{leftSideOfONStatement} = {anothertablename}.{rightSideOfONStatement} WHERE {anothertablename}.{relevantColumnNameInAnotherTable} = {identifyer_ByWhatInQMarks}";
                commandtextStoredProcedureName = "DAO_BASE_GetSomethingInOneTableBySomethingInAnotherInternal_METHOD_QUERY";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", tableName);
                storedProcedureParameters.Add("ANOTHER_TABLE_NAME", anothertablename);
                storedProcedureParameters.Add("LEFT_SIDE_OF_ONSTATEMENT", leftSideOfONStatement);
                storedProcedureParameters.Add("RIGHT_SIDE_OF_ONSTATEMENT", rightSideOfONStatement);
                storedProcedureParameters.Add("RELEVANT_COLUMN_NAME_IN_ANOTHER_TABLE", relevantColumnNameInAnotherTable);
                storedProcedureParameters.Add("COLUMN_IDENTIFIER", identifyer_ByWhatInQMarks);
            };

            //return RunToRead(executeCurrentMethosProcedure: execute, CommandType.StoredProcedure).FirstOrDefault(); 
            return Run(ReadFromDb_method: () => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }

        protected T GetRegisteredUserInOneTableBySomethingInAnotherInternal(object byWhatInOneTable, int anotherPocoTypePropertyNumber, Type anotherPocoType)
        {
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));
                string anothertablename = this.GetTableName(anotherPocoType);
                //string relevantColumnName = typeof(T).GetProperties()[anotherPocoTypePropertyNumber].Name;
                string relevantColumnNameInAnotherTable = anotherPocoType.GetProperties()[anotherPocoTypePropertyNumber].Name;
                string identifyer_ByWhatInQMarks = string.Empty;
                if (byWhatInOneTable.GetType().Name == "String") identifyer_ByWhatInQMarks = $"'{byWhatInOneTable}'";
                else identifyer_ByWhatInQMarks = byWhatInOneTable.ToString();

                string leftSideOfONStatement = "USER_ID";
                string rightSideOfONStatement = "ID";
                if (typeof(T).Name == "Utility_class_User")
                {
                    leftSideOfONStatement = "ID";
                    rightSideOfONStatement = "USER_ID";
                }

                commandTextForTextMode = $"SELECT {tableName}.* FROM {tableName} JOIN {anothertablename} ON {tableName}.{leftSideOfONStatement} = {anothertablename}.{rightSideOfONStatement} WHERE {anothertablename}.{relevantColumnNameInAnotherTable} = {identifyer_ByWhatInQMarks}";
                commandtextStoredProcedureName = "DAO_BASE_GetSomethingInOneTableBySomethingInAnotherInternal_METHOD_QUERY";
                //commandtextStoredProcedureName = "DAO_BASE_GetRegisteredUserInOneTableBySomethingInAnotherInternal_METHOD_QUERY";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", tableName);
                storedProcedureParameters.Add("ANOTHER_TABLE_NAME", anothertablename);
                storedProcedureParameters.Add("LEFT_SIDE_OF_ONSTATEMENT", leftSideOfONStatement);
                storedProcedureParameters.Add("RIGHT_SIDE_OF_ONSTATEMENT", rightSideOfONStatement);
                storedProcedureParameters.Add("RELEVANT_COLUMN_NAME_IN_ANOTHER_TABLE", relevantColumnNameInAnotherTable);
                storedProcedureParameters.Add("COLUMN_IDENTIFIER", identifyer_ByWhatInQMarks);
            };

            //return RunToRead(executeCurrentMethosProcedure: execute, CommandType.StoredProcedure).FirstOrDefault(); 
            return Run(ReadFromDb_method: () => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure).FirstOrDefault();
        }


        /// <summary>
        /// This method allows to get a Poco object of type T by value of any property of this object.
        /// The firts parameter is the value by which we get the object, the second parameter is an enumeration value which corresponds to the number of the property in question. It needs to be casted to "int".
        /// The enumeration resides in each poco.
        /// </summary>
        /// <param name="identifier">Object with value and underlying data type of the corresponding property.</param>
        /// <param name="propertyNumber">Enumeration value the corresponds to the number of the property</param>
        /// <returns></returns>
        public override T GetSomethingBySomethingInternal(object identifier, int propertyNumber)
        {
            ExecuteCurrentMethosProcedure execute = CreateDelegateForGetSomethingBySomethingInternal(identifier, propertyNumber);

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure).FirstOrDefault();
            return Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure).FirstOrDefault();

        }
        public async override Task<T> GetSomethingBySomethingInternalAsync(object identifier, int propertyNumber)
        {
            ExecuteCurrentMethosProcedure execute = CreateDelegateForGetSomethingBySomethingInternal(identifier, propertyNumber);

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure).FirstOrDefault();
            List<T> list = await RunAsync(() => { return ReadFromDbAsync(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);
            return await Task.Run(() => { return list.FirstOrDefault(); });

        }
        private ExecuteCurrentMethosProcedure CreateDelegateForGetSomethingBySomethingInternal(object identifier, int propertyNumber)
        {
            return (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));
                string commandtext = string.Empty;
                string relevantColumnName = typeof(T).GetProperties().OrderBy(x => x.MetadataToken).ToArray()[propertyNumber].Name;
                string identifierInQMarks = string.Empty;
                if (identifier.GetType().Name == "String") identifierInQMarks = $"'{identifier}'";
                else identifierInQMarks = identifier.ToString();

                commandTextForTextMode = $"SELECT * FROM {tableName} WHERE {relevantColumnName} = {identifierInQMarks}";
                commandtextStoredProcedureName = "DAO_BASE_GetSomethingBySomethingInternal_METHOD_QUERY";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", tableName);
                storedProcedureParameters.Add("RELEVANT_COLUMN_NAME", relevantColumnName);
                storedProcedureParameters.Add("COLUMN_IDENTIFIER", identifierInQMarks);
            };
        }


        public override IEnumerable<T> GetAllASIEnumerable()
        {
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                commandTextForTextMode = $"select * from {GetTableName(typeof(T))}";
                commandtextStoredProcedureName = "DAO_BASE_GetAll_METHOD_QUERY";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", GetTableName(typeof(T)));
            };

            return Run(() => { return ReadFromDbAsIEnumerable(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }

        public override List<T> GetAll()
        {
            ExecuteCurrentMethosProcedure execute = CreateDelegateForGetAllInternal();

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);
            return Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }
        public override Task<List<T>> GetAllAsync()
        {
            ExecuteCurrentMethosProcedure execute = CreateDelegateForGetAllInternal();

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);
            return RunAsync(() => { return ReadFromDbAsync(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
        }
        private ExecuteCurrentMethosProcedure CreateDelegateForGetAllInternal()
        {
            return (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                commandTextForTextMode = $"select * from {GetTableName(typeof(T))}";
                commandtextStoredProcedureName = "DAO_BASE_GetAll_METHOD_QUERY";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", GetTableName(typeof(T)));
            };
        }

        public override long Add(T poco)
        {
            //adding an identifier if the poco object doesen't have one
            var currentIdentifier = typeof(T).GetProperty("IDENTIFIER").GetValue(poco);
            var absentIdentifier = typeof(T).GetProperty("IDENTIFIER").GetValue(new T());
            if (currentIdentifier.Equals(absentIdentifier))
                typeof(T).GetProperty("IDENTIFIER").SetValue(poco, Guid.NewGuid().ToString());
            //End: adding an identifier if the poco object doesen't have one


            long IDvalue = -1;            
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = GetTableName(typeof(T));

                PropertyInfo property = null;
                for (int i = 0; i < typeof(T).GetProperties().Length; i++)
                {
                    PropertyInfo prop = typeof(T).GetProperties()[i];
                    if (prop.Name.ToUpper().Equals("ID"))
                    {
                        int propertyPlace = i + 1;
                        if(i == typeof(T).GetProperties().Length - 1) propertyPlace = i - 1;

                        property = typeof(T).GetProperties()[propertyPlace];
                        break;
                    }
                }

                

                commandTextForTextMode = $"INSERT INTO {tableName} ({property.Name}) VALUES ('{property.GetValue(poco)}') SELECT SCOPE_IDENTITY()";
                commandtextStoredProcedureName = "DAO_BASE_Add_METHOD_QUERY_for_insert";

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", tableName);
                var name = property.Name;
                var value = property.GetValue(poco);
                storedProcedureParameters.Add("SECOND_COLUMN_NAME", name);
                storedProcedureParameters.Add("SECOND_COLUMN_VALUE", value);
            };

            //IDvalue = RunToAdd(executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
            //There is a problem in this stored procedute ("DAO_BASE_Add_METHOD_QUERY_for_insert")
            IDvalue = Run(() => { return AddToDb(); } ,executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
            //IDvalue = RunToAdd(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);
            //poco.GetType().GetProperties()[0].SetValue(poco, IDvalue);
            poco.GetType().GetProperty("ID").SetValue(poco, IDvalue);

            //the second part of the function            
            for (int i = 0; i < typeof(T).GetProperties().Length; i++)
            {
                if (typeof(T).GetProperties()[i].Name.Equals("ID".ToUpper())) continue;

                execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
                {
                    string tableName = GetTableName(typeof(T));

                    PropertyInfo property = typeof(T).GetProperties()[i];
                    PropertyInfo firstColumnProperty = typeof(T).GetProperty("ID");

                    var value = property.GetValue(poco);
                    if (value is DateTime)
                        value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
                    


                    commandTextForTextMode = $"UPDATE {tableName} SET {property.Name} = '{value}' WHERE {firstColumnProperty.Name} = '{firstColumnProperty.GetValue(poco)}'";
                    commandtextStoredProcedureName = "DAO_BASE_Add_METHOD_QUERY_for_update";

                    storedProcedureParameters = new Dictionary<string, object>();
                    storedProcedureParameters.Add("@TABLE_NAME", tableName);
                    storedProcedureParameters.Add("@SUBSEQUENT_COLUMN_NAME", property.Name);
                    storedProcedureParameters.Add("@SUBSEQUENT_COLUMN_VALUE", property.GetValue(poco));
                    storedProcedureParameters.Add("@FIRST_COLUMN_NAME", firstColumnProperty.Name);
                    storedProcedureParameters.Add("@FIRST_COLUMN_VALUE", firstColumnProperty.GetValue(poco));
                };

                Run(() => { UpdateInDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
                //RunToUpdate(executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
                //RunToUpdate(executeCurrentMethosProcedure: execute, commandType: CommandType.StoredProcedure);

            }
            return IDvalue;

        }

        /// <summary>
        /// Gets one poco object from the DB by ID.
        /// In this specific method the Storedprocedure option isn't ready yet
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public override T Get(long ID)
        {
            ExecuteCurrentMethosProcedure execute = CreateDelegateForGetInternal(ID);

            //return RunToRead(executeCurrentMethosProcedure: execute, commandType: CommandType.Text).FirstOrDefault();
            return Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text).FirstOrDefault();
        }
        /// <summary>
        /// Gets one poco object from the DB by any property of this object.
        /// Requres the name of the property being provided as well.
        /// In this specific method the Storedprocedure option isn't ready yet
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public T GetByAny(object getByWhat, string getByWhatPropertyName)
        {
            PropertyInfo[] propInfos = typeof(T).GetProperties().OrderBy(x => x.MetadataToken).ToArray();
            int propertyNumber = -1;
            for(int i = 0; i < propInfos.Length; i++)
            {
                if(propInfos[i].Name == getByWhatPropertyName)
                {
                    propertyNumber = i;
                    break;
                }
            }

            return GetSomethingBySomethingInternal(getByWhat, propertyNumber);
        }
        public async Task<T> GetAsync(long ID)
        {
            ExecuteCurrentMethosProcedure execute = CreateDelegateForGetInternal(ID);

            List<T> list = await RunAsync(() => { return ReadFromDbAsync(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
            return await Task.Run(() => 
            {
                return list.FirstOrDefault();
            });
        }
        private ExecuteCurrentMethosProcedure CreateDelegateForGetInternal(long ID)
        {
            return (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));
                string firstColumnName = typeof(T).GetProperties()[0].Name;
                commandTextForTextMode = $"SELECT * FROM {tableName} WHERE {firstColumnName} = {ID}";
                commandtextStoredProcedureName = "";
                storedProcedureParameters = null;
            };
        }

        public override void Remove(T poco)
        {
                      
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));
                string firstColumnName = typeof(T).GetProperties()[0].Name;
                object firstColumnValue = typeof(T).GetProperties()[0].GetValue(poco);
                commandTextForTextMode = $"DELETE FROM {tableName} WHERE {firstColumnName} = {firstColumnValue}";
                commandtextStoredProcedureName = string.Empty;
                storedProcedureParameters = null;
            };

            //this method is also in use for DELETE sql statements
            //RunToUpdate(executeCurrentMethosProcedure: execute, CommandType.Text);
            Run(() => { UpdateInDb(); }, executeCurrentMethosProcedure: execute, CommandType.Text);
        }

        public override void DeleteAll()
        {            
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                commandTextForTextMode = $"DELETE FROM {this.GetTableName(typeof(T))}";
                commandtextStoredProcedureName = string.Empty;
                storedProcedureParameters = null;
            };

            //this method is also in use for DELETE sql statements
            //RunToUpdate(executeCurrentMethosProcedure: execute, CommandType.Text);
            Run(() => { UpdateInDb(); }, executeCurrentMethosProcedure: execute, CommandType.Text);
        }




        /// <summary>
        /// This method don't imlement the Template design pattern
        /// </summary>
        public virtual void DeleteAllNotRegardingForeignKeys()
        {
            try
            {
                _connection.Open();
                string tableName = this.GetTableName(typeof(T));
                _command.CommandType = CommandType.Text;
                bool isHaveForeignKey = false;
                string primaryKeyTableName = string.Empty;
                string foreignKeyTableName = string.Empty;
                foreach (var sDict in _dataAboutForeignKeysOfAllTables)
                {
                    sDict.TryGetValue("Primary_Key_Table", out primaryKeyTableName);
                    sDict.TryGetValue("Foreign_Key_Table", out foreignKeyTableName);
                    if (tableName.Equals(primaryKeyTableName))
                    {
                        isHaveForeignKey = true;
                        _command.CommandText = $"alter table {foreignKeyTableName} nocheck constraint all";
                        _command.ExecuteNonQuery();
                        break;
                    }

                }

                _command.CommandText = $"DELETE FROM {tableName}";
                _command.ExecuteNonQuery();

                if (isHaveForeignKey)
                {
                    _command.CommandText = $"alter table {foreignKeyTableName} check constraint all";
                    _command.ExecuteNonQuery();
                }
            }
            finally { _connection.Close(); }
        }

        /// <summary>
        /// Creates a new empty database with the same structure as the old, renames rhe old database and gives to the new DB the name of the old        
        /// </summary>        
        public async void SwapDataBases()
        {
            Func<Task> f = async () =>
            {
                SqlConnection connection = new SqlConnection($"Data Source={Environment.MachineName};Initial Catalog=master;Integrated Security=True");
                SqlCommand command = new SqlCommand();
                command.CommandType = CommandType.Text;
                command.Connection = connection;
                try
                {
                    connection.Open();                    
                    List<string> databases = await ConnectionStringManager.GetAllDataBases();
                    if (!databases.Contains("The_very_important_Flight_Center_Project_BACK"))
                    {
                        command.CommandText = @"
                                            ALTER DATABASE The_very_important_Flight_Center_Project SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                                            ALTER DATABASE The_very_important_Flight_Center_Project MODIFY NAME = The_very_important_Flight_Center_Project_BACK
                                            ALTER DATABASE [The_very_important_Flight_Center_Project_BACK] MODIFY FILE (NAME=N'The_very_important_Flight_Center_Project', NEWNAME=N'The_very_important_Flight_Center_Project_BACK')                                            

                                            exec sys.sp_detach_db The_very_important_Flight_Center_Project_BACK
                                            ";
                        command.ExecuteNonQuery();

                        string pathToTheBasesDirectory = @"C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\";
                        if (File.Exists($"{pathToTheBasesDirectory}The_very_important_Flight_Center_Project.mdf")) File.Move($"{pathToTheBasesDirectory}The_very_important_Flight_Center_Project.mdf", $"{pathToTheBasesDirectory}The_very_important_Flight_Center_Project_BACK.mdf");
                        //if (File.Exists($"{pathToTheBasesDirectory}The_very_important_Flight_Center_Project_log.ldf")) File.Move($"{pathToTheBasesDirectory}The_very_important_Flight_Center_Project_log.ldf", $"{pathToTheBasesDirectory}The_very_important_Flight_Center_Project_BACK_log.ldf");
                        command.CommandText = @"
                                            exec sys.sp_attach_db    The_very_important_Flight_Center_Project_BACK,    'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\The_very_important_Flight_Center_Project_BACK.mdf'   
                                            ALTER DATABASE The_very_important_Flight_Center_Project_BACK SET MULTI_USER

                                              ";
                        command.ExecuteNonQuery();


                    }
                    else
                    {
                        command.CommandText = @"
                                             
                                             
                                             ALTER DATABASE The_very_important_Flight_Center_Project_BACK SET MULTI_USER
                                             ALTER DATABASE [The_very_important_Flight_Center_Project_BACK] MODIFY FILE (NAME=N'The_very_important_Flight_Center_Project_BACK', NEWNAME=N'The_very_important_Flight_Center_Project') 
                                             ALTER DATABASE The_very_important_Flight_Center_Project_BACK MODIFY NAME = The_very_important_Flight_Center_Project                                             
                                                ";
                        command.ExecuteNonQuery();
                        /* 
                         //SQL code for attaching
                          
                         */
                    }

                    /*_command.CommandText = "ALTER DATABASE [The_very_important_Flight_Center_Project_BACK] MODIFY FILE (NAME=N'The_very_important_Flight_Center_Project_BACK', NEWNAME=N'The_very_important_Flight_Center_Project')";
                    _command.ExecuteNonQuery();*/
                    /*_command.CommandText = @"
                                            ALTER DATABASE The_very_important_Flight_Center_Project_BACK SET MULTI_USER
                                            ALTER DATABASE The_very_important_Flight_Center_Project_BACK MODIFY NAME = The_very_important_Flight_Center_Project
                                            ";
                    _command.ExecuteNonQuery();*/





                    //ConnectionStringManager.CreateNewDatabase("The_very_important_Flight_Center_Project", _command);


                    /*SET SINGLE_USER 
                    WITH ROLLBACK IMMEDIATE

                    MODIFY NAME = The_very_important_Flight_Center_Project_BACK

                    ALTER DATABASE The_very_important_Flight_Center_Project_BACK SET MULTI_USER 

                    ";*/



                    //var tablesInDB = ConnectionStringManager.GetAllDataBases();
                    //var r = tablesInDB;





                }
                finally { connection.Close(); }
                bool isFileCreated =  await ConnectionStringManager.WriteConnectionStringToFile(proposedDataBaseName: "The_very_important_Flight_Center_Project_BACK", defaultDataBaseName: "The_very_important_Flight_Center_Project");

                MessageBox.Show(isFileCreated.ToString());                               
            };
            await ProcessExceptionsVoidAsync(f);
            
        }

        public override void Remove(long ID)
        {            
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));
                string firstColumnName = "ID";
                object firstColumnValue = ID;
                commandTextForTextMode = $"DELETE FROM {tableName} WHERE {firstColumnName} = {firstColumnValue}";
                commandtextStoredProcedureName = string.Empty;
                storedProcedureParameters = null;
            };

            //this method is also in use for DELETE sql statements
            //RunToUpdate(execute, CommandType.Text);
            Run(() => { UpdateInDb(); }, execute, CommandType.Text);
        }

        public override void Update(T poco)
        {
            var propInfos = typeof(T).GetProperties().OrderBy(x => x.MetadataToken).ToArray();
            string firstColumnName = typeof(T).GetProperty("ID").Name.ToUpper();
            object firstColumnValue = typeof(T).GetProperty("ID").GetValue(poco);


            for (int i = 0; i < typeof(T).GetProperties().Length; i++)
            {
                if (propInfos[i].Name.Equals("ID".ToUpper())) continue;
                UpdateOneRowInternal(propInfos, firstColumnName, firstColumnValue, poco, i);
            }
        }

        public override void UpdateOneRow(int propertyPlaceNumber, T poco)
        {
            var propInfos = typeof(T).GetProperties().OrderBy(x => x.MetadataToken).ToArray();
            string firstColumnName = typeof(T).GetProperty("ID").Name.ToUpper();
            object firstColumnValue = typeof(T).GetProperty("ID").GetValue(poco);

            UpdateOneRowInternal(propInfos, firstColumnName, firstColumnValue, poco, propertyPlaceNumber);
        }

        private void UpdateOneRowInternal(PropertyInfo[] propInfos, string firstColumnName, object firstColumnValue, T poco, int propertyPlaceNumber)
        {
            ExecuteCurrentMethosProcedure execute = (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));

                var value = propInfos[propertyPlaceNumber].GetValue(poco);

                if (value is String || value is DateTime)
                {
                    if (value is DateTime)
                        value = ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");

                    if (value == null) value = String.Empty;
                    else value = $"'{value}'";
                }
                if (propInfos[propertyPlaceNumber].GetValue(poco).GetType().Name.ToLower().Contains("int") && value == null) value = 0;
                if (propInfos[propertyPlaceNumber].GetValue(poco).GetType().Name == "DateTime" && value == null) value = DateTime.MinValue;

                commandTextForTextMode = $"UPDATE {tableName} SET {propInfos[propertyPlaceNumber].Name} = {value} WHERE {firstColumnName} = {firstColumnValue}";
                commandtextStoredProcedureName = string.Empty;
                storedProcedureParameters = null;
            };

            Run(() => { UpdateInDb(); }, execute, CommandType.Text);
        }

        /// <summary>
        /// This method don't implement the Template design pattern yet          
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="propertyNumber"></param>
        /// <returns></returns>
        public override List<T> GetManyBySomethingInternal(object identifier, int propertyNumber)
        {
            try
            {
                _connection.Open();
                List<T> manyOfSomething = new List<T>();
                string tableName = this.GetTableName(typeof(T));
                string relevantColumnName = typeof(T).GetProperties()[propertyNumber].Name;
                string identifierInQMarks = string.Empty;
                if (identifier.GetType().Name == "String") identifierInQMarks = $"'{identifier}'";
                else identifierInQMarks = identifier.ToString();
                //_command.CommandText = $"SELECT * FROM {tableName} WHERE {relevantColumnName} = {identifierInQMarks}";

                _command.Parameters.Clear();
                _command.CommandText = "DAO_BASE_GetManyBySomethingInternal_METHOD_QUERY";
                _command.Parameters.AddWithValue("TABLE_NAME", tableName);
                _command.Parameters.AddWithValue("RELEVANT_COLUMN_NAME", relevantColumnName);
                _command.Parameters.AddWithValue("COLUMN_IDENTIFIER", identifierInQMarks);

                using (SqlDataReader reader = _command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        T something = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            typeof(T).GetProperties()[i].SetValue(something, reader.GetValue(i));
                        }
                        manyOfSomething.Add(something);
                    }
                }
                return manyOfSomething;

            }
            finally { _connection.Close(); }
        }

        public List<Tinternal> PreloadAll<Tinternal>(string whatExactlyPreaload)
        {
            string preloadColumnName = typeof(T).GetProperties().Where(x => x.Name.Contains(whatExactlyPreaload)).FirstOrDefault().Name;
            ExecuteCurrentMethosProcedure execute = CreateDelegateForPreloadAllInternal();

            List<T> pocosWithOnlyNames = Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
            return pocosWithOnlyNames.Select(x => (Tinternal)typeof(T).GetProperty(preloadColumnName).GetValue(x)).ToList();
        }
        public async Task<List<Tinternal>> PreloadAllAsync<Tinternal>(string whatExactlyPreaload)
        {
            string preloadColumnName = typeof(T).GetProperties().Where(x => x.Name.Contains(whatExactlyPreaload)).FirstOrDefault().Name;
            ExecuteCurrentMethosProcedure execute = CreateDelegateForPreloadAllInternal();

            List<T> pocosWithOnlyNames = await RunAsync(() => { return ReadFromDbAsync(); }, executeCurrentMethosProcedure: execute, commandType: CommandType.Text);
            return pocosWithOnlyNames.Select(x => (Tinternal)typeof(T).GetProperty(preloadColumnName).GetValue(x)).ToList();
        }
        private ExecuteCurrentMethosProcedure CreateDelegateForPreloadAllInternal()
        {
            return (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                string tableName = this.GetTableName(typeof(T));
                commandTextForTextMode = $"SELECT * FROM {tableName}";
                commandtextStoredProcedureName = "";
                storedProcedureParameters = null;
            };
        }

        #endregion Public service methods


    }

    

}
