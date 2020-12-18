/*This project is supposed to get information from a file 
 * and display selected information according with options given*/

/*
 *Author: Caio Victor Goncalves
 *Date:2020-11-10
 */

using System;
using CsvHelper;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;
using System.IO;
using System.Globalization;

namespace Assignment3
{
    class Program
    {
        //Code Sorced From: <https://stackoverflow.com/questions/5237666/adding-text-decorations-to-console-output/5237949>
        //User: Kieran Foot
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        //----------------------------------------------------
        static void Main(string[] args)
        {
            //1.0 Display Title of the App, with Under Line, Version, Date, Your Name, Student ID, etc.
            WriteUnderline("\t\tAssignment 3 v1.0");
            Console.WriteLine("Date:\t\t" + DateTime.Now.ToString("M/d/yyyy"));
            Console.WriteLine("Name:\t\tCaio Victor Goncalves");
            Console.WriteLine("Student ID:\tA00088903");
            //

            //creates a of object and populates it with info from csv file
            List<cities> cities_list = new List<cities>();

            //read data form csv file
            //creates a path variable for the file's path            
            string path = "postalData.csv";

            //reads from the file
            using (var reader = new StreamReader(path))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<cities>();
                cities_list = records.ToList();
            }

            /*
             If you can add the sort option to the menu, that would be great. 
            After step 1 & before step 2, Display PROVINCE_ABBR 
            Sort ASC or DESC. Provide two options to type, if no choice provided, sort by ASC.
             */
            orderrequest(cities_list);

            string userprovince = "";
            userprovince = provincedisplay(cities_list, userprovince);

            Console.WriteLine();

            int skipnum = 0;

            string nextpage = "";

            //bottom of the page
            int numofpages;
            int currentpage = 1;
            int recordsleft = 0;
            int recordsinpage = 0;

            //loop for the pagination
            int increment = 0;
            bool control = false;

            int numberofrecords;

            while (control == false)
            {
                numberofrecords = dataforpages(cities_list, userprovince, skipnum);

                //number of pages
                if (numberofrecords > 50)
                {
                    numofpages = numberofrecords / 50;
                }
                else
                {
                    numofpages = 1;
                }

                //pagination info                
                if ((skipnum + 50) < numberofrecords)
                {
                    recordsinpage = skipnum + 50;
                    pagination(skipnum, numofpages, currentpage, recordsinpage);
                }
                else
                {                    
                    recordsinpage = numberofrecords;
                    pagination(skipnum, numofpages, currentpage, recordsinpage);
                    skipnum = 0;
                    currentpage = 1;
                    increment = 0;
                    //After reaching the last page, prompt the user to go to the first page or to Display all Dinstinct PROVINCE
                    orderrequest(cities_list);
                    userprovince = provincedisplay(cities_list, userprovince);
                    numberofrecords = dataforpages(cities_list, userprovince, skipnum);
                    if ((skipnum + 50) < numberofrecords)
                    {
                        recordsinpage = skipnum + 50;
                    }
                    else
                    {
                        recordsinpage = numberofrecords;
                    }
                        numofpages = numberofrecords / 50;
                    pagination(skipnum, numofpages, currentpage, recordsinpage);
                }

                //pagination(skipnum, numofpages, currentpage, recordsinpage);
                
                Console.ReadLine(); 
                increment += 50;
                currentpage += 1;
                skipnum += 50;
                Console.Clear();

            }

        }

        private static int dataforpages(List<cities> cities_list, string userprovince, int skipnum)
        {
            //number of results for the designed province
            var numberofrecords = cities_list.Where(x => x.PROVINCE_ABBR == userprovince.ToUpper()).Count();
            Console.WriteLine("Number of records: {0}", numberofrecords);

            //displays a limited amout of results for the desired province
            var page = (from item in cities_list
                        where item.PROVINCE_ABBR == userprovince.ToUpper()
                        select item)
                       //skip will skip the number of elements 
                       .Skip(skipnum)
                       //the 50 limits how many data is going to be shown
                       .Take(50);
            string header = String.Format("{0}| {1,30} | {2,15} | {3,15} | {4,15} | {5,15}|", "Postal Code", "City", "Province", "Time Zone", "Latitude", "Longitude");
            Console.WriteLine(header);
            foreach (var item in page)
            {
                Console.WriteLine(string.Format("{0,10} | {1,30} | {2,15} | {3,15} | {4,15} | {5,15}|", item.POSTAL_CODE, item.CITY, item.PROVINCE_ABBR, item.TIME_ZONE, item.LATITUDE, item.LONGITUDE));
            }

            return numberofrecords;
        }

        private static void orderrequest(List<cities> cities_list)
        {
            Console.WriteLine("\nType 'ASC' to sorte the provincesdesc in ascending order or 'DESC' to sort in descending order.");
            string order = " ";
            order = Console.ReadLine();
            if (order == "desc" || order == "DESC")
            {
                Console.WriteLine("\nList of provincesdesc: ");
                provincesdesc(cities_list);
            }
            else
            {
                Console.WriteLine("\nList of provincesdesc: ");
                provincesasc(cities_list);
            }
        }

