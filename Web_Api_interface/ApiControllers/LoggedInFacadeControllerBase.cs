using Flight_Center_Project_FinalExam_BL;
using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Windows;

namespace Web_Api_interface.ApiControllers
{
    public abstract class LoggedInFacadeControllerBase: ApiController
    {
        protected FlyingCenterSystem _fsc = FlyingCenterSystem.GetInstance();

        protected bool GetInternalLoginTokenInternal<T>(out LoginToken<T> loginToken) where T : class, IPoco, new()
        {
            bool isAuthorized = false;
            LoginToken<T> loginTokenInternal = null;
            Action act = () =>
            {
                var identity = (ClaimsIdentity)User.Identity;
                var role = identity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).FirstOrDefault();
                var password = identity.Claims.Where(x => x.Type == "Password").Select(x => x.Value).FirstOrDefault();
                var username = identity.Claims.Where(x => x.Type == ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();

                LoginService<T> loginService = _fsc.GetLoginService(new T());
                isAuthorized = loginService.TryUserLogin(username, password, role, out LoginToken<T> loginTokenInternalInternal);
                loginTokenInternal = loginTokenInternalInternal;
            };
            ProcessExceptions(act);
            loginToken = loginTokenInternal;
            return isAuthorized;
        }
        protected async Task<LoginToken<T>> GetInternalLoginTokenInternalAsync<T>() where T : class, IPoco, new()
        {
            Func<Task<LoginToken<T>>> func = async () =>
            {
                var identity = (ClaimsIdentity)User.Identity;
                var role = identity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).FirstOrDefault();
                var password = identity.Claims.Where(x => x.Type == "Password").Select(x => x.Value).FirstOrDefault();
                var username = identity.Claims.Where(x => x.Type == ClaimTypes.Name).Select(x => x.Value).FirstOrDefault();

                LoginService<T> loginService = _fsc.GetLoginService(new T());
                return await loginService.TryUserLoginAsync(username, password, role);
            };
            return await ProcessExceptionsAsync(func);
        }


















        protected void ProcessExceptions(Action act)
        {
            try
            {
                act();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
            }
        }
        protected async Task<Tout> ProcessExceptionsAsync<Tout>(Func<Task<Tout>> funcAsync)
        {
            Task<Tout> retValTask = null;
            try
            {
                retValTask = funcAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.GetType().Name}\n\n{ex.Message}\n\n\n{ex.StackTrace}");
            }
            return await retValTask;
        }
    }
}