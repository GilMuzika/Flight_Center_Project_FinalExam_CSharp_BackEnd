using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;

namespace Flight_Center_Project_FinalExam_DAL
{
    public abstract class DaoBase<T> : IBasicDB<T> where T : class, IPoco, new()
    {
        #region Template methods
        /// <summary>
        /// Open the SQL connection
        /// </summary>
        protected abstract void OpenConnection();
        protected abstract Task OpenConnectionAsync();

        /// <summary>
        /// Close the SQL connection
        /// </summary>
        protected abstract void Close();

        /// <summary>
        /// Set the SQL command
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        /// <param name="storedProcedureParameters">Dictionary<string, string> that contains names and values of the stored proceture parameters</param>
        protected abstract void SetCommand(CommandType commandType, string commandText, Dictionary<string, object> storedProcedureParameters);

        /// <summary>
        /// Set the SQL command
        /// </summary>
        /// <param name="commandType"></param>
        /// <param name="commandText"></param>
        protected abstract void SetCommand(CommandType commandType, string commandText);        

        protected abstract List<T> ReadFromDb();

        protected abstract Task<List<T>> ReadFromDbAsync();

        protected abstract IEnumerable<T> ReadFromDbAsIEnumerable();

        protected abstract long AddToDb();

        protected abstract void UpdateInDb();


        /// <summary>
        /// "storedProcedureParameters" is a Dictionary<string, string> that contains parameter for stored procedure.
        /// If it null so the CommandType is "Text"
        /// This methos is optional. You can overload the Run with appropriate Action delegate instead.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTextOrStoredProcedureName"></param>
        /// <param name="storedProcedureParametersDict"></param>
        /// <returns></returns>
        protected List<T> RunToRead(ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            List<T> value = null;
            Action a = () =>
            {
                try
                {
                    OpenConnection();
                    executeCurrentMethosProcedure(out string commandTextStoredProdedureName, out string commandTextForTextMode, out Dictionary<string, Object> stroredProsedureParametersDict);
                    if (commandType == CommandType.Text) SetCommand(commandType, commandTextForTextMode);
                    else SetCommand(commandType, commandTextStoredProdedureName, stroredProsedureParametersDict);
                    value = ReadFromDb();
                }
                finally { Close(); }
            };
            ProcessExceptions(a);
            return value;
        }

        /// <summary>
        /// "ExecuteScalar" for extract a first column ("ID") value.
        /// This methos is optional. You can overload the Run with appropriate Action delegate instead.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTextOrStoredProcedureName"></param>
        /// <param name="storedProcedureParametersDict"></param>
        /// <returns></returns>
        protected long RunToAdd(ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            long value = -1;
            Action a = () =>
             {
                 try
                 {
                     OpenConnection();
                     executeCurrentMethosProcedure(out string commandTextStoredProdedureName, out string commandTextForTextMode, out Dictionary<string, Object> stroredProsedureParametersDict);
                     if (commandType == CommandType.Text) SetCommand(commandType, commandTextForTextMode);
                     else SetCommand(commandType, commandTextStoredProdedureName, stroredProsedureParametersDict);
                     value = AddToDb();
                 }
                 finally { Close(); }
            };
            ProcessExceptions(a);
            return value;
        }

        /// <summary>
        /// This methos is in use to execute UPDATE and also REMOVE sql statements.
        /// This methos is optional. You can overload the Run with appropriate Action delegate instead.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="commandType"></param>
        /// <param name="commandTextOrStoredProcedureName"></param>
        /// <param name="storedProcedureParametersDict"></param>
        protected void RunToUpdate(ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            Action a = () =>
            {
                try
                {
                    OpenConnection();
                    executeCurrentMethosProcedure(out string commandTextStoredProdedureName, out string commandTextForTextMode, out Dictionary<string, Object> stroredProsedureParametersDict);
                    if (commandType == CommandType.Text) SetCommand(commandType, commandTextForTextMode);
                    else SetCommand(commandType, commandTextStoredProdedureName, stroredProsedureParametersDict);
                    UpdateInDb();
                }
                finally { Close(); }
            };
            ProcessExceptions(a);
        }
        /// <summary>
        /// "ExecuteScalar" for extract a first column ("ID") value.
        /// This methos is optional. In this case, you can use "RunToRead" instead.
        /// </summary>
        /// <param name="AddToDb_method">Action delegate that encapsulates the "AddToDb" method</param>
        /// <param name="executeCurrentMethosProcedure">Delegate that encapsulates the changing part of the pattern</param>
        /// <param name="commandType">Type of SqlCommand, Text or Storedprocedure either</param>
        /// <returns></returns>
        protected long Run(Func<long> AddToDb_method, ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            //If the "ReadFromDb_method" and "UpdateInDb_method" parameters are null the method will execute "AddToDb_method" return "idValueAfterAdding". "payloadValueAfterGetting" will be null.
            //the underlying type of "out IEnumerable<T> payloadValueAfterGetting" is "List<T>", but this variable has no function in this method and is present only because of internal methos signature
            RunInternal(AddToDb_method, null, null, null, out long idValueAfterAdding, out IEnumerable<T> payloadValueAfterGetting, executeCurrentMethosProcedure, commandType);
            return idValueAfterAdding;
        }

