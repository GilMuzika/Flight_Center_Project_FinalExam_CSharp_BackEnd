using Flight_Center_Project_FinalExam_DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBase_Generator_WPF.Models
{
    class FillEmptyInExisting<T> where T : class, IPoco,  new()
    {
        private List<T> allItems;
        private DBGenerator<T> _DBGenerator = new DBGenerator<T>();
        private Factory _itemsFactory;
        private List<Dictionary<string, string>> _airlineCompaniesFromExternalSource = AirlineGenerator.Airlines;

        private DAOCreatorFactory<T> _appropriateDAOcreator = new DAOCreatorFactory<T>();
        private IBasicDB<T> _appropriateDAO;
        private Random _rnd = new Random();
        private T _emptyPoco = new T();

        public FillEmptyInExisting()
        {
                _appropriateDAO = _appropriateDAOcreator.SelectAppropriateDAO();

            _itemsFactory = _DBGenerator.Factory;
        }

        public void FillEmpty()
        {
            allItems = _appropriateDAO.GetAll();

            Dictionary<Type, Func<T>> _typeItemCorrelation = new Dictionary<Type, Func<T>>();
            _typeItemCorrelation.Add(typeof(Administrator), () => { return _itemsFactory.CreateAdministrator() as T; });
            _typeItemCorrelation.Add(typeof(AirlineCompany), () => {
                int index = _rnd.Next(_airlineCompaniesFromExternalSource.Count -1);
                return _itemsFactory.CreateAirlineCompany(index) as T; 
            });
            _typeItemCorrelation.Add(typeof(Customer), () => { return _itemsFactory.CreateCustomer() as T; });
            _typeItemCorrelation.Add(typeof(Flight), () =>  _itemsFactory.CreateFlight() as T );
            _typeItemCorrelation.Add(typeof(Ticket), () => _itemsFactory.CreateTicket() as T);

            foreach(T s in allItems)
            {
                if(ishaveEmptyProperties(s))
                {
                    T item = _typeItemCorrelation[typeof(T)]();
                    PropertyInfo[] propres = typeof(T).GetProperties();
                    int count = 0;
                    foreach(PropertyInfo pi in propres)
                    {
                        if (pi.GetValue(s).Equals(pi.GetValue(_emptyPoco)))
                        {
                            pi.SetValue(s, pi.GetValue(item));
                            _appropriateDAO.UpdateOneRow(count, s);
                        }

                        count++;
                    }

                    //_appropriateDAO.Update(s);
                }

            }

        }

        private bool ishaveEmptyProperties(T poco)
        {
            PropertyInfo[] propers = typeof(T).GetProperties();
            foreach(PropertyInfo s in propers)
            {
                if (s.GetValue(poco).Equals(s.GetValue(_emptyPoco)))
                    return true;
            }

            return false;
        }


    }
}
