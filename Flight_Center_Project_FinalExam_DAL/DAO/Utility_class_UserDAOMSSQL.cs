using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Center_Project_FinalExam_DAL
{
    public class Utility_class_UserDAOMSSQL<T> : UserBaseMSSQLDAO<T>, IUtility_class_UserDAO<T> where T : Utility_class_User, IPoco, new()
    {
        public Utility_class_UserDAOMSSQL() : base() { }

        private ExecuteCurrentMethosProcedure CreateDelegateForGetAllByUserKind(string userRole)
        {
            return (out string commandtextStoredProcedureName, out string commandTextForTextMode, out Dictionary<string, object> storedProcedureParameters) =>
            {
                commandTextForTextMode = $"select * from {GetTableName(typeof(T))} WHERE USER_KIND = '{userRole}'";
                commandtextStoredProcedureName = "DAO_BASE_GetAllByUserKind_METHOD_QUERY"; //this stored procedure isn't exist yet

                storedProcedureParameters = new Dictionary<string, object>();
                storedProcedureParameters.Add("TABLE_NAME", GetTableName(typeof(T)));
            };
        }

        public List<T> GetAllByUserKind(string userRole)
        {
            ExecuteCurrentMethosProcedure utility_class_UserExecute = CreateDelegateForGetAllByUserKind(userRole);
            return Run(() => { return ReadFromDb(); }, executeCurrentMethosProcedure: utility_class_UserExecute, commandType: CommandType.Text);
        }

        public Task<List<T>> GetAllByUserKindAsync(string userRole)
        {
            ExecuteCurrentMethosProcedure utility_class_UserExecute = CreateDelegateForGetAllByUserKind(userRole);
            return RunAsync(() => { return ReadFromDbAsync(); }, executeCurrentMethosProcedure: utility_class_UserExecute, commandType: CommandType.Text);
        }

        public T GetUserByIdentifier2(IPoco anotherTypePoco)
        {
            List<T> users = new List<T>();
            users.Add(base.GetRegisteredUserInOneTableBySomethingInAnotherInternal(anotherTypePoco.GetType().GetProperty("USER_ID").GetValue(anotherTypePoco), (int)CustomerPropertyNumber.USER_ID, typeof(Customer)));
            users.Add(base.GetRegisteredUserInOneTableBySomethingInAnotherInternal(anotherTypePoco.GetType().GetProperty("USER_ID").GetValue(anotherTypePoco), (int)AirlineCompanyPropertyNumber.USER_ID, typeof(AirlineCompany)));
            users.Add(base.GetRegisteredUserInOneTableBySomethingInAnotherInternal(anotherTypePoco.GetType().GetProperty("USER_ID").GetValue(anotherTypePoco), (int)AdministratorPropertyNumber.USER_ID, typeof(Administrator)));
            /*users.Add(base.GetSomethingBySomethingInternal(anotherTypePoco.GetType().GetProperties()[0].GetValue(anotherTypePoco), (int)Utility_class_UserPropertyNumber.AIRLINE_ID));
            users.Add(base.GetSomethingBySomethingInternal(anotherTypePoco.GetType().GetProperties()[0].GetValue(anotherTypePoco), (int)Utility_class_UserPropertyNumber.CUSTOMER_ID));
            users.Add(base.GetSomethingBySomethingInternal(anotherTypePoco.GetType().GetProperties()[0].GetValue(anotherTypePoco), (int)Utility_class_UserPropertyNumber.ADMINISTRATOR_ID));*/

            T toReturn = null;
            foreach(var s in users)
                if (s != new T()) toReturn = s;
            
            return toReturn;
        }
        public T GetUserByIdentifier(IPoco anotherTypePoco)
        {
            return GetRegisteredUserInOneTableBySomethingInAnotherInternal(anotherTypePoco.GetType().GetProperty("USER_ID").GetValue(anotherTypePoco), anotherTypePoco.GetType().GetProperties().Length - 1, anotherTypePoco.GetType());
        }

    }
}