        /// <summary>
        /// "storedProcedureParameters" is a Dictionary<string, string> that contains parameter for stored procedure.
        /// If it null so the CommandType is "Text".
        /// This methos is optional. In this case, you can use "RunToRead" instead.
        /// </summary>
        /// <param name="AddToDb_method">Action delegate that encapsulates the "ReadFromDb" method</param>
        /// <param name="executeCurrentMethosProcedure">Delegate that encapsulates the changing part of the pattern</param>
        /// <param name="commandType">Type of SqlCommand, Text or Storedprocedure either</param>
        /// <returns></returns>
        protected List<T> Run(Func<List<T>> ReadFromDb_method, ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            //If "AddToDb_method" and "UpdateInDb_method" are null "ReadFromDb_method" will be executed and "payloadValueAfterGetting" will be returned. "idValueAfterAdding" will be -1.
            RunInternal(null, ReadFromDb_method, null, null, out long idValueAfterAdding, out IEnumerable<T> payloadValueAfterGetting, executeCurrentMethosProcedure, commandType);
            return (List<T>)payloadValueAfterGetting;
        }
        protected async Task<List<T>> RunAsync(Func<Task<List<T>>> ReadFromDbAsync_method, ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            //If "AddToDb_method" and "UpdateInDb_method" are null "ReadFromDb_method" will be executed and "payloadValueAfterGetting" will be returned. "idValueAfterAdding" will be -1.
            return await RunInternalAsync(null, ReadFromDbAsync_method, null, null, executeCurrentMethosProcedure, commandType);
        }

        /// <summary>
        /// "storedProcedureParameters" is a Dictionary<string, string> that contains parameter for stored procedure.
        /// If it null so the CommandType is "Text".
        /// This methos is optional. In this case, you can use "RunToRead" instead.
        /// Uses "yield return" mechanism and returns IEnumetable<T>
        /// </summary>
        /// <param name="AddToDb_method">Action delegate that encapsulates the "ReadFromDb" method</param>
        /// <param name="executeCurrentMethosProcedure">Delegate that encapsulates the changing part of the pattern</param>
        /// <param name="commandType">Type of SqlCommand, Text or Storedprocedure either</param>
        /// <returns></returns>
        protected IEnumerable<T> Run(Func<IEnumerable<T>> ReadFromDb_methodAsIEnumerable, ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            //If "AddToDb_method" and "UpdateInDb_method" are null "ReadFromDb_method" will be executed and "payloadValueAfterGetting" will be returned. "idValueAfterAdding" will be -1.            
            RunInternal(null, null, ReadFromDb_methodAsIEnumerable, null, out long idValueAfterAdding, out IEnumerable<T> payloadValueAfterGetting, executeCurrentMethosProcedure, commandType);
            return payloadValueAfterGetting;
        }

        /// <summary>
        /// This methos is in use to execute UPDATE and also REMOVE sql statements.
        /// This methos is optional. In this case, you can use "RunToRead" instead.
        /// </summary>
        /// <param name="AddToDb_method">Action delegate that encapsulates the "UpdateInDb" method</param>
        /// <param name="executeCurrentMethosProcedure">Delegate that encapsulates the changing part of the pattern</param>
        /// <param name="commandType">Type of SqlCommand, Text or Storedprocedure either</param>
        protected void Run(Action UpdateInDb_method, ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            //If "AddToDb_method" and "ReadFromDb_method" are null, "UpdateInDb_method" will be executed. Since "UpdateInDb_method" is Action and don't return any value, so "idValueAfterAdding" and "payloadValueAfterGetting" will be their respective default values, -1 and null. 
            //This methos don't need to return any value.
            //the underlying type of "out IEnumerable<T> payloadValueAfterGetting" is "List<T>", but this variable has no function in this method and is present only because of internal methos signature
            RunInternal(null, null, null, UpdateInDb_method, out long idValueAfterAdding, out IEnumerable<T> payloadValueAfterGetting, executeCurrentMethosProcedure, commandType);
        }
        

