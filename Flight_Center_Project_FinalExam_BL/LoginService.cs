using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Flight_Center_Project_FinalExam_BL
{

    public class LoginService<T> : ILoginService<T> where T : class, IPoco, new()
    {
        private UserBaseMSSQLDAO<T> _currentUserBaseMSSQLDAO = new UserBaseMSSQLDAO<T>();
        private Utility_class_UserDAOMSSQL<Utility_class_User> _currentUtility_Class_UserDAOMSSQL = new Utility_class_UserDAOMSSQL<Utility_class_User>();
        private AdministratorDAOMSSQL<Administrator> _currentAdministratorDAOMSSQL = new AdministratorDAOMSSQL<Administrator>();
        private CustomerDAOMSSQL<Customer> _currentCustomerDAOMSSQL = new CustomerDAOMSSQL<Customer>();
        private AirlineDAOMSSQL<AirlineCompany> _currentAirlineDAOMSSQL = new AirlineDAOMSSQL<AirlineCompany>();

        
        private LoginToken<T> TryUserLoginCommon(string userName, string password, List<Utility_class_User> allTheusers, out bool isUserExistsExternal)
        {
            string real_userName = string.Empty;
            string real_password = string.Empty;
            if (userName.Length < 50) real_userName = userName;
            else real_userName = EncryptionProvider.Decryprt(userName);
            if (password.Length < 50) real_password = password;
            else real_password = EncryptionProvider.Decryprt(password);

            bool isUserExists = false;
            LoginToken<T> loginToken = null;
            //List<Utility_class_User> allTheusers = _currentUtility_Class_UserDAOMSSQL.GetAll();

            Dictionary<string, Func<Utility_class_User, IPoco>> correlation = new Dictionary<string, Func<Utility_class_User, IPoco>>();
            correlation.Add(typeof(Administrator).Name, (utility_user) => { return _currentAdministratorDAOMSSQL.GetAdministratorByUserID(utility_user.ID); });
            correlation.Add(typeof(Customer).Name, (utility_user) => { return _currentCustomerDAOMSSQL.GetCustomerByUserID(utility_user.ID); });
            correlation.Add(typeof(AirlineCompany).Name, (utility_user) => { return _currentAirlineDAOMSSQL.GerAirlineCompanyByUserID(utility_user.ID); });
            foreach (var s in allTheusers)
            {
                string s_username = string.Empty;
                if (s.USER_NAME.Length < 50) s_username = s.USER_NAME;
                else s_username = EncryptionProvider.Decryprt(s.USER_NAME);
                if (real_userName == s_username)
                {
                    isUserExists = true;
                    string s_password = string.Empty;
                    if (s.PASSWORD.Length < 50) s_password = s.PASSWORD;
                    else s_password = EncryptionProvider.Decryprt(s.PASSWORD);
                    if (real_password == s_password)
                    {
                        var actualUser = correlation[s.USER_KIND](s);
                        loginToken = new LoginToken<T>();
                        loginToken.ActualUser = actualUser as T;
                        loginToken.UserAsUser = s;

                        break;
                    }
                    else throw new WrongPasswordException(password);
                }
            }
            if (!isUserExists) throw new UserNotFoundException(userName);
            isUserExistsExternal = isUserExists;
            return loginToken;
        }

        public bool TryUserLogin(string userName, string password, string userRole, out LoginToken<T> token)
        {
            bool isUserExists = TryUserLoginInternal(userName, password, userRole, out LoginToken<T> tokenInternal);
            token = tokenInternal;
            return isUserExists;
        }
        public bool TryUserLogin(string userName, string password, out LoginToken<T> token)
        {
           bool isUserExists = TryUserLoginInternal(userName, password, null, out LoginToken<T> tokenInternal);
           token = tokenInternal;
           return isUserExists;
        }
        private bool TryUserLoginInternal(string userName, string password, string userRole, out LoginToken<T> token)
        {
            List<Utility_class_User> allTheusers = null;
            if (userRole != null)
                allTheusers = _currentUtility_Class_UserDAOMSSQL.GetAllByUserKind(userRole);
            else
                allTheusers = _currentUtility_Class_UserDAOMSSQL.GetAll();

            token = TryUserLoginCommon(userName, password, allTheusers, out bool isUserExists);
            return isUserExists;
        }

        public async Task<LoginToken<T>> TryUserLoginAsync(string userName, string password, string userRole)
        {
            return await TryUserLoginAsyncInternal(userName, password, userRole);
        }
        public async Task<LoginToken<T>> TryUserLoginAsync(string userName, string password)
        {
            return await TryUserLoginAsyncInternal(userName, password, null);
        }
        private async Task<LoginToken<T>> TryUserLoginAsyncInternal(string userName, string password, string userRole)
        {
            List<Utility_class_User> allTheusers = null;
            if(userRole != null)
                allTheusers = await _currentUtility_Class_UserDAOMSSQL.GetAllByUserKindAsync(userRole);
            else 
                allTheusers = await _currentUtility_Class_UserDAOMSSQL.GetAllAsync();

            return TryUserLoginCommon(userName, password, allTheusers, out bool isuserExists);
            //return isUserExists;
        }

    }
}