        private static void pagination(int skipnum, int numofpages, int currentpage, int recordsinpage)
        {
            Console.WriteLine("Pages: " + currentpage + "/" + numofpages);
            Console.WriteLine("Showing records " + (skipnum + 1) + " to " + recordsinpage);

            Console.WriteLine("Press Any Key to Go to Next Page");
        }

        private static string provincedisplay(List<cities> cities_list, string userprovince)
        {
            bool condition = false;

            //keep looping if the user enter an invalid province
            while (condition == false)
            {
                Console.Write("\n\nType the province: ");

                //user input for province
                userprovince = Console.ReadLine().Substring(0, 2);

                //just for testing
                //Console.WriteLine(userprovince);

                if (!(userprovince == "nl" || userprovince == "Nl" || userprovince == "nL" || userprovince == "NL"
                    || userprovince == "ns" || userprovince == "Ns" || userprovince == "nS" || userprovince == "NS"
                    || userprovince == "pe" || userprovince == "Pe" || userprovince == "pE" || userprovince == "PE"
                    || userprovince == "nb" || userprovince == "Nb" || userprovince == "nB" || userprovince == "NB"
                    || userprovince == "qc" || userprovince == "Qc" || userprovince == "qC" || userprovince == "QC"
                    || userprovince == "on" || userprovince == "On" || userprovince == "oN" || userprovince == "ON"
                    || userprovince == "mb" || userprovince == "Mb" || userprovince == "mB" || userprovince == "MB"
                    || userprovince == "sk" || userprovince == "Sk" || userprovince == "sK" || userprovince == "SK"
                    || userprovince == "ab" || userprovince == "Ab" || userprovince == "aB" || userprovince == "AB"
                    || userprovince == "bc" || userprovince == "Bc" || userprovince == "bC" || userprovince == "BC"
                    || userprovince == "nu" || userprovince == "Nu" || userprovince == "nU" || userprovince == "NU"
                    || userprovince == "nt" || userprovince == "Nt" || userprovince == "nT" || userprovince == "NT"
                    || userprovince == "yt" || userprovince == "Yt" || userprovince == "yT" || userprovince == "YT"))
                {
                    Console.WriteLine("\nplease write one of the provincesdesc from the list below: ");
                    provincesdesc(cities_list);
                }
                else
                {
                    condition = true;
                }
            }

            return userprovince;
        }

        private static void provincesdesc(List<cities> cities_list)
        {
            //Display all Dinstinct PROVINCE_ABBR from the. CSV            
            var distinctProvinces = (from s in cities_list
                                     orderby s.PROVINCE_ABBR descending
                                     select s.PROVINCE_ABBR).Distinct();
            //var distinctProvinces = cities_list.Select(o => o.PROVINCE_ABBR).Distinct();
            foreach (var item in distinctProvinces)
            {
                //If you display PROVINCE_ABBR in two or more columns with, equal tab space gap, that would be great
                Console.Write("{0} ", item);
            }
        }

        private static void provincesasc(List<cities> cities_list)
        {
            //Display all Dinstinct PROVINCE_ABBR from the. CSV  in ascending order          
            var distinctProvinces = (from s in cities_list
                                     orderby s.PROVINCE_ABBR ascending
                                     select s.PROVINCE_ABBR).Distinct();
            //var distinctProvinces = cities_list.Select(o => o.PROVINCE_ABBR).Distinct();
            foreach (var item in distinctProvinces)
            {
                //If you display PROVINCE_ABBR in two or more columns with, equal tab space gap, that would be great
                Console.Write("{0} ", item);
            }
        }

        class cities
        {
            public string POSTAL_CODE { get; set; }
            public string CITY { get; set; }
            public string PROVINCE_ABBR { get; set; }
            public string TIME_ZONE { get; set; }
            public string LATITUDE { get; set; }
            public string LONGITUDE { get; set; }

            public override string ToString()
            {
                Console.WriteLine("-----------------------------------------------------------------------------------------------");
                return String.Format("Postal Code: {0} | City: {1} | Province: {2} | Time Zone: {3} | Latitude: {4} | Longitude: {5}|", POSTAL_CODE, CITY, PROVINCE_ABBR, TIME_ZONE, LATITUDE, LONGITUDE);
            }
        }

        //Code Sorced From: <https://stackoverflow.com/questions/5237666/adding-text-decorations-to-console-output/5237949>
        //User: Kieran Foot
        private static void WriteUnderline(string s)
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            uint mode;
            GetConsoleMode(handle, out mode);
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;
            SetConsoleMode(handle, mode);
            Console.WriteLine($"\x1B[4m{s}\x1B[24m");
        }
        //----------------------------------------------------------------------
    }
}