        private void RunInternal(Func<long> AddToDb_method, Func<List<T>> ReadFromDb_method, Func<IEnumerable<T>> ReadFromDb_methodAsIEnumerable, Action UpdateInDb_method, out long idValueAfterAdding, out IEnumerable<T> payloadValueAfterGetting, ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {
            object funcReturnValue = null;            
            Func<object> f = () =>
            {
                try
                {
                    object returnValueInternal = null;

                    OpenConnection();
                    executeCurrentMethosProcedure(out string commandTextStoredProdedureName, out string commandTextForTextMode, out Dictionary<string, Object> stroredProsedureParametersDict);
                    if (commandType == CommandType.Text) SetCommand(commandType, commandTextForTextMode);
                    else SetCommand(commandType, commandTextStoredProdedureName, stroredProsedureParametersDict);

                    if (AddToDb_method != null) returnValueInternal = AddToDb_method();
                    if (ReadFromDb_method != null) returnValueInternal = ReadFromDb_method();
                    if (ReadFromDb_methodAsIEnumerable != null) returnValueInternal = ReadFromDb_methodAsIEnumerable();
                    if (UpdateInDb_method != null) UpdateInDb_method();

                    return returnValueInternal;
                }
                finally { Close(); }
                

            };
            funcReturnValue = ProcessExceptions(f);
            if (funcReturnValue is long) idValueAfterAdding = (long)funcReturnValue; else idValueAfterAdding = -1;
            if (funcReturnValue is List<T>) payloadValueAfterGetting = (List<T>)funcReturnValue; else payloadValueAfterGetting = null;
            if (funcReturnValue is IEnumerable<T>) payloadValueAfterGetting = (IEnumerable<T>)funcReturnValue; else payloadValueAfterGetting = null;

        }
        private async Task<List<T>> RunInternalAsync(Func<long> AddToDb_method, Func<Task<List<T>>> ReadFromDbAsync_method, Func<IEnumerable<T>> ReadFromDb_methodAsIEnumerable, Action UpdateInDb_method, ExecuteCurrentMethosProcedure executeCurrentMethosProcedure, CommandType commandType)
        {

            return await Task.Run(async () =>
            {

                List<T> funcReturnValue = null;
                Func<Task<List<T>>> f = async () =>
                {
                    try
                    {
                        List<T> returnValueInternal = null;
                        
                        await OpenConnectionAsync();
                        executeCurrentMethosProcedure(out string commandTextStoredProdedureName, out string commandTextForTextMode, out Dictionary<string, Object> stroredProsedureParametersDict);
                        if (commandType == CommandType.Text) SetCommand(commandType, commandTextForTextMode);
                        else SetCommand(commandType, commandTextStoredProdedureName, stroredProsedureParametersDict);

                        //if (AddToDb_method != null) returnValueInternal = AddToDb_method();
                        if (ReadFromDbAsync_method != null) returnValueInternal = await ReadFromDbAsync_method();
                        //if (ReadFromDb_methodAsIEnumerable != null) returnValueInternal = ReadFromDb_methodAsIEnumerable();
                        if (UpdateInDb_method != null) UpdateInDb_method();

                        return returnValueInternal;
                    }
                    finally { Close(); }
                };


                funcReturnValue = await ProcessExceptions(f);
                return funcReturnValue; ////////////////
            });

            /*if (funcReturnValue is long) idValueAfterAdding = (long)funcReturnValue; else idValueAfterAdding = -1;
            if (funcReturnValue is List<T>) payloadValueAfterGetting = (List<T>)funcReturnValue; else payloadValueAfterGetting = null;
            if (funcReturnValue is IEnumerable<T>) payloadValueAfterGetting = (IEnumerable<T>)funcReturnValue; else payloadValueAfterGetting = null;*/

        }

        #endregion Template methods

        #region abstract methods definitions

        public abstract long Add(T poco);

        public abstract T Get(long ID);

        public abstract List<T> GetAll();

        public abstract Task<List<T>> GetAllAsync();

        public abstract IEnumerable<T> GetAllASIEnumerable();

        public abstract void Remove(T poco);

        public abstract void Update(T poco);

        public abstract void UpdateOneRow(int propertyPlaceNumber, T poco);

        public abstract T GetSomethingBySomethingInternal(object identifier, int propertyNumber);

        public abstract Task<T> GetSomethingBySomethingInternalAsync(object identifier, int propertyNumber);

        public abstract void DeleteAll();

        public abstract void Remove(long ID);

        public abstract List<T> GetManyBySomethingInternal(object identifier, int propertyNumber);

        #endregion abstract methods definitions


        #region Additional auxiliary methods
        protected async Task<Tt> ProcessExceptions<Tt>(Func<Task<Tt>> func)
        {
            Tt returnValue = default(Tt);
            try
            {
                returnValue = await func();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
            }
            return returnValue;
        }
        protected void ProcessExceptions(Action act)
        {
            try
            {
                act();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
            }
        }
        protected async Task ProcessExceptionsVoidAsync(Func<Task> act)
        {
            try
            {
               await act();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
            }
        }
        protected object ProcessExceptions(Func<object> func)
        {
            object val = null;
            try
            {
                val = func();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
            }
            return val;
        }

        #endregion Additional auxiliary methods
    }
}
