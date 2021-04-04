using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Web_Api_interface.Comparers
{
    /// <summary>
    /// compares two classes of the same type by string-valued proprty of this type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ComparerByStringValuedProperty<T> : IComparer<T>
    {
        private string _properyName;

        private string _propertyValue1;
        private string _propertyValue2;


        /// <summary>
        /// Takes the name of the property by wich the compasion should be done
        /// </summary>
        /// <param name="properyName">Name of the property by wich the compasion should be done</param>
        public ComparerByStringValuedProperty(string properyName)
        {
            _properyName = properyName;           
        }     

        public int Compare(T x, T y) 
        {
            PropertyInfo propInfo = typeof(T)?.GetProperty(_properyName);
            if (propInfo == null)
                throw new ArgumentException($"The type \"{typeof(T).Name}\" doesn't have any property named \"{_properyName}\"");            


            if(propInfo.PropertyType.Name != "String")
                throw new ArgumentException($"The property \"{_properyName}\" must be of the type \"String\"! Its actual type is {propInfo.PropertyType.Name}");


            var propertyValue1 = propInfo.GetValue(x);
            var propertyValue2 = propInfo.GetValue(y);


            _propertyValue1 = (string)propertyValue1;
            _propertyValue2 = (string)propertyValue2;



            if (ReferenceEquals(_propertyValue1, _propertyValue2))
                return 0;
            else
            {
                int retVal = CompareCharsNumericReprsentation(x, y);
                if(retVal == 0 && _propertyValue1.Length != _propertyValue2.Length)
                {
                    if (_propertyValue1.Length > _propertyValue2.Length)
                         return 1;
                    else return -1;
                }

                return retVal;

            }
        }

        private int CompareCharsNumericReprsentation(T x, T y)
        {
            int strLenght = 0;
            if (_propertyValue1.Length <= _propertyValue2.Length) 
                 strLenght = _propertyValue1.Length;
            else strLenght = _propertyValue2.Length;

            for (int i = 0; i < strLenght; i++)
            {
                if ((int)_propertyValue1[i] != (int)_propertyValue2[i])
                {
                    if ((int)_propertyValue1[i] > (int)_propertyValue2[i])
                        return 1;

                    if ((int)_propertyValue1[i] < (int)_propertyValue2[i])
                        return -1;
                }
            }

            return 0;
        }


        public override string ToString()
        {
            return $"[current comparsion by a property named \"{_properyName}\", type \"String\"]";
        }


    }
}
