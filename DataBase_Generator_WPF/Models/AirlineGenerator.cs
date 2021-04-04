using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBase_Generator_WPF
{
    static class AirlineGenerator
    {
        static List<Dictionary<string, string>> _listOfAirlines = new List<Dictionary<string, string>>();
        
        static Stream _rawCSVairlinesData = Statics.GetStreamFromUrl("https://raw.githubusercontent.com/jpatokal/openflights/master/data/airlines.dat");

        /// <summary>
        /// This public property revels a list of a data pieces, such as the airline company name, the country name,
        /// and an "adorning", some string comprised of some letters and numbers, to be further used in creating an AirlineCompany objects,
        /// imn the form of a "Dictionary<string, string>", where a key is constant to each airline company and provides a name of the spesific data piece, 
        /// and the value is the actual data.
        /// </summary>
        static public List<Dictionary<string, string>> Airlines
        {
            get => _listOfAirlines;
        }

        static AirlineGenerator()
        {
            _listOfAirlines = MakeListOfAirlines(_rawCSVairlinesData);
        }

        static private List<Dictionary<string, string>> MakeListOfAirlines(Stream rawCSVairlinesData)
        {
            List<Dictionary<string, string>> listDicts = new List<Dictionary<string, string>>();
            using (TextFieldParser parser = new TextFieldParser(rawCSVairlinesData))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>();
                    //Process row
                    string[] fields = parser.ReadFields();
                    int count = 0;
                    string adorning = string.Empty;
                    foreach (string field in fields)
                    {
                        adorning = string.Empty;
                        if (count == 1) dict.Add("name", field);
                        if (count == 6)
                        {
                            if (field.Equals(@"\N")) dict.Add("country", "unknown_country");
                            else if (field.Equals(string.Empty)) dict.Add("country", "private_flight");
                            else dict.Add("country", field);
                        }
                        
                        if(count == 3)
                        {
                            if (!String.IsNullOrEmpty(field)) adorning = field + "-";
                        }
                        if (count == 4) { adorning += field; dict.Add("adorning", adorning); }

                        

                        //TODO: Process field
                        count++;
                    }
                    listDicts.Add(dict);
                }
            }
            return listDicts;
        }




    }
}
