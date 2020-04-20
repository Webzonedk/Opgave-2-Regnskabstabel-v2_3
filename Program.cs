using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;

namespace Opgave_2_Regnskabstabel_v2_3
{
    class Program
    {
        static void Main(string[] args)
        {



            //Creating a simple list containing the two dataset files
            List<string> files = new List<string>
            {
                "Omsætningstal afd A.txt",
                "Omsætningstal afd B.txt",
                "Testfil.txt"
            };

            //Name of the file that data is being written to
            string outputFile = "Dataset til regnskabsafdelingen.txt";

            //Splitting up the content so first runthrough isWriting everything to a file.
            //Secund runthrough is writing to the console.
            FileStream dataOutput;
            StreamWriter fileWriter;
            TextWriter consoleOut = Console.Out;
            try
            {
                dataOutput = new FileStream($"{outputFile}", FileMode.OpenOrCreate, FileAccess.Write);//trying to open the file
                fileWriter = new StreamWriter(dataOutput);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open the output-file for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(fileWriter);//From here the text will be written to a file instead of the console

            #region Here data is being written to file

            for (int file = 0; file < files.Count; file++)//Runs through the number og files in the file list. called this for file instead of "i" because i added it late in the process
            {
                string filePath = files[file];

                decimal vat = 125;
                decimal multiplier = 100;
                decimal dividend = 100;

                //**************************************************************************************
                //Inserting all lines into the list rawData
                //**************************************************************************************
                List<string> rawData = new List<string>();//represent the list with unfiltered data
                List<string> rawDataClean = new List<string>();//Represents the list with data after it has been checked
                List<string> rawDataErrors = new List<string>();//Represents the list with data that contains errors
                //**************************************************************************************


                string dataLine;//variable declared to input content from readline.

                //Reading each lines in the file, and putting it into the two lists rawData and rawDataClean
                using (StreamReader textFile = new StreamReader(filePath))
                {
                    while ((dataLine = textFile.ReadLine()) != null)
                    {
                        rawData.Add(dataLine);
                        rawDataClean.Add(dataLine);
                    }
                }



                //**************************************************************************************
                //Checking for errors: amount of dividers, in date, if company name is empty, and turnover if its a valid decimal number.
                //adding the string lines to the rawDataErrors list and removes the lines from rawDataClean list
                //the errorcount is to be sure that the right lines are being removed from the rawDataClean list
                //**************************************************************************************
                int errorCount = 0;
                for (int i = 0; i < rawData.Count; i++)
                {
                    string divider = ";";
                    int dividerCount = (rawData[i].Length - rawData[i].Replace(divider, "").Length) / divider.Length;//counts the amount og dividers
                    string date = rawData[i].Substring(0, 10);
                    int companyNameStart = rawData[i].IndexOf(";");/* + ";".Length;*/
                    int companyNameEnd = rawData[i].LastIndexOf(";");
                    string turnOver = rawData[i].Substring(rawData[i].LastIndexOf(';') + 1);
                    Regex allowedTurnOver = new Regex(@"^(\d*\,)?\d+$");//making a check to ensure that it is a decimal number (TryParse does not work for this check as it allows all kind of dividers and decimals also mixed together)

                    if (dividerCount < 2)
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by too few dividers!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (dividerCount > 2)
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by too many dividers!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (!DateTime.TryParse(date, out _))
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by error in the date!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (companyNameStart >= companyNameEnd)
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by error in the company name!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (!allowedTurnOver.IsMatch(turnOver))
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by error in the turnover result. We couldn't convert it to decimal number!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                }



                //**************************************************************************************
                //Splitting data into 5 different lists with variating datatypes
                //**************************************************************************************
                for (int i = 0; i < rawDataClean.Count; i++)
                {
                    string[] splitValue = rawDataClean[i].Split(';'); //Creates a string array. Alternative Visual studio got angry unless i called it for "var" and teachers dont like that
                    for (int j = 0; j < splitValue.Length; j++)
                    {
                        int convertInput = j % 3;
                        switch (convertInput)
                        {
                            case 0:
                                DateTime parsedDate = DateTime.Parse(splitValue[j]);
                                DataSetLists.Date.Add(parsedDate);
                                break;

                            case 1:
                                DataSetLists.Company.Add(splitValue[j]);
                                break;

                            default:

                                decimal parsedNumber = Decimal.Parse(splitValue[j]);
                                DataSetLists.Turnover.Add(parsedNumber);
                                decimal turnoverVatIncluded = parsedNumber * (vat / dividend);// Calculating Turnover including VAT
                                DataSetLists.TurnoverVatIncluded.Add(turnoverVatIncluded);
                                decimal turnOverVatIncludedRound = Math.Round(turnoverVatIncluded, 2, MidpointRounding.AwayFromZero);
                                DataSetLists.TurnoverVatIncludedRound.Add(turnOverVatIncludedRound);
                                break;
                        }
                    }
                }


                //**************************************************************************************
                //Combining the 5 different lists to a single list containing 4 ddifferent datatypes defined in my custom class DataSet
                //**************************************************************************************
                List<DataSet> dataSet = new List<DataSet>();
                for (int i = 0; i < DataSetLists.Date.Count; i++)
                {
                    dataSet.Add(new DataSet
                    {
                        Date = DataSetLists.Date[i],
                        Company = DataSetLists.Company[i],
                        Turnover = DataSetLists.Turnover[i],
                        TurnoverVatIncluded = DataSetLists.TurnoverVatIncluded[i],
                        TurnoverVatIncludedRound = DataSetLists.TurnoverVatIncludedRound[i]
                    });
                }


                //**************************************************************************************
                //Adding all company names to a new list to be able to remove all duplicates
                //**************************************************************************************
                List<string> unicCompanyNames = new List<string>();

                for (int i = 0; i < DataSetLists.Company.Count; i++)
                {
                    unicCompanyNames.Add(DataSetLists.Company[i]);
                }


                //**************************************************************************************
                //Removes all duplicates so all names are unic in the list. Created to be able to sort numbers under each company
                //**************************************************************************************
                unicCompanyNames = unicCompanyNames.Distinct().ToList();
                unicCompanyNames.Sort();


                //**************************************************************************************
                //adding row and line number based on company name and month number
                //**************************************************************************************
                for (int i = 0; i < dataSet.Count; i++)
                {
                    dataSet[i].MonthNumber = dataSet[i].Date.Month;
                    string name = dataSet[i].Company;
                    for (int j = 0; j < unicCompanyNames.Count; j++)
                    {
                        if (name == unicCompanyNames[j])
                        {
                            dataSet[i].CompanyNumber = j;
                        }

                    }

                }


                //**************************************************************************************
                //Vriting data (compNumber, name and monthnumber) into multiple lists to be able to filter
                //data by month and merge them together in the custom list afterwards
                //**************************************************************************************
                for (int i = 0; i < dataSet.Count; i++)
                {
                    string name = dataSet[i].Company;
                    int monthNumber = dataSet[i].MonthNumber;
                    int companyNumber = dataSet[i].CompanyNumber;

                    ResultLists.CompNr.Add(companyNumber);
                    ResultLists.CompName.Add(name);
                    ResultLists.MonthNr.Add(monthNumber);
                    ResultLists.Jan.Add(0);
                    ResultLists.JanTotal.Add(0);
                    ResultLists.Feb.Add(0);
                    ResultLists.FebTotal.Add(0);
                    ResultLists.Mar.Add(0);
                    ResultLists.MarTotal.Add(0);
                    ResultLists.Apr.Add(0);
                    ResultLists.AprTotal.Add(0);
                    ResultLists.May.Add(0);
                    ResultLists.MayTotal.Add(0);
                    ResultLists.Jun.Add(0);
                    ResultLists.JunTotal.Add(0);
                    ResultLists.Jul.Add(0);
                    ResultLists.JulTotal.Add(0);
                    ResultLists.Aug.Add(0);
                    ResultLists.AugTotal.Add(0);
                    ResultLists.Sep.Add(0);
                    ResultLists.SepTotal.Add(0);
                    ResultLists.Oct.Add(0);
                    ResultLists.OctTotal.Add(0);
                    ResultLists.Nov.Add(0);
                    ResultLists.NovTotal.Add(0);
                    ResultLists.Dec.Add(0);
                    ResultLists.DecTotal.Add(0);
                    ResultLists.JanRound.Add(0);
                    ResultLists.JanRoundTotal.Add(0);
                    ResultLists.FebRound.Add(0);
                    ResultLists.FebRoundTotal.Add(0);
                    ResultLists.MarRound.Add(0);
                    ResultLists.MarRoundTotal.Add(0);
                    ResultLists.AprRound.Add(0);
                    ResultLists.AprRoundTotal.Add(0);
                    ResultLists.MayRound.Add(0);
                    ResultLists.MayRoundTotal.Add(0);
                    ResultLists.JunRound.Add(0);
                    ResultLists.JunRoundTotal.Add(0);
                    ResultLists.JulRound.Add(0);
                    ResultLists.JulRoundTotal.Add(0);
                    ResultLists.AugRound.Add(0);
                    ResultLists.AugRoundTotal.Add(0);
                    ResultLists.SepRound.Add(0);
                    ResultLists.SepRoundTotal.Add(0);
                    ResultLists.OctRound.Add(0);
                    ResultLists.OctRoundTotal.Add(0);
                    ResultLists.NovRound.Add(0);
                    ResultLists.NovRoundTotal.Add(0);
                    ResultLists.DecRound.Add(0);
                    ResultLists.DecRoundTotal.Add(0);
                    ResultLists.YearCompanyTotalEsclVat.Add(0);
                    ResultLists.YearCompanyTotalInclVat.Add(0);
                    ResultLists.YearCompanyTotalInclVatRounded.Add(0);
                    ResultLists.MonthAllTotalEsclVat.Add(0);
                    ResultLists.MonthAllTotalInclVat.Add(0);
                    ResultLists.MonthAllTotalInclVatFromRounded.Add(0);
                    ResultLists.YearAllTotalEsclVat.Add(0);
                    ResultLists.YearAllTotalInclVat.Add(0);
                    ResultLists.YearAllTotalInclVatFromRounded.Add(0);
                }

                //**************************************************************************************
                //Adding the data from the different lists into a single list with the properties from the Class Result 
                //basicly just(compNumber, name and monthnumber) the rest is 0
                //**************************************************************************************
                List<Result> result = new List<Result>();

                for (int i = 0; i < ResultLists.CompNr.Count; i++)
                {
                    result.Add(new Result
                    {
                        CompNr = ResultLists.CompNr[i],
                        MonthNr = ResultLists.MonthNr[i],
                        CompName = ResultLists.CompName[i],
                        Jan = ResultLists.Jan[i],
                        JanTotal = ResultLists.JanTotal[i],
                        Feb = ResultLists.Feb[i],
                        FebTotal = ResultLists.FebTotal[i],
                        Mar = ResultLists.Mar[i],
                        MarTotal = ResultLists.MarTotal[i],
                        Apr = ResultLists.Apr[i],
                        AprTotal = ResultLists.AprTotal[i],
                        May = ResultLists.May[i],
                        MayTotal = ResultLists.MayTotal[i],
                        Jun = ResultLists.Jun[i],
                        JunTotal = ResultLists.JunTotal[i],
                        Jul = ResultLists.Jul[i],
                        JulTotal = ResultLists.JulTotal[i],
                        Aug = ResultLists.Aug[i],
                        AugTotal = ResultLists.AugTotal[i],
                        Sep = ResultLists.Sep[i],
                        SepTotal = ResultLists.SepTotal[i],
                        Oct = ResultLists.Oct[i],
                        OctTotal = ResultLists.OctTotal[i],
                        Nov = ResultLists.Nov[i],
                        NovTotal = ResultLists.NovTotal[i],
                        Dec = ResultLists.Dec[i],
                        DecTotal = ResultLists.DecTotal[i],
                        JanRound = ResultLists.JanRound[i],
                        JanRoundTotal = ResultLists.JanRoundTotal[i],
                        FebRound = ResultLists.FebRound[i],
                        FebRoundTotal = ResultLists.FebRoundTotal[i],
                        MarRound = ResultLists.MarRound[i],
                        MarRoundTotal = ResultLists.MarRoundTotal[i],
                        AprRound = ResultLists.AprRound[i],
                        AprRoundTotal = ResultLists.AprRoundTotal[i],
                        MayRound = ResultLists.MayRound[i],
                        MayRoundTotal = ResultLists.MayRoundTotal[i],
                        JunRound = ResultLists.JunRound[i],
                        JunRoundTotal = ResultLists.JunRoundTotal[i],
                        JulRound = ResultLists.JulRound[i],
                        JulRoundTotal = ResultLists.JulRoundTotal[i],
                        AugRound = ResultLists.AugRound[i],
                        AugRoundTotal = ResultLists.AugRoundTotal[i],
                        SepRound = ResultLists.SepRound[i],
                        SepRoundTotal = ResultLists.SepRoundTotal[i],
                        OctRound = ResultLists.OctRound[i],
                        OctRoundTotal = ResultLists.OctRoundTotal[i],
                        NovRound = ResultLists.NovRound[i],
                        NovRoundTotal = ResultLists.NovRoundTotal[i],
                        DecRound = ResultLists.DecRound[i],
                        DecRoundTotal = ResultLists.DecRoundTotal[i],
                        YearCompanyTotalEsclVat = ResultLists.YearCompanyTotalEsclVat[i],
                        YearCompanyTotalInclVat = ResultLists.YearCompanyTotalInclVat[i],
                        YearCompanyTotalInclVatRounded = ResultLists.YearAllTotalInclVatFromRounded[i],
                        MonthAllTotalEsclVat = ResultLists.MonthAllTotalEsclVat[i],
                        MonthAllTotalInclVat = ResultLists.MonthAllTotalInclVat[i],
                        MonthAllTotalInclVatFromRounded = ResultLists.MonthAllTotalInclVatFromRounded[i],
                        YearAllTotalEsclVat = ResultLists.YearAllTotalEsclVat[i],
                        YearAllTotalInclVat = ResultLists.YearAllTotalInclVat[i],
                        YearAllTotalInclVatFromRounded = ResultLists.YearAllTotalInclVatFromRounded[i]
                    });
                }


                //**************************************************************************************
                // Adding the turnover values incl. Vat to the list "result" by using a switch to decide which month to add the result to
                // This is both done with accurate numbers and rounded numbers.
                //**************************************************************************************
                for (int i = 0; i < dataSet.Count; i++)
                {
                    int monthNumber = dataSet[i].MonthNumber;
                    int companyNumber = dataSet[i].CompanyNumber;

                    switch (monthNumber)
                    {
                        case 1:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Jan = dataSet[i].TurnoverVatIncluded;
                                    result[i].JanRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 2:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Feb = dataSet[i].TurnoverVatIncluded;
                                    result[i].FebRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 3:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Mar = dataSet[i].TurnoverVatIncluded;
                                    result[i].MarRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 4:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Apr = dataSet[i].TurnoverVatIncluded;
                                    result[i].AprRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 5:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].May = dataSet[i].TurnoverVatIncluded;
                                    result[i].MayRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 6:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Jun = dataSet[i].TurnoverVatIncluded;
                                    result[i].JunRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 7:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Jul = dataSet[i].TurnoverVatIncluded;
                                    result[i].JulRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 8:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Aug = dataSet[i].TurnoverVatIncluded;
                                    result[i].AugRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 9:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Sep = dataSet[i].TurnoverVatIncluded;
                                    result[i].SepRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 10:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Oct = dataSet[i].TurnoverVatIncluded;
                                    result[i].OctRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 11:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Nov = dataSet[i].TurnoverVatIncluded;
                                    result[i].NovRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        default:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Dec = dataSet[i].TurnoverVatIncluded;
                                    result[i].DecRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                    }
                }

                //**************************************************************************************
                //adding companynumber and name and zero'es to a bunch of list Created in a class called EndResultA
                //to show the final result in a nice way with only one row for each company all other lists are being filled with zero'es
                //**************************************************************************************
                for (int i = 0; i < unicCompanyNames.Count; i++)
                {
                    EndResultLists.CompNr.Add(i);
                    EndResultLists.CompName.Add(unicCompanyNames[i]);
                    EndResultLists.MonthNr.Add(0);
                    EndResultLists.Jan.Add(0);
                    EndResultLists.JanTotal.Add(0);
                    EndResultLists.Feb.Add(0);
                    EndResultLists.FebTotal.Add(0);
                    EndResultLists.Mar.Add(0);
                    EndResultLists.MarTotal.Add(0);
                    EndResultLists.Apr.Add(0);
                    EndResultLists.AprTotal.Add(0);
                    EndResultLists.May.Add(0);
                    EndResultLists.MayTotal.Add(0);
                    EndResultLists.Jun.Add(0);
                    EndResultLists.JunTotal.Add(0);
                    EndResultLists.Jul.Add(0);
                    EndResultLists.JulTotal.Add(0);
                    EndResultLists.Aug.Add(0);
                    EndResultLists.AugTotal.Add(0);
                    EndResultLists.Sep.Add(0);
                    EndResultLists.SepTotal.Add(0);
                    EndResultLists.Oct.Add(0);
                    EndResultLists.OctTotal.Add(0);
                    EndResultLists.Nov.Add(0);
                    EndResultLists.NovTotal.Add(0);
                    EndResultLists.Dec.Add(0);
                    EndResultLists.DecTotal.Add(0);
                    EndResultLists.JanRound.Add(0);
                    EndResultLists.JanRoundTotal.Add(0);
                    EndResultLists.FebRound.Add(0);
                    EndResultLists.FebRoundTotal.Add(0);
                    EndResultLists.MarRound.Add(0);
                    EndResultLists.MarRoundTotal.Add(0);
                    EndResultLists.AprRound.Add(0);
                    EndResultLists.AprRoundTotal.Add(0);
                    EndResultLists.MayRound.Add(0);
                    EndResultLists.MayRoundTotal.Add(0);
                    EndResultLists.JunRound.Add(0);
                    EndResultLists.JunRoundTotal.Add(0);
                    EndResultLists.JulRound.Add(0);
                    EndResultLists.JulRoundTotal.Add(0);
                    EndResultLists.AugRound.Add(0);
                    EndResultLists.AugRoundTotal.Add(0);
                    EndResultLists.SepRound.Add(0);
                    EndResultLists.SepRoundTotal.Add(0);
                    EndResultLists.OctRound.Add(0);
                    EndResultLists.OctRoundTotal.Add(0);
                    EndResultLists.NovRound.Add(0);
                    EndResultLists.NovRoundTotal.Add(0);
                    EndResultLists.DecRound.Add(0);
                    EndResultLists.DecRoundTotal.Add(0);
                    EndResultLists.YearCompanyTotalEsclVat.Add(0);
                    EndResultLists.YearCompanyTotalInclVat.Add(0);
                    EndResultLists.YearCompanyTotalInclVatRounded.Add(0);
                    EndResultLists.MonthAllTotalEsclVat.Add(0);
                    EndResultLists.MonthAllTotalInclVat.Add(0);
                    EndResultLists.MonthAllTotalInclVatFromRounded.Add(0);
                    EndResultLists.YearAllTotalEsclVat.Add(0);
                    EndResultLists.YearAllTotalInclVat.Add(0);
                    EndResultLists.YearAllTotalInclVatFromRounded.Add(0);
                }

                //**************************************************************************************
                //Writing all data sorted into months into a single List called endResult with custom properties
                //**************************************************************************************
                List<Result> endResult = new List<Result>();


                for (int i = 0; i < EndResultLists.CompNr.Count; i++)
                {
                    endResult.Add(new Result
                    {
                        CompNr = EndResultLists.CompNr[i],
                        MonthNr = EndResultLists.MonthNr[i],
                        CompName = EndResultLists.CompName[i],
                        Jan = EndResultLists.Jan[i],
                        JanTotal = EndResultLists.JanTotal[i],
                        Feb = EndResultLists.Feb[i],
                        FebTotal = EndResultLists.FebTotal[i],
                        Mar = EndResultLists.Mar[i],
                        MarTotal = EndResultLists.MarTotal[i],
                        Apr = EndResultLists.Apr[i],
                        AprTotal = EndResultLists.AprTotal[i],
                        May = EndResultLists.May[i],
                        MayTotal = EndResultLists.MayTotal[i],
                        Jun = EndResultLists.Jun[i],
                        JunTotal = EndResultLists.JunTotal[i],
                        Jul = EndResultLists.Jul[i],
                        JulTotal = EndResultLists.JulTotal[i],
                        Aug = EndResultLists.Aug[i],
                        AugTotal = EndResultLists.AugTotal[i],
                        Sep = EndResultLists.Sep[i],
                        SepTotal = EndResultLists.SepTotal[i],
                        Oct = EndResultLists.Oct[i],
                        OctTotal = EndResultLists.OctTotal[i],
                        Nov = EndResultLists.Nov[i],
                        NovTotal = EndResultLists.NovTotal[i],
                        Dec = EndResultLists.Dec[i],
                        DecTotal = EndResultLists.DecTotal[i],
                        JanRound = EndResultLists.JanRound[i],
                        JanRoundTotal = EndResultLists.JanRoundTotal[i],
                        FebRound = EndResultLists.FebRound[i],
                        FebRoundTotal = EndResultLists.FebRoundTotal[i],
                        MarRound = EndResultLists.MarRound[i],
                        MarRoundTotal = EndResultLists.MarRoundTotal[i],
                        AprRound = EndResultLists.AprRound[i],
                        AprRoundTotal = EndResultLists.AprRoundTotal[i],
                        MayRound = EndResultLists.MayRound[i],
                        MayRoundTotal = EndResultLists.MayRoundTotal[i],
                        JunRound = EndResultLists.JunRound[i],
                        JunRoundTotal = EndResultLists.JunRoundTotal[i],
                        JulRound = EndResultLists.JulRound[i],
                        JulRoundTotal = EndResultLists.JulRoundTotal[i],
                        AugRound = EndResultLists.AugRound[i],
                        AugRoundTotal = EndResultLists.AugRoundTotal[i],
                        SepRound = EndResultLists.SepRound[i],
                        SepRoundTotal = EndResultLists.SepRoundTotal[i],
                        OctRound = EndResultLists.OctRound[i],
                        OctRoundTotal = EndResultLists.OctRoundTotal[i],
                        NovRound = EndResultLists.NovRound[i],
                        NovRoundTotal = EndResultLists.NovRoundTotal[i],
                        DecRound = EndResultLists.DecRound[i],
                        DecRoundTotal = EndResultLists.DecRoundTotal[i],
                        YearCompanyTotalEsclVat = EndResultLists.YearCompanyTotalEsclVat[i],
                        YearCompanyTotalInclVat = EndResultLists.YearCompanyTotalInclVat[i],
                        YearCompanyTotalInclVatRounded = EndResultLists.YearAllTotalInclVatFromRounded[i],
                        MonthAllTotalEsclVat = EndResultLists.MonthAllTotalEsclVat[i],
                        MonthAllTotalInclVat = EndResultLists.MonthAllTotalInclVat[i],
                        MonthAllTotalInclVatFromRounded = EndResultLists.MonthAllTotalInclVatFromRounded[i],
                        YearAllTotalEsclVat = EndResultLists.YearAllTotalEsclVat[i],
                        YearAllTotalInclVat = EndResultLists.YearAllTotalInclVat[i],
                        YearAllTotalInclVatFromRounded = EndResultLists.YearAllTotalInclVatFromRounded[i]
                    });
                }

                //**************************************************************************************
                //Calculating the monthly turnover both based on no rounding for each company and for each month and also include rounding as well for each case, 
                //and calculating the total per year for each company
                //**************************************************************************************
                for (int i = 0; i < result.Count; i++)
                {
                    int monthNr = result[i].MonthNr;
                    int companyNr = result[i].CompNr;
                    decimal existingTurnOver;
                    decimal newTurnOver;
                    decimal existingTurnOverRound;
                    decimal newTurnOverRound;

                    for (int j = 0; j < endResult.Count; j++)
                    {
                        if (companyNr == j)
                        {
                            switch (monthNr)
                            {
                                case 1:
                                    existingTurnOver = endResult[j].Jan;
                                    newTurnOver = result[i].Jan;
                                    endResult[j].Jan = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].JanRound;
                                    newTurnOverRound = result[i].JanRound;
                                    endResult[j].JanRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 2:
                                    existingTurnOver = endResult[j].Feb;
                                    newTurnOver = result[i].Feb;
                                    endResult[j].Feb = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].FebRound;
                                    newTurnOverRound = result[i].FebRound;
                                    endResult[j].FebRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 3:
                                    existingTurnOver = endResult[j].Mar;
                                    newTurnOver = result[i].Mar;
                                    endResult[j].Mar = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].MarRound;
                                    newTurnOverRound = result[i].MarRound;
                                    endResult[j].MarRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 4:
                                    existingTurnOver = endResult[j].Apr;
                                    newTurnOver = result[i].Apr;
                                    endResult[j].Apr = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].AprRound;
                                    newTurnOverRound = result[i].AprRound;
                                    endResult[j].AprRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 5:
                                    existingTurnOver = endResult[j].May;
                                    newTurnOver = result[i].May;
                                    endResult[j].May = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].MayRound;
                                    newTurnOverRound = result[i].MayRound;
                                    endResult[j].MayRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 6:
                                    existingTurnOver = endResult[j].Jun;
                                    newTurnOver = result[i].Jun;
                                    endResult[j].Jun = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].JunRound;
                                    newTurnOverRound = result[i].JunRound;
                                    endResult[j].JunRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 7:
                                    existingTurnOver = endResult[j].Jul;
                                    newTurnOver = result[i].Jul;
                                    endResult[j].Jul = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].JulRound;
                                    newTurnOverRound = result[i].JulRound;
                                    endResult[j].JulRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 8:
                                    existingTurnOver = endResult[j].Aug;
                                    newTurnOver = result[i].Aug;
                                    endResult[j].Aug = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].AugRound;
                                    newTurnOverRound = result[i].AugRound;
                                    endResult[j].AugRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 9:
                                    existingTurnOver = endResult[j].Sep;
                                    newTurnOver = result[i].Sep;
                                    endResult[j].Sep = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].SepRound;
                                    newTurnOverRound = result[i].SepRound;
                                    endResult[j].SepRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 10:
                                    existingTurnOver = endResult[j].Oct;
                                    newTurnOver = result[i].Oct;
                                    endResult[j].Oct = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].OctRound;
                                    newTurnOverRound = result[i].OctRound;
                                    endResult[j].OctRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 11:
                                    existingTurnOver = endResult[j].Nov;
                                    newTurnOver = result[i].Nov;
                                    endResult[j].Nov = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].NovRound;
                                    newTurnOverRound = result[i].NovRound;
                                    endResult[j].NovRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                default:
                                    existingTurnOver = endResult[j].Dec;
                                    newTurnOver = result[i].Dec;
                                    endResult[j].Dec = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].DecRound;
                                    newTurnOverRound = result[i].DecRound;
                                    endResult[j].DecRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                            }
                        }
                    }
                }

                #region Printing area for data based on accurate numbers
                //Variables for the printing part.
                string companyName = "Company name   ";
                string janName = "January   ";
                string febName = "February   ";
                string marName = "Marts   ";
                string aprName = "April   ";
                string mayName = "May   ";
                string junName = "June   ";
                string julName = "July   ";
                string augName = "August   ";
                string sepName = "September   ";
                string octName = "October   ";
                string novName = "November   ";
                string decName = "December   ";
                string turnoverYear = "Year total Vat incl.";
                string totalPrMonthVatIncl = "Total Vat incl.";
                string totalPrMonthVatExcl = "Total Vat excl.";
                string spacer15 = "----------------";
                string spacer25 = "--------------------------";
                string spacer30 = "-------------------------------";

                //Printing two lines of spacers to make things shiny
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }

                //Printing explanation for how things are calculated
                Console.WriteLine();
                Console.WriteLine($"Running through the datafile ({filePath}) where the results are calculated based on complete numbers with no roundings." +
                    " The Vat is calculated for each sale with multiple decimals to ensure accuracy in the results.\n");

                //Printing spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //Printig the names of the months to make thing shiny.
                Console.Write($"{companyName,-30} ");
                Console.Write($"{janName,15} ");
                Console.Write($"{febName,15} ");
                Console.Write($"{marName,15} ");
                Console.Write($"{aprName,15} ");
                Console.Write($"{mayName,15} ");
                Console.Write($"{junName,15} ");
                Console.Write($"{julName,15} ");
                Console.Write($"{augName,15} ");
                Console.Write($"{sepName,15} ");
                Console.Write($"{octName,15} ");
                Console.Write($"{novName,15} ");
                Console.Write($"{decName,15} ");
                Console.Write($"{turnoverYear,25}\n");

                //Printing more spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");


                //Printing the monthly turnover for each company and for each month, and calculating the total per year for each company
                for (int i = 0; i < endResult.Count; i++)
                {
                    decimal jan = endResult[i].Jan;
                    decimal feb = endResult[i].Feb;
                    decimal mar = endResult[i].Mar;
                    decimal apr = endResult[i].Apr;
                    decimal may = endResult[i].May;
                    decimal jun = endResult[i].Jun;
                    decimal jul = endResult[i].Jul;
                    decimal aug = endResult[i].Aug;
                    decimal sep = endResult[i].Sep;
                    decimal oct = endResult[i].Oct;
                    decimal nov = endResult[i].Nov;
                    decimal dec = endResult[i].Dec;
                    decimal companyTurnOverPerYear = jan + feb + mar + apr + may + jun +
                                                     jul + aug + sep + oct + nov + dec;


                    Console.Write($"{endResult[i].CompName,-30} ");
                    Console.Write($"{Math.Round(endResult[i].Jan, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Feb, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Mar, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Apr, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].May, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Jun, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Jul, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Aug, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Sep, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Oct, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Nov, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Dec, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYear, 2, MidpointRounding.AwayFromZero),25}\n");
                }


                //Printing even more spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //Calculating each monthly total for all companies combined.
                //**************************************************************************************

                decimal janTotalInclVat = 0;
                decimal febTotalInclVat = 0;
                decimal marTotalInclVat = 0;
                decimal aprTotalInclVat = 0;
                decimal mayTotalInclVat = 0;
                decimal junTotalInclVat = 0;
                decimal julTotalInclVat = 0;
                decimal augTotalInclVat = 0;
                decimal sepTotalInclVat = 0;
                decimal octTotalInclVat = 0;
                decimal novTotalInclVat = 0;
                decimal decTotalInclVat = 0;
                decimal companyTurnOverPerYearInclVat;


                for (int i = 0; i < 1; i++)
                {
                    for (int j = 0; j < endResult.Count; j++)
                    {

                        endResult[i].JanTotal = endResult[i].JanTotal + endResult[j].Jan;
                        janTotalInclVat = endResult[i].JanTotal;
                        endResult[i].FebTotal = endResult[i].FebTotal + endResult[j].Feb;
                        febTotalInclVat = endResult[i].FebTotal;
                        endResult[i].MarTotal = endResult[i].MarTotal + endResult[j].Mar;
                        marTotalInclVat = endResult[i].MarTotal;
                        endResult[i].AprTotal = endResult[i].AprTotal + endResult[j].Apr;
                        aprTotalInclVat = endResult[i].AprTotal;
                        endResult[i].MayTotal = endResult[i].MayTotal + endResult[j].May;
                        mayTotalInclVat = endResult[i].MayTotal;
                        endResult[i].JunTotal = endResult[i].JunTotal + endResult[j].Jun;
                        junTotalInclVat = endResult[i].JunTotal;
                        endResult[i].JulTotal = endResult[i].JulTotal + endResult[j].Jul;
                        julTotalInclVat = endResult[i].JulTotal;
                        endResult[i].AugTotal = endResult[i].AugTotal + endResult[j].Aug;
                        augTotalInclVat = endResult[i].AugTotal;
                        endResult[i].SepTotal = endResult[i].SepTotal + endResult[j].Sep;
                        sepTotalInclVat = endResult[i].SepTotal;
                        endResult[i].OctTotal = endResult[i].OctTotal + endResult[j].Oct;
                        octTotalInclVat = endResult[i].OctTotal;
                        endResult[i].NovTotal = endResult[i].NovTotal + endResult[j].Nov;
                        novTotalInclVat = endResult[i].NovTotal;
                        endResult[i].DecTotal = endResult[i].DecTotal + endResult[j].Dec;
                        decTotalInclVat = endResult[i].DecTotal;
                    }

                }
                companyTurnOverPerYearInclVat = janTotalInclVat + febTotalInclVat + marTotalInclVat + aprTotalInclVat +
                                                mayTotalInclVat + junTotalInclVat + julTotalInclVat + augTotalInclVat +
                                                sepTotalInclVat + octTotalInclVat + novTotalInclVat + decTotalInclVat;


                //Printing total pr month for all companies combined Incl. Vat
                Console.Write($"{totalPrMonthVatIncl,-30} ");
                Console.Write($"{Math.Round(janTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(febTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(marTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(aprTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(mayTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(junTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(julTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(augTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(sepTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(octTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(novTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(decTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(companyTurnOverPerYearInclVat, 2, MidpointRounding.AwayFromZero),25}\n");


                //Printing line of spacers to make things shiny between the two results(don't we just love spacers?)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //Calculating excl vat result pr month in total.
                //**************************************************************************************

                decimal minusVat = multiplier / vat;
                decimal janTotalExclVat = janTotalInclVat * minusVat;
                decimal febTotalExclVat = febTotalInclVat * minusVat;
                decimal marTotalExclVat = marTotalInclVat * minusVat;
                decimal aprTotalExclVat = aprTotalInclVat * minusVat;
                decimal mayTotalExclVat = mayTotalInclVat * minusVat;
                decimal junTotalExclVat = junTotalInclVat * minusVat;
                decimal julTotalExclVat = julTotalInclVat * minusVat;
                decimal augTotalExclVat = augTotalInclVat * minusVat;
                decimal sepTotalExclVat = sepTotalInclVat * minusVat;
                decimal octTotalExclVat = octTotalInclVat * minusVat;
                decimal novTotalExclVat = novTotalInclVat * minusVat;
                decimal decTotalExclVat = decTotalInclVat * minusVat;
                decimal companyTurnOverPerYearExclVat = janTotalExclVat + febTotalExclVat + marTotalExclVat + aprTotalExclVat +
                                                        mayTotalExclVat + junTotalExclVat + julTotalExclVat + augTotalExclVat +
                                                        sepTotalExclVat + octTotalExclVat + novTotalExclVat + decTotalExclVat;


                //Printing total pr month for all companies combined Incl. Vat
                Console.Write($"{totalPrMonthVatExcl,-30} ");
                Console.Write($"{Math.Round(janTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(febTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(marTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(aprTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(mayTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(junTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(julTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(augTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(sepTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(octTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(novTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(decTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(companyTurnOverPerYearExclVat, 2, MidpointRounding.AwayFromZero),25}\n");

                //}

                //Printing two lines of spacers to make things shiny after the result(Spacers, spacers, spacers, spacers Wuhuuu!!!)
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }



                //Making some space before the rounded part is printed (Noooo. this is not spacers!)
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                #endregion



                #region Printing area for data based on rounded numbers
                //Variables for the printing part.
                companyName = "Company name   ";
                janName = "January   ";
                febName = "February   ";
                marName = "Marts   ";
                aprName = "April   ";
                mayName = "May   ";
                junName = "June   ";
                julName = "July   ";
                augName = "August   ";
                sepName = "September   ";
                octName = "October   ";
                novName = "November   ";
                decName = "December   ";
                turnoverYear = "Year total Vat incl.";
                totalPrMonthVatIncl = "Total Vat incl.";
                totalPrMonthVatExcl = "Total Vat excl";
                spacer15 = "----------------";
                spacer25 = "--------------------------";
                spacer30 = "-------------------------------";


                //Printing spacers to make things shiny (GOING CRAZY!!!!! ) Damn you spacers!!
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //Printing explanation for how things are calculated
                Console.WriteLine();
                Console.WriteLine($"Running through the datafile ({filePath}) where the results are calculated " +
                    $"based on rounded numbers for each sale to match the DKK currency\n");

                //Printing spacers to make things shiny (Wonder if there is any spacemen hidden in this program? Go look for them if you dare)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //Printig the names of the months to make thing shiny.
                Console.Write($"{companyName,-30} ");
                Console.Write($"{janName,15} ");
                Console.Write($"{febName,15} ");
                Console.Write($"{marName,15} ");
                Console.Write($"{aprName,15} ");
                Console.Write($"{mayName,15} ");
                Console.Write($"{junName,15} ");
                Console.Write($"{julName,15} ");
                Console.Write($"{augName,15} ");
                Console.Write($"{sepName,15} ");
                Console.Write($"{octName,15} ");
                Console.Write($"{novName,15} ");
                Console.Write($"{decName,15} ");
                Console.Write($"{turnoverYear,25}\n");

                //Printing spacers to make things shiny(I'm an alien.. I' a little alien I'm and spaceman here in the code)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //calculating the total per year for each company and printing the monthly turnover for each company and for each month. 
                //**************************************************************************************
                decimal companyTurnOverPerYearRound;
                for (int i = 0; i < endResult.Count; i++)
                {
                    decimal janRound = endResult[i].JanRound;
                    decimal febRound = endResult[i].FebRound;
                    decimal marRound = endResult[i].MarRound;
                    decimal aprRound = endResult[i].AprRound;
                    decimal mayRound = endResult[i].MayRound;
                    decimal junRound = endResult[i].JunRound;
                    decimal julRound = endResult[i].JulRound;
                    decimal augRound = endResult[i].AugRound;
                    decimal sepRound = endResult[i].SepRound;
                    decimal octRound = endResult[i].OctRound;
                    decimal novRound = endResult[i].NovRound;
                    decimal decRound = endResult[i].DecRound;
                    companyTurnOverPerYearRound = janRound + febRound + marRound + aprRound +
                                                         mayRound + junRound + julRound + augRound +
                                                         sepRound + octRound + novRound + decRound;


                    Console.Write($"{endResult[i].CompName,-30} ");
                    Console.Write($"{Math.Round(endResult[i].JanRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].FebRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MarRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AprRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MayRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JunRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JulRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AugRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].SepRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].OctRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].NovRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].DecRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYearRound, 2, MidpointRounding.AwayFromZero),25}\n");
                }


                //Printing spacers to make things shiny(There might be life in space you know......)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //Calculating the monthly total Vat icluded, for all companies combined We are still in the part where calculations are based on rounded numbers.
                //**************************************************************************************

                decimal janRoundTotalInclVat = 0;
                decimal febRoundTotalInclVat = 0;
                decimal marRoundTotalInclVat = 0;
                decimal aprRoundTotalInclVat = 0;
                decimal mayRoundTotalInclVat = 0;
                decimal junRoundTotalInclVat = 0;
                decimal julRoundTotalInclVat = 0;
                decimal augRoundTotalInclVat = 0;
                decimal sepRoundTotalInclVat = 0;
                decimal octRoundTotalInclVat = 0;
                decimal novRoundTotalInclVat = 0;
                decimal decRoundTotalInclVat = 0;



                for (int i = 0; i < 1; i++)
                {



                    for (int j = 0; j < endResult.Count; j++)
                    {
                        endResult[i].JanRoundTotal = endResult[i].JanRoundTotal + endResult[j].JanRound;
                        janRoundTotalInclVat = endResult[i].JanRoundTotal;
                        endResult[i].FebRoundTotal = endResult[i].FebRoundTotal + endResult[j].FebRound;
                        febRoundTotalInclVat = endResult[i].FebRoundTotal;
                        endResult[i].MarRoundTotal = endResult[i].MarRoundTotal + endResult[j].MarRound;
                        marRoundTotalInclVat = endResult[i].MarRoundTotal;
                        endResult[i].AprRoundTotal = endResult[i].AprRoundTotal + endResult[j].AprRound;
                        aprRoundTotalInclVat = endResult[i].AprRoundTotal;
                        endResult[i].MayRoundTotal = endResult[i].MayRoundTotal + endResult[j].MayRound;
                        mayRoundTotalInclVat = endResult[i].MayRoundTotal;
                        endResult[i].JunRoundTotal = endResult[i].JunRoundTotal + endResult[j].JunRound;
                        junRoundTotalInclVat = endResult[i].JunRoundTotal;
                        endResult[i].JulRoundTotal = endResult[i].JulRoundTotal + endResult[j].JulRound;
                        julRoundTotalInclVat = endResult[i].JulRoundTotal;
                        endResult[i].AugRoundTotal = endResult[i].AugRoundTotal + endResult[j].AugRound;
                        augRoundTotalInclVat = endResult[i].AugRoundTotal;
                        endResult[i].SepRoundTotal = endResult[i].SepRoundTotal + endResult[j].SepRound;
                        sepRoundTotalInclVat = endResult[i].SepRoundTotal;
                        endResult[i].OctRoundTotal = endResult[i].OctRoundTotal + endResult[j].OctRound;
                        octRoundTotalInclVat = endResult[i].OctRoundTotal;
                        endResult[i].NovRoundTotal = endResult[i].NovRoundTotal + endResult[j].NovRound;
                        novRoundTotalInclVat = endResult[i].NovRoundTotal;
                        endResult[i].DecRoundTotal = endResult[i].DecRoundTotal + endResult[j].DecRound;
                        decRoundTotalInclVat = endResult[i].DecRoundTotal;
                    }




                    companyTurnOverPerYearRound = janRoundTotalInclVat + febRoundTotalInclVat + marRoundTotalInclVat + aprRoundTotalInclVat +
                                                         mayRoundTotalInclVat + junRoundTotalInclVat + julRoundTotalInclVat + augRoundTotalInclVat +
                                                         sepRoundTotalInclVat + octRoundTotalInclVat + novRoundTotalInclVat + decRoundTotalInclVat;


                    //**************************************************************************************
                    //Calculating excl vat result pr month in total.
                    //**************************************************************************************
                    multiplier = 100;
                    vat = 125;
                    minusVat = multiplier / vat;
                    decimal janRoundTotalExclVat = endResult[i].JanRoundTotal * minusVat;
                    decimal febRoundTotalExclVat = endResult[i].FebRoundTotal * minusVat;
                    decimal marRoundTotalExclVat = endResult[i].MarRoundTotal * minusVat;
                    decimal aprRoundTotalExclVat = endResult[i].AprRoundTotal * minusVat;
                    decimal mayRoundTotalExclVat = endResult[i].MayRoundTotal * minusVat;
                    decimal junRoundTotalExclVat = endResult[i].JunRoundTotal * minusVat;
                    decimal julRoundTotalExclVat = endResult[i].JulRoundTotal * minusVat;
                    decimal augRoundTotalExclVat = endResult[i].AugRoundTotal * minusVat;
                    decimal sepRoundTotalExclVat = endResult[i].SepRoundTotal * minusVat;
                    decimal octRoundTotalExclVat = endResult[i].OctRoundTotal * minusVat;
                    decimal novRoundTotalExclVat = endResult[i].NovRoundTotal * minusVat;
                    decimal decRoundTotalExclVat = endResult[i].DecRoundTotal * minusVat;
                    companyTurnOverPerYearExclVat = janRoundTotalExclVat + febRoundTotalExclVat + marRoundTotalExclVat + aprRoundTotalExclVat +
                                                           mayRoundTotalExclVat + junRoundTotalExclVat + julRoundTotalExclVat + augRoundTotalExclVat +
                                                           sepRoundTotalExclVat + octRoundTotalExclVat + novRoundTotalExclVat + decRoundTotalExclVat;



                    //Printing total pr month for all companies combined incl Vat.
                    Console.Write($"{totalPrMonthVatIncl,-30} ");
                    Console.Write($"{Math.Round(endResult[i].JanRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].FebRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MarRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AprRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MayRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JunRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JulRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AugRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].SepRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].OctRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].NovRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].DecRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYearRound, 2, MidpointRounding.AwayFromZero),25}\n");

                    //Printing line of spacers to make things shiny between the two results (this is getting closerto area 51 for each spacer!)
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");

                    //Printing total pr month for all companies combined Excl. Vat
                    Console.Write($"{totalPrMonthVatExcl,-30} ");
                    Console.Write($"{Math.Round(janRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(febRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(marRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(aprRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(mayRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(junRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(julRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(augRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(sepRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(octRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(novRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(decRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYearExclVat, 2, MidpointRounding.AwayFromZero),25}\n");
                }

                //Printing two lines of spacers to make things shiny under the result (Double spacers!! Go Gadget Go!!)
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }


                //Printing the errors found in the document
                Console.WriteLine();
                Console.WriteLine($"The following errors was found inthe datafile ({filePath}) and is not included in the calculation\n" +
                    "Some lines might contain more than one error, but will only be listed here for the first error as it has then been filtered away already.\n");


                //Printing spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");


                //Printing the errors found in the datafile
                for (int i = 0; i < rawDataErrors.Count; i++)
                {
                    Console.WriteLine($"{rawDataErrors[i]}");
                }

                //Printing two lines of spacers to make things shiny after running through the file. (phew... This are the last two spacers in this region)
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }

                //Making some space before next program run
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                #endregion
                //**************************************************************************************
                //End printing.
                //**************************************************************************************



                //Clearing "ResultLists" before running through next file. Else the data will be mixed.
                DataSetLists.Date.Clear();
                DataSetLists.Company.Clear();
                DataSetLists.Turnover.Clear();
                DataSetLists.TurnoverVatIncluded.Clear();
                DataSetLists.TurnoverVatIncludedRound.Clear();
                DataSetLists.CompanyNumber.Clear();
                DataSetLists.MonthNumber.Clear();



                //Clearing all Lists created within the code based on either "DataSet" or "Result" classes
                result.Clear();
                endResult.Clear();
                rawData.Clear();
                rawDataClean.Clear();
                rawDataErrors.Clear();
                dataSet.Clear();
                unicCompanyNames.Clear();



                //Clearing "ResultLists" before running through next file. Else the data will be mixed.
                ResultLists.CompNr.Clear();
                ResultLists.CompName.Clear();
                ResultLists.MonthNr.Clear();
                ResultLists.Jan.Clear();
                ResultLists.JanRound.Clear();
                ResultLists.JanRoundTotal.Clear();
                ResultLists.Feb.Clear();
                ResultLists.FebRound.Clear();
                ResultLists.FebRoundTotal.Clear();
                ResultLists.Mar.Clear();
                ResultLists.MarRound.Clear();
                ResultLists.MarRoundTotal.Clear();
                ResultLists.Apr.Clear();
                ResultLists.AprRound.Clear();
                ResultLists.AprRoundTotal.Clear();
                ResultLists.May.Clear();
                ResultLists.MayRound.Clear();
                ResultLists.MayRoundTotal.Clear();
                ResultLists.Jun.Clear();
                ResultLists.JunRound.Clear();
                ResultLists.JunRoundTotal.Clear();
                ResultLists.Jul.Clear();
                ResultLists.JulRound.Clear();
                ResultLists.JulRoundTotal.Clear();
                ResultLists.Aug.Clear();
                ResultLists.AugRound.Clear();
                ResultLists.AugRoundTotal.Clear();
                ResultLists.Sep.Clear();
                ResultLists.SepRound.Clear();
                ResultLists.SepRoundTotal.Clear();
                ResultLists.Oct.Clear();
                ResultLists.OctRound.Clear();
                ResultLists.OctRoundTotal.Clear();
                ResultLists.Nov.Clear();
                ResultLists.NovRound.Clear();
                ResultLists.NovRoundTotal.Clear();
                ResultLists.Dec.Clear();
                ResultLists.DecRound.Clear();
                ResultLists.DecRoundTotal.Clear();
                ResultLists.YearCompanyTotalEsclVat.Clear();
                ResultLists.YearCompanyTotalInclVat.Clear();
                ResultLists.YearCompanyTotalInclVatRounded.Clear();
                ResultLists.MonthAllTotalEsclVat.Clear();
                ResultLists.MonthAllTotalInclVat.Clear();
                ResultLists.MonthAllTotalInclVatFromRounded.Clear();
                ResultLists.YearAllTotalEsclVat.Clear();
                ResultLists.YearAllTotalInclVat.Clear();
                ResultLists.YearAllTotalInclVatFromRounded.Clear();



                //Clearing "Endresultlists" before running through next file. Else the data will be mixed.
                EndResultLists.CompNr.Clear();
                EndResultLists.CompName.Clear();
                EndResultLists.MonthNr.Clear();
                EndResultLists.Jan.Clear();
                EndResultLists.JanRound.Clear();
                EndResultLists.JanRoundTotal.Clear();
                EndResultLists.Feb.Clear();
                EndResultLists.FebRound.Clear();
                EndResultLists.FebRoundTotal.Clear();
                EndResultLists.Mar.Clear();
                EndResultLists.MarRound.Clear();
                EndResultLists.MarRoundTotal.Clear();
                EndResultLists.Apr.Clear();
                EndResultLists.AprRound.Clear();
                EndResultLists.AprRoundTotal.Clear();
                EndResultLists.May.Clear();
                EndResultLists.MayRound.Clear();
                EndResultLists.MayRoundTotal.Clear();
                EndResultLists.Jun.Clear();
                EndResultLists.JunRound.Clear();
                EndResultLists.JunRoundTotal.Clear();
                EndResultLists.Jul.Clear();
                EndResultLists.JulRound.Clear();
                EndResultLists.JulRoundTotal.Clear();
                EndResultLists.Aug.Clear();
                EndResultLists.AugRound.Clear();
                EndResultLists.AugRoundTotal.Clear();
                EndResultLists.Sep.Clear();
                EndResultLists.SepRound.Clear();
                EndResultLists.SepRoundTotal.Clear();
                EndResultLists.Oct.Clear();
                EndResultLists.OctRound.Clear();
                EndResultLists.OctRoundTotal.Clear();
                EndResultLists.Nov.Clear();
                EndResultLists.NovRound.Clear();
                EndResultLists.NovRoundTotal.Clear();
                EndResultLists.Dec.Clear();
                EndResultLists.DecRound.Clear();
                EndResultLists.DecRoundTotal.Clear();
                EndResultLists.YearCompanyTotalEsclVat.Clear();
                EndResultLists.YearCompanyTotalInclVat.Clear();
                EndResultLists.YearCompanyTotalInclVatRounded.Clear();
                EndResultLists.MonthAllTotalEsclVat.Clear();
                EndResultLists.MonthAllTotalInclVat.Clear();
                EndResultLists.MonthAllTotalInclVatFromRounded.Clear();
                EndResultLists.YearAllTotalEsclVat.Clear();
                EndResultLists.YearAllTotalInclVat.Clear();
                EndResultLists.YearAllTotalInclVatFromRounded.Clear();

            } //Move this to the buttom of main Belongs to the loop running through the lists it belongs to main loop for fileselection
            #endregion


            Console.SetOut(consoleOut);//From here, the text will only be written to Console

            //**************************************************************************************
            //**************************************************************************************
            //**************************************************************************************
            //From here all data is being process once again to be printed in the console.
            //Yes I know. I could have just made the printingpart double, instead of the whole program. But this was far easier!
            //**************************************************************************************
            //**************************************************************************************
            //**************************************************************************************

            #region Here data is being printed to console
            for (int file = 0; file < files.Count; file++)//Runs through the number og files in the file list. called this for file instead of "i" because i added it late in the process
            {
                string filePath = files[file];

                decimal vat = 125;
                decimal multiplier = 100;
                decimal dividend = 100;

                //**************************************************************************************
                //Inserting all lines into the list rawData
                //**************************************************************************************
                List<string> rawData = new List<string>();//represent the list with unfiltered data
                List<string> rawDataClean = new List<string>();//Represents the list with data after it has been checked
                List<string> rawDataErrors = new List<string>();//Represents the list with data that contains errors
                //**************************************************************************************


                string dataLine;//variable declared to input content from readline.

                //Reading each lines in the file, and putting it into the two lists rawData and rawDataClean
                using (StreamReader textFile = new StreamReader(filePath))
                {
                    while ((dataLine = textFile.ReadLine()) != null)
                    {
                        rawData.Add(dataLine);
                        rawDataClean.Add(dataLine);
                    }
                }



                //**************************************************************************************
                //Checking for errors: amount of dividers, in date, if company name is empty, and turnover if its a valid decimal number.
                //adding the string lines to the rawDataErrors list and removes the lines from rawDataClean list
                //the errorcount is to be sure that the right lines are being removed from the rawDataClean list
                //**************************************************************************************
                int errorCount = 0;
                for (int i = 0; i < rawData.Count; i++)
                {
                    string divider = ";";
                    int dividerCount = (rawData[i].Length - rawData[i].Replace(divider, "").Length) / divider.Length;//counts the amount og dividers
                    string date = rawData[i].Substring(0, 10);
                    int companyNameStart = rawData[i].IndexOf(";");/* + ";".Length;*/
                    int companyNameEnd = rawData[i].LastIndexOf(";");
                    string turnOver = rawData[i].Substring(rawData[i].LastIndexOf(';') + 1);
                    Regex allowedTurnOver = new Regex(@"^(\d*\,)?\d+$");//making a check to ensure that it is a decimal number (TryParse does not work for this check as it allows all kind of dividers and decimals also mixed together)

                    if (dividerCount < 2)
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by too few dividers!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (dividerCount > 2)
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by too many dividers!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (!DateTime.TryParse(date, out _))
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by error in the date!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (companyNameStart >= companyNameEnd)
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by error in the company name!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                    else if (!allowedTurnOver.IsMatch(turnOver))
                    {
                        rawDataErrors.Add($"Line nr. {i + 1,10}\t {rawData[i],-50} \tError caused by error in the turnover result. We couldn't convert it to decimal number!");
                        rawDataClean.Remove(rawDataClean[i - errorCount]);
                        errorCount++;
                    }
                }



                //**************************************************************************************
                //Splitting data into 5 different lists with variating datatypes
                //**************************************************************************************
                for (int i = 0; i < rawDataClean.Count; i++)
                {
                    string[] splitValue = rawDataClean[i].Split(';'); //Creates a string array. Alternative Visual studio got angry unless i called it for "var" and teachers dont like that
                    for (int j = 0; j < splitValue.Length; j++)
                    {
                        int convertInput = j % 3;
                        switch (convertInput)
                        {
                            case 0:
                                DateTime parsedDate = DateTime.Parse(splitValue[j]);
                                DataSetLists.Date.Add(parsedDate);
                                break;

                            case 1:
                                DataSetLists.Company.Add(splitValue[j]);
                                break;

                            default:

                                decimal parsedNumber = Decimal.Parse(splitValue[j]);
                                DataSetLists.Turnover.Add(parsedNumber);
                                decimal turnoverVatIncluded = parsedNumber * (vat / dividend);// Calculating Turnover including VAT
                                DataSetLists.TurnoverVatIncluded.Add(turnoverVatIncluded);
                                decimal turnOverVatIncludedRound = Math.Round(turnoverVatIncluded, 2, MidpointRounding.AwayFromZero);
                                DataSetLists.TurnoverVatIncludedRound.Add(turnOverVatIncludedRound);
                                break;
                        }
                    }
                }


                //**************************************************************************************
                //Combining the 5 different lists to a single list containing 4 ddifferent datatypes defined in my custom class DataSet
                //**************************************************************************************
                List<DataSet> dataSet = new List<DataSet>();
                for (int i = 0; i < DataSetLists.Date.Count; i++)
                {
                    dataSet.Add(new DataSet
                    {
                        Date = DataSetLists.Date[i],
                        Company = DataSetLists.Company[i],
                        Turnover = DataSetLists.Turnover[i],
                        TurnoverVatIncluded = DataSetLists.TurnoverVatIncluded[i],
                        TurnoverVatIncludedRound = DataSetLists.TurnoverVatIncludedRound[i]
                    });
                }


                //**************************************************************************************
                //Adding all company names to a new list to be able to remove all duplicates
                //**************************************************************************************
                List<string> unicCompanyNames = new List<string>();

                for (int i = 0; i < DataSetLists.Company.Count; i++)
                {
                    unicCompanyNames.Add(DataSetLists.Company[i]);
                }


                //**************************************************************************************
                //Removes all duplicates so all names are unic in the list. Created to be able to sort numbers under each company
                //**************************************************************************************
                unicCompanyNames = unicCompanyNames.Distinct().ToList();
                unicCompanyNames.Sort();


                //**************************************************************************************
                //adding row and line number based on company name and month number
                //**************************************************************************************
                for (int i = 0; i < dataSet.Count; i++)
                {
                    dataSet[i].MonthNumber = dataSet[i].Date.Month;
                    string name = dataSet[i].Company;
                    for (int j = 0; j < unicCompanyNames.Count; j++)
                    {
                        if (name == unicCompanyNames[j])
                        {
                            dataSet[i].CompanyNumber = j;
                        }

                    }

                }


                //**************************************************************************************
                //Vriting data (compNumber, name and monthnumber) into multiple lists to be able to filter
                //data by month and merge them together in the custom list afterwards
                //**************************************************************************************
                for (int i = 0; i < dataSet.Count; i++)
                {
                    string name = dataSet[i].Company;
                    int monthNumber = dataSet[i].MonthNumber;
                    int companyNumber = dataSet[i].CompanyNumber;

                    ResultLists.CompNr.Add(companyNumber);
                    ResultLists.CompName.Add(name);
                    ResultLists.MonthNr.Add(monthNumber);
                    ResultLists.Jan.Add(0);
                    ResultLists.JanTotal.Add(0);
                    ResultLists.Feb.Add(0);
                    ResultLists.FebTotal.Add(0);
                    ResultLists.Mar.Add(0);
                    ResultLists.MarTotal.Add(0);
                    ResultLists.Apr.Add(0);
                    ResultLists.AprTotal.Add(0);
                    ResultLists.May.Add(0);
                    ResultLists.MayTotal.Add(0);
                    ResultLists.Jun.Add(0);
                    ResultLists.JunTotal.Add(0);
                    ResultLists.Jul.Add(0);
                    ResultLists.JulTotal.Add(0);
                    ResultLists.Aug.Add(0);
                    ResultLists.AugTotal.Add(0);
                    ResultLists.Sep.Add(0);
                    ResultLists.SepTotal.Add(0);
                    ResultLists.Oct.Add(0);
                    ResultLists.OctTotal.Add(0);
                    ResultLists.Nov.Add(0);
                    ResultLists.NovTotal.Add(0);
                    ResultLists.Dec.Add(0);
                    ResultLists.DecTotal.Add(0);
                    ResultLists.JanRound.Add(0);
                    ResultLists.JanRoundTotal.Add(0);
                    ResultLists.FebRound.Add(0);
                    ResultLists.FebRoundTotal.Add(0);
                    ResultLists.MarRound.Add(0);
                    ResultLists.MarRoundTotal.Add(0);
                    ResultLists.AprRound.Add(0);
                    ResultLists.AprRoundTotal.Add(0);
                    ResultLists.MayRound.Add(0);
                    ResultLists.MayRoundTotal.Add(0);
                    ResultLists.JunRound.Add(0);
                    ResultLists.JunRoundTotal.Add(0);
                    ResultLists.JulRound.Add(0);
                    ResultLists.JulRoundTotal.Add(0);
                    ResultLists.AugRound.Add(0);
                    ResultLists.AugRoundTotal.Add(0);
                    ResultLists.SepRound.Add(0);
                    ResultLists.SepRoundTotal.Add(0);
                    ResultLists.OctRound.Add(0);
                    ResultLists.OctRoundTotal.Add(0);
                    ResultLists.NovRound.Add(0);
                    ResultLists.NovRoundTotal.Add(0);
                    ResultLists.DecRound.Add(0);
                    ResultLists.DecRoundTotal.Add(0);
                    ResultLists.YearCompanyTotalEsclVat.Add(0);
                    ResultLists.YearCompanyTotalInclVat.Add(0);
                    ResultLists.YearCompanyTotalInclVatRounded.Add(0);
                    ResultLists.MonthAllTotalEsclVat.Add(0);
                    ResultLists.MonthAllTotalInclVat.Add(0);
                    ResultLists.MonthAllTotalInclVatFromRounded.Add(0);
                    ResultLists.YearAllTotalEsclVat.Add(0);
                    ResultLists.YearAllTotalInclVat.Add(0);
                    ResultLists.YearAllTotalInclVatFromRounded.Add(0);
                }

                //**************************************************************************************
                //Adding the data from the different lists into a single list with the properties from the Class Result 
                //basicly just(compNumber, name and monthnumber) the rest is 0
                //**************************************************************************************
                List<Result> result = new List<Result>();

                for (int i = 0; i < ResultLists.CompNr.Count; i++)
                {
                    result.Add(new Result
                    {
                        CompNr = ResultLists.CompNr[i],
                        MonthNr = ResultLists.MonthNr[i],
                        CompName = ResultLists.CompName[i],
                        Jan = ResultLists.Jan[i],
                        JanTotal = ResultLists.JanTotal[i],
                        Feb = ResultLists.Feb[i],
                        FebTotal = ResultLists.FebTotal[i],
                        Mar = ResultLists.Mar[i],
                        MarTotal = ResultLists.MarTotal[i],
                        Apr = ResultLists.Apr[i],
                        AprTotal = ResultLists.AprTotal[i],
                        May = ResultLists.May[i],
                        MayTotal = ResultLists.MayTotal[i],
                        Jun = ResultLists.Jun[i],
                        JunTotal = ResultLists.JunTotal[i],
                        Jul = ResultLists.Jul[i],
                        JulTotal = ResultLists.JulTotal[i],
                        Aug = ResultLists.Aug[i],
                        AugTotal = ResultLists.AugTotal[i],
                        Sep = ResultLists.Sep[i],
                        SepTotal = ResultLists.SepTotal[i],
                        Oct = ResultLists.Oct[i],
                        OctTotal = ResultLists.OctTotal[i],
                        Nov = ResultLists.Nov[i],
                        NovTotal = ResultLists.NovTotal[i],
                        Dec = ResultLists.Dec[i],
                        DecTotal = ResultLists.DecTotal[i],
                        JanRound = ResultLists.JanRound[i],
                        JanRoundTotal = ResultLists.JanRoundTotal[i],
                        FebRound = ResultLists.FebRound[i],
                        FebRoundTotal = ResultLists.FebRoundTotal[i],
                        MarRound = ResultLists.MarRound[i],
                        MarRoundTotal = ResultLists.MarRoundTotal[i],
                        AprRound = ResultLists.AprRound[i],
                        AprRoundTotal = ResultLists.AprRoundTotal[i],
                        MayRound = ResultLists.MayRound[i],
                        MayRoundTotal = ResultLists.MayRoundTotal[i],
                        JunRound = ResultLists.JunRound[i],
                        JunRoundTotal = ResultLists.JunRoundTotal[i],
                        JulRound = ResultLists.JulRound[i],
                        JulRoundTotal = ResultLists.JulRoundTotal[i],
                        AugRound = ResultLists.AugRound[i],
                        AugRoundTotal = ResultLists.AugRoundTotal[i],
                        SepRound = ResultLists.SepRound[i],
                        SepRoundTotal = ResultLists.SepRoundTotal[i],
                        OctRound = ResultLists.OctRound[i],
                        OctRoundTotal = ResultLists.OctRoundTotal[i],
                        NovRound = ResultLists.NovRound[i],
                        NovRoundTotal = ResultLists.NovRoundTotal[i],
                        DecRound = ResultLists.DecRound[i],
                        DecRoundTotal = ResultLists.DecRoundTotal[i],
                        YearCompanyTotalEsclVat = ResultLists.YearCompanyTotalEsclVat[i],
                        YearCompanyTotalInclVat = ResultLists.YearCompanyTotalInclVat[i],
                        YearCompanyTotalInclVatRounded = ResultLists.YearAllTotalInclVatFromRounded[i],
                        MonthAllTotalEsclVat = ResultLists.MonthAllTotalEsclVat[i],
                        MonthAllTotalInclVat = ResultLists.MonthAllTotalInclVat[i],
                        MonthAllTotalInclVatFromRounded = ResultLists.MonthAllTotalInclVatFromRounded[i],
                        YearAllTotalEsclVat = ResultLists.YearAllTotalEsclVat[i],
                        YearAllTotalInclVat = ResultLists.YearAllTotalInclVat[i],
                        YearAllTotalInclVatFromRounded = ResultLists.YearAllTotalInclVatFromRounded[i]
                    });
                }


                //**************************************************************************************
                // Adding the turnover values incl. Vat to the list "result" by using a switch to decide which month to add the result to
                // This is both done with accurate numbers and rounded numbers.
                //**************************************************************************************
                for (int i = 0; i < dataSet.Count; i++)
                {
                    int monthNumber = dataSet[i].MonthNumber;
                    int companyNumber = dataSet[i].CompanyNumber;

                    switch (monthNumber)
                    {
                        case 1:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Jan = dataSet[i].TurnoverVatIncluded;
                                    result[i].JanRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 2:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Feb = dataSet[i].TurnoverVatIncluded;
                                    result[i].FebRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 3:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Mar = dataSet[i].TurnoverVatIncluded;
                                    result[i].MarRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 4:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Apr = dataSet[i].TurnoverVatIncluded;
                                    result[i].AprRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 5:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].May = dataSet[i].TurnoverVatIncluded;
                                    result[i].MayRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 6:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Jun = dataSet[i].TurnoverVatIncluded;
                                    result[i].JunRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 7:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Jul = dataSet[i].TurnoverVatIncluded;
                                    result[i].JulRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 8:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Aug = dataSet[i].TurnoverVatIncluded;
                                    result[i].AugRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 9:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Sep = dataSet[i].TurnoverVatIncluded;
                                    result[i].SepRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 10:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Oct = dataSet[i].TurnoverVatIncluded;
                                    result[i].OctRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        case 11:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Nov = dataSet[i].TurnoverVatIncluded;
                                    result[i].NovRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                        default:
                            for (int j = 0; j < unicCompanyNames.Count; j++)
                            {
                                if (companyNumber == j)
                                {
                                    result[i].Dec = dataSet[i].TurnoverVatIncluded;
                                    result[i].DecRound = dataSet[i].TurnoverVatIncludedRound;
                                }
                            }
                            break;
                    }
                }

                //**************************************************************************************
                //adding companynumber and name and zero'es to a bunch of list Created in a class called EndResultA
                //to show the final result in a nice way with only one row for each company all other lists are being filled with zero'es
                //**************************************************************************************
                for (int i = 0; i < unicCompanyNames.Count; i++)
                {
                    EndResultLists.CompNr.Add(i);
                    EndResultLists.CompName.Add(unicCompanyNames[i]);
                    EndResultLists.MonthNr.Add(0);
                    EndResultLists.Jan.Add(0);
                    EndResultLists.JanTotal.Add(0);
                    EndResultLists.Feb.Add(0);
                    EndResultLists.FebTotal.Add(0);
                    EndResultLists.Mar.Add(0);
                    EndResultLists.MarTotal.Add(0);
                    EndResultLists.Apr.Add(0);
                    EndResultLists.AprTotal.Add(0);
                    EndResultLists.May.Add(0);
                    EndResultLists.MayTotal.Add(0);
                    EndResultLists.Jun.Add(0);
                    EndResultLists.JunTotal.Add(0);
                    EndResultLists.Jul.Add(0);
                    EndResultLists.JulTotal.Add(0);
                    EndResultLists.Aug.Add(0);
                    EndResultLists.AugTotal.Add(0);
                    EndResultLists.Sep.Add(0);
                    EndResultLists.SepTotal.Add(0);
                    EndResultLists.Oct.Add(0);
                    EndResultLists.OctTotal.Add(0);
                    EndResultLists.Nov.Add(0);
                    EndResultLists.NovTotal.Add(0);
                    EndResultLists.Dec.Add(0);
                    EndResultLists.DecTotal.Add(0);
                    EndResultLists.JanRound.Add(0);
                    EndResultLists.JanRoundTotal.Add(0);
                    EndResultLists.FebRound.Add(0);
                    EndResultLists.FebRoundTotal.Add(0);
                    EndResultLists.MarRound.Add(0);
                    EndResultLists.MarRoundTotal.Add(0);
                    EndResultLists.AprRound.Add(0);
                    EndResultLists.AprRoundTotal.Add(0);
                    EndResultLists.MayRound.Add(0);
                    EndResultLists.MayRoundTotal.Add(0);
                    EndResultLists.JunRound.Add(0);
                    EndResultLists.JunRoundTotal.Add(0);
                    EndResultLists.JulRound.Add(0);
                    EndResultLists.JulRoundTotal.Add(0);
                    EndResultLists.AugRound.Add(0);
                    EndResultLists.AugRoundTotal.Add(0);
                    EndResultLists.SepRound.Add(0);
                    EndResultLists.SepRoundTotal.Add(0);
                    EndResultLists.OctRound.Add(0);
                    EndResultLists.OctRoundTotal.Add(0);
                    EndResultLists.NovRound.Add(0);
                    EndResultLists.NovRoundTotal.Add(0);
                    EndResultLists.DecRound.Add(0);
                    EndResultLists.DecRoundTotal.Add(0);
                    EndResultLists.YearCompanyTotalEsclVat.Add(0);
                    EndResultLists.YearCompanyTotalInclVat.Add(0);
                    EndResultLists.YearCompanyTotalInclVatRounded.Add(0);
                    EndResultLists.MonthAllTotalEsclVat.Add(0);
                    EndResultLists.MonthAllTotalInclVat.Add(0);
                    EndResultLists.MonthAllTotalInclVatFromRounded.Add(0);
                    EndResultLists.YearAllTotalEsclVat.Add(0);
                    EndResultLists.YearAllTotalInclVat.Add(0);
                    EndResultLists.YearAllTotalInclVatFromRounded.Add(0);
                }

                //**************************************************************************************
                //Writing all data sorted into months into a single List called endResult with custom properties
                //**************************************************************************************
                List<Result> endResult = new List<Result>();


                for (int i = 0; i < EndResultLists.CompNr.Count; i++)
                {
                    endResult.Add(new Result
                    {
                        CompNr = EndResultLists.CompNr[i],
                        MonthNr = EndResultLists.MonthNr[i],
                        CompName = EndResultLists.CompName[i],
                        Jan = EndResultLists.Jan[i],
                        JanTotal = EndResultLists.JanTotal[i],
                        Feb = EndResultLists.Feb[i],
                        FebTotal = EndResultLists.FebTotal[i],
                        Mar = EndResultLists.Mar[i],
                        MarTotal = EndResultLists.MarTotal[i],
                        Apr = EndResultLists.Apr[i],
                        AprTotal = EndResultLists.AprTotal[i],
                        May = EndResultLists.May[i],
                        MayTotal = EndResultLists.MayTotal[i],
                        Jun = EndResultLists.Jun[i],
                        JunTotal = EndResultLists.JunTotal[i],
                        Jul = EndResultLists.Jul[i],
                        JulTotal = EndResultLists.JulTotal[i],
                        Aug = EndResultLists.Aug[i],
                        AugTotal = EndResultLists.AugTotal[i],
                        Sep = EndResultLists.Sep[i],
                        SepTotal = EndResultLists.SepTotal[i],
                        Oct = EndResultLists.Oct[i],
                        OctTotal = EndResultLists.OctTotal[i],
                        Nov = EndResultLists.Nov[i],
                        NovTotal = EndResultLists.NovTotal[i],
                        Dec = EndResultLists.Dec[i],
                        DecTotal = EndResultLists.DecTotal[i],
                        JanRound = EndResultLists.JanRound[i],
                        JanRoundTotal = EndResultLists.JanRoundTotal[i],
                        FebRound = EndResultLists.FebRound[i],
                        FebRoundTotal = EndResultLists.FebRoundTotal[i],
                        MarRound = EndResultLists.MarRound[i],
                        MarRoundTotal = EndResultLists.MarRoundTotal[i],
                        AprRound = EndResultLists.AprRound[i],
                        AprRoundTotal = EndResultLists.AprRoundTotal[i],
                        MayRound = EndResultLists.MayRound[i],
                        MayRoundTotal = EndResultLists.MayRoundTotal[i],
                        JunRound = EndResultLists.JunRound[i],
                        JunRoundTotal = EndResultLists.JunRoundTotal[i],
                        JulRound = EndResultLists.JulRound[i],
                        JulRoundTotal = EndResultLists.JulRoundTotal[i],
                        AugRound = EndResultLists.AugRound[i],
                        AugRoundTotal = EndResultLists.AugRoundTotal[i],
                        SepRound = EndResultLists.SepRound[i],
                        SepRoundTotal = EndResultLists.SepRoundTotal[i],
                        OctRound = EndResultLists.OctRound[i],
                        OctRoundTotal = EndResultLists.OctRoundTotal[i],
                        NovRound = EndResultLists.NovRound[i],
                        NovRoundTotal = EndResultLists.NovRoundTotal[i],
                        DecRound = EndResultLists.DecRound[i],
                        DecRoundTotal = EndResultLists.DecRoundTotal[i],
                        YearCompanyTotalEsclVat = EndResultLists.YearCompanyTotalEsclVat[i],
                        YearCompanyTotalInclVat = EndResultLists.YearCompanyTotalInclVat[i],
                        YearCompanyTotalInclVatRounded = EndResultLists.YearAllTotalInclVatFromRounded[i],
                        MonthAllTotalEsclVat = EndResultLists.MonthAllTotalEsclVat[i],
                        MonthAllTotalInclVat = EndResultLists.MonthAllTotalInclVat[i],
                        MonthAllTotalInclVatFromRounded = EndResultLists.MonthAllTotalInclVatFromRounded[i],
                        YearAllTotalEsclVat = EndResultLists.YearAllTotalEsclVat[i],
                        YearAllTotalInclVat = EndResultLists.YearAllTotalInclVat[i],
                        YearAllTotalInclVatFromRounded = EndResultLists.YearAllTotalInclVatFromRounded[i]
                    });
                }

                //**************************************************************************************
                //Calculating the monthly turnover both based on no rounding for each company and for each month and also include rounding as well for each case, 
                //and calculating the total per year for each company
                //**************************************************************************************
                for (int i = 0; i < result.Count; i++)
                {
                    int monthNr = result[i].MonthNr;
                    int companyNr = result[i].CompNr;
                    decimal existingTurnOver;
                    decimal newTurnOver;
                    decimal existingTurnOverRound;
                    decimal newTurnOverRound;

                    for (int j = 0; j < endResult.Count; j++)
                    {
                        if (companyNr == j)
                        {
                            switch (monthNr)
                            {
                                case 1:
                                    existingTurnOver = endResult[j].Jan;
                                    newTurnOver = result[i].Jan;
                                    endResult[j].Jan = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].JanRound;
                                    newTurnOverRound = result[i].JanRound;
                                    endResult[j].JanRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 2:
                                    existingTurnOver = endResult[j].Feb;
                                    newTurnOver = result[i].Feb;
                                    endResult[j].Feb = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].FebRound;
                                    newTurnOverRound = result[i].FebRound;
                                    endResult[j].FebRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 3:
                                    existingTurnOver = endResult[j].Mar;
                                    newTurnOver = result[i].Mar;
                                    endResult[j].Mar = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].MarRound;
                                    newTurnOverRound = result[i].MarRound;
                                    endResult[j].MarRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 4:
                                    existingTurnOver = endResult[j].Apr;
                                    newTurnOver = result[i].Apr;
                                    endResult[j].Apr = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].AprRound;
                                    newTurnOverRound = result[i].AprRound;
                                    endResult[j].AprRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 5:
                                    existingTurnOver = endResult[j].May;
                                    newTurnOver = result[i].May;
                                    endResult[j].May = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].MayRound;
                                    newTurnOverRound = result[i].MayRound;
                                    endResult[j].MayRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 6:
                                    existingTurnOver = endResult[j].Jun;
                                    newTurnOver = result[i].Jun;
                                    endResult[j].Jun = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].JunRound;
                                    newTurnOverRound = result[i].JunRound;
                                    endResult[j].JunRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 7:
                                    existingTurnOver = endResult[j].Jul;
                                    newTurnOver = result[i].Jul;
                                    endResult[j].Jul = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].JulRound;
                                    newTurnOverRound = result[i].JulRound;
                                    endResult[j].JulRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 8:
                                    existingTurnOver = endResult[j].Aug;
                                    newTurnOver = result[i].Aug;
                                    endResult[j].Aug = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].AugRound;
                                    newTurnOverRound = result[i].AugRound;
                                    endResult[j].AugRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 9:
                                    existingTurnOver = endResult[j].Sep;
                                    newTurnOver = result[i].Sep;
                                    endResult[j].Sep = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].SepRound;
                                    newTurnOverRound = result[i].SepRound;
                                    endResult[j].SepRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 10:
                                    existingTurnOver = endResult[j].Oct;
                                    newTurnOver = result[i].Oct;
                                    endResult[j].Oct = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].OctRound;
                                    newTurnOverRound = result[i].OctRound;
                                    endResult[j].OctRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                case 11:
                                    existingTurnOver = endResult[j].Nov;
                                    newTurnOver = result[i].Nov;
                                    endResult[j].Nov = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].NovRound;
                                    newTurnOverRound = result[i].NovRound;
                                    endResult[j].NovRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                                default:
                                    existingTurnOver = endResult[j].Dec;
                                    newTurnOver = result[i].Dec;
                                    endResult[j].Dec = existingTurnOver + newTurnOver;

                                    existingTurnOverRound = endResult[j].DecRound;
                                    newTurnOverRound = result[i].DecRound;
                                    endResult[j].DecRound = existingTurnOverRound + newTurnOverRound;
                                    break;
                            }
                        }
                    }
                }

                #region Printing area for data based on accurate numbers
                //Variables for the printing part.
                string companyName = "Company name   ";
                string janName = "January   ";
                string febName = "February   ";
                string marName = "Marts   ";
                string aprName = "April   ";
                string mayName = "May   ";
                string junName = "June   ";
                string julName = "July   ";
                string augName = "August   ";
                string sepName = "September   ";
                string octName = "October   ";
                string novName = "November   ";
                string decName = "December   ";
                string turnoverYear = "Year total Vat incl.";
                string totalPrMonthVatIncl = "Total Vat incl.";
                string totalPrMonthVatExcl = "Total Vat excl.";
                string spacer15 = "----------------";
                string spacer25 = "--------------------------";
                string spacer30 = "-------------------------------";

                //Printing two lines of spacers to make things shiny
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }

                //Printing explanation for how things are calculated
                Console.WriteLine();
                Console.WriteLine($"Running through the datafile ({filePath}) where the results are calculated based on complete numbers with no roundings." +
                    " The Vat is calculated for each sale with multiple decimals to ensure accuracy in the results.\n");

                //Printing spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //Printig the names of the months to make thing shiny.
                Console.Write($"{companyName,-30} ");
                Console.Write($"{janName,15} ");
                Console.Write($"{febName,15} ");
                Console.Write($"{marName,15} ");
                Console.Write($"{aprName,15} ");
                Console.Write($"{mayName,15} ");
                Console.Write($"{junName,15} ");
                Console.Write($"{julName,15} ");
                Console.Write($"{augName,15} ");
                Console.Write($"{sepName,15} ");
                Console.Write($"{octName,15} ");
                Console.Write($"{novName,15} ");
                Console.Write($"{decName,15} ");
                Console.Write($"{turnoverYear,25}\n");

                //Printing more spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");


                //Printing the monthly turnover for each company and for each month, and calculating the total per year for each company
                for (int i = 0; i < endResult.Count; i++)
                {
                    decimal jan = endResult[i].Jan;
                    decimal feb = endResult[i].Feb;
                    decimal mar = endResult[i].Mar;
                    decimal apr = endResult[i].Apr;
                    decimal may = endResult[i].May;
                    decimal jun = endResult[i].Jun;
                    decimal jul = endResult[i].Jul;
                    decimal aug = endResult[i].Aug;
                    decimal sep = endResult[i].Sep;
                    decimal oct = endResult[i].Oct;
                    decimal nov = endResult[i].Nov;
                    decimal dec = endResult[i].Dec;
                    decimal companyTurnOverPerYear = jan + feb + mar + apr + may + jun +
                                                     jul + aug + sep + oct + nov + dec;


                    Console.Write($"{endResult[i].CompName,-30} ");
                    Console.Write($"{Math.Round(endResult[i].Jan, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Feb, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Mar, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Apr, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].May, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Jun, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Jul, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Aug, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Sep, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Oct, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Nov, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].Dec, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYear, 2, MidpointRounding.AwayFromZero),25}\n");
                }


                //Printing even more spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //Calculating each monthly total for all companies combined.
                //**************************************************************************************

                decimal janTotalInclVat = 0;
                decimal febTotalInclVat = 0;
                decimal marTotalInclVat = 0;
                decimal aprTotalInclVat = 0;
                decimal mayTotalInclVat = 0;
                decimal junTotalInclVat = 0;
                decimal julTotalInclVat = 0;
                decimal augTotalInclVat = 0;
                decimal sepTotalInclVat = 0;
                decimal octTotalInclVat = 0;
                decimal novTotalInclVat = 0;
                decimal decTotalInclVat = 0;
                decimal companyTurnOverPerYearInclVat;


                for (int i = 0; i < 1; i++)
                {
                    for (int j = 0; j < endResult.Count; j++)
                    {

                        endResult[i].JanTotal = endResult[i].JanTotal + endResult[j].Jan;
                        janTotalInclVat = endResult[i].JanTotal;
                        endResult[i].FebTotal = endResult[i].FebTotal + endResult[j].Feb;
                        febTotalInclVat = endResult[i].FebTotal;
                        endResult[i].MarTotal = endResult[i].MarTotal + endResult[j].Mar;
                        marTotalInclVat = endResult[i].MarTotal;
                        endResult[i].AprTotal = endResult[i].AprTotal + endResult[j].Apr;
                        aprTotalInclVat = endResult[i].AprTotal;
                        endResult[i].MayTotal = endResult[i].MayTotal + endResult[j].May;
                        mayTotalInclVat = endResult[i].MayTotal;
                        endResult[i].JunTotal = endResult[i].JunTotal + endResult[j].Jun;
                        junTotalInclVat = endResult[i].JunTotal;
                        endResult[i].JulTotal = endResult[i].JulTotal + endResult[j].Jul;
                        julTotalInclVat = endResult[i].JulTotal;
                        endResult[i].AugTotal = endResult[i].AugTotal + endResult[j].Aug;
                        augTotalInclVat = endResult[i].AugTotal;
                        endResult[i].SepTotal = endResult[i].SepTotal + endResult[j].Sep;
                        sepTotalInclVat = endResult[i].SepTotal;
                        endResult[i].OctTotal = endResult[i].OctTotal + endResult[j].Oct;
                        octTotalInclVat = endResult[i].OctTotal;
                        endResult[i].NovTotal = endResult[i].NovTotal + endResult[j].Nov;
                        novTotalInclVat = endResult[i].NovTotal;
                        endResult[i].DecTotal = endResult[i].DecTotal + endResult[j].Dec;
                        decTotalInclVat = endResult[i].DecTotal;
                    }

                }
                companyTurnOverPerYearInclVat = janTotalInclVat + febTotalInclVat + marTotalInclVat + aprTotalInclVat +
                                                mayTotalInclVat + junTotalInclVat + julTotalInclVat + augTotalInclVat +
                                                sepTotalInclVat + octTotalInclVat + novTotalInclVat + decTotalInclVat;


                //Printing total pr month for all companies combined Incl. Vat
                Console.Write($"{totalPrMonthVatIncl,-30} ");
                Console.Write($"{Math.Round(janTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(febTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(marTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(aprTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(mayTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(junTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(julTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(augTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(sepTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(octTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(novTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(decTotalInclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(companyTurnOverPerYearInclVat, 2, MidpointRounding.AwayFromZero),25}\n");


                //Printing line of spacers to make things shiny between the two results(don't we just love spacers?)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //Calculating excl vat result pr month in total.
                //**************************************************************************************

                decimal minusVat = multiplier / vat;
                decimal janTotalExclVat = janTotalInclVat * minusVat;
                decimal febTotalExclVat = febTotalInclVat * minusVat;
                decimal marTotalExclVat = marTotalInclVat * minusVat;
                decimal aprTotalExclVat = aprTotalInclVat * minusVat;
                decimal mayTotalExclVat = mayTotalInclVat * minusVat;
                decimal junTotalExclVat = junTotalInclVat * minusVat;
                decimal julTotalExclVat = julTotalInclVat * minusVat;
                decimal augTotalExclVat = augTotalInclVat * minusVat;
                decimal sepTotalExclVat = sepTotalInclVat * minusVat;
                decimal octTotalExclVat = octTotalInclVat * minusVat;
                decimal novTotalExclVat = novTotalInclVat * minusVat;
                decimal decTotalExclVat = decTotalInclVat * minusVat;
                decimal companyTurnOverPerYearExclVat = janTotalExclVat + febTotalExclVat + marTotalExclVat + aprTotalExclVat +
                                                        mayTotalExclVat + junTotalExclVat + julTotalExclVat + augTotalExclVat +
                                                        sepTotalExclVat + octTotalExclVat + novTotalExclVat + decTotalExclVat;


                //Printing total pr month for all companies combined Incl. Vat
                Console.Write($"{totalPrMonthVatExcl,-30} ");
                Console.Write($"{Math.Round(janTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(febTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(marTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(aprTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(mayTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(junTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(julTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(augTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(sepTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(octTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(novTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(decTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                Console.Write($"{Math.Round(companyTurnOverPerYearExclVat, 2, MidpointRounding.AwayFromZero),25}\n");

                //}

                //Printing two lines of spacers to make things shiny after the result(Spacers, spacers, spacers, spacers Wuhuuu!!!)
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }



                //Making some space before the rounded part is printed (Noooo. this is not spacers!)
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                #endregion



                #region Printing area for data based on rounded numbers
                //Variables for the printing part.
                companyName = "Company name   ";
                janName = "January   ";
                febName = "February   ";
                marName = "Marts   ";
                aprName = "April   ";
                mayName = "May   ";
                junName = "June   ";
                julName = "July   ";
                augName = "August   ";
                sepName = "September   ";
                octName = "October   ";
                novName = "November   ";
                decName = "December   ";
                turnoverYear = "Year total Vat incl.";
                totalPrMonthVatIncl = "Total Vat incl.";
                totalPrMonthVatExcl = "Total Vat excl";
                spacer15 = "----------------";
                spacer25 = "--------------------------";
                spacer30 = "-------------------------------";


                //Printing spacers to make things shiny (GOING CRAZY!!!!! ) Damn you spacers!!
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //Printing explanation for how things are calculated
                Console.WriteLine();
                Console.WriteLine($"Running through the datafile ({filePath}) where the results are calculated " +
                    $"based on rounded numbers for each sale to match the DKK currency\n");

                //Printing spacers to make things shiny (Wonder if there is any spacemen hidden in this program? Go look for them if you dare)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //Printig the names of the months to make thing shiny.
                Console.Write($"{companyName,-30} ");
                Console.Write($"{janName,15} ");
                Console.Write($"{febName,15} ");
                Console.Write($"{marName,15} ");
                Console.Write($"{aprName,15} ");
                Console.Write($"{mayName,15} ");
                Console.Write($"{junName,15} ");
                Console.Write($"{julName,15} ");
                Console.Write($"{augName,15} ");
                Console.Write($"{sepName,15} ");
                Console.Write($"{octName,15} ");
                Console.Write($"{novName,15} ");
                Console.Write($"{decName,15} ");
                Console.Write($"{turnoverYear,25}\n");

                //Printing spacers to make things shiny(I'm an alien.. I' a little alien I'm and spaceman here in the code)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //calculating the total per year for each company and printing the monthly turnover for each company and for each month. 
                //**************************************************************************************
                decimal companyTurnOverPerYearRound;
                for (int i = 0; i < endResult.Count; i++)
                {
                    decimal janRound = endResult[i].JanRound;
                    decimal febRound = endResult[i].FebRound;
                    decimal marRound = endResult[i].MarRound;
                    decimal aprRound = endResult[i].AprRound;
                    decimal mayRound = endResult[i].MayRound;
                    decimal junRound = endResult[i].JunRound;
                    decimal julRound = endResult[i].JulRound;
                    decimal augRound = endResult[i].AugRound;
                    decimal sepRound = endResult[i].SepRound;
                    decimal octRound = endResult[i].OctRound;
                    decimal novRound = endResult[i].NovRound;
                    decimal decRound = endResult[i].DecRound;
                    companyTurnOverPerYearRound = janRound + febRound + marRound + aprRound +
                                                         mayRound + junRound + julRound + augRound +
                                                         sepRound + octRound + novRound + decRound;


                    Console.Write($"{endResult[i].CompName,-30} ");
                    Console.Write($"{Math.Round(endResult[i].JanRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].FebRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MarRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AprRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MayRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JunRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JulRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AugRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].SepRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].OctRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].NovRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].DecRound, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYearRound, 2, MidpointRounding.AwayFromZero),25}\n");
                }


                //Printing spacers to make things shiny(There might be life in space you know......)
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");

                //**************************************************************************************
                //Calculating the monthly total Vat icluded, for all companies combined We are still in the part where calculations are based on rounded numbers.
                //**************************************************************************************

                decimal janRoundTotalInclVat = 0;
                decimal febRoundTotalInclVat = 0;
                decimal marRoundTotalInclVat = 0;
                decimal aprRoundTotalInclVat = 0;
                decimal mayRoundTotalInclVat = 0;
                decimal junRoundTotalInclVat = 0;
                decimal julRoundTotalInclVat = 0;
                decimal augRoundTotalInclVat = 0;
                decimal sepRoundTotalInclVat = 0;
                decimal octRoundTotalInclVat = 0;
                decimal novRoundTotalInclVat = 0;
                decimal decRoundTotalInclVat = 0;



                for (int i = 0; i < 1; i++)
                {



                    for (int j = 0; j < endResult.Count; j++)
                    {
                        endResult[i].JanRoundTotal = endResult[i].JanRoundTotal + endResult[j].JanRound;
                        janRoundTotalInclVat = endResult[i].JanRoundTotal;
                        endResult[i].FebRoundTotal = endResult[i].FebRoundTotal + endResult[j].FebRound;
                        febRoundTotalInclVat = endResult[i].FebRoundTotal;
                        endResult[i].MarRoundTotal = endResult[i].MarRoundTotal + endResult[j].MarRound;
                        marRoundTotalInclVat = endResult[i].MarRoundTotal;
                        endResult[i].AprRoundTotal = endResult[i].AprRoundTotal + endResult[j].AprRound;
                        aprRoundTotalInclVat = endResult[i].AprRoundTotal;
                        endResult[i].MayRoundTotal = endResult[i].MayRoundTotal + endResult[j].MayRound;
                        mayRoundTotalInclVat = endResult[i].MayRoundTotal;
                        endResult[i].JunRoundTotal = endResult[i].JunRoundTotal + endResult[j].JunRound;
                        junRoundTotalInclVat = endResult[i].JunRoundTotal;
                        endResult[i].JulRoundTotal = endResult[i].JulRoundTotal + endResult[j].JulRound;
                        julRoundTotalInclVat = endResult[i].JulRoundTotal;
                        endResult[i].AugRoundTotal = endResult[i].AugRoundTotal + endResult[j].AugRound;
                        augRoundTotalInclVat = endResult[i].AugRoundTotal;
                        endResult[i].SepRoundTotal = endResult[i].SepRoundTotal + endResult[j].SepRound;
                        sepRoundTotalInclVat = endResult[i].SepRoundTotal;
                        endResult[i].OctRoundTotal = endResult[i].OctRoundTotal + endResult[j].OctRound;
                        octRoundTotalInclVat = endResult[i].OctRoundTotal;
                        endResult[i].NovRoundTotal = endResult[i].NovRoundTotal + endResult[j].NovRound;
                        novRoundTotalInclVat = endResult[i].NovRoundTotal;
                        endResult[i].DecRoundTotal = endResult[i].DecRoundTotal + endResult[j].DecRound;
                        decRoundTotalInclVat = endResult[i].DecRoundTotal;
                    }




                    companyTurnOverPerYearRound = janRoundTotalInclVat + febRoundTotalInclVat + marRoundTotalInclVat + aprRoundTotalInclVat +
                                                         mayRoundTotalInclVat + junRoundTotalInclVat + julRoundTotalInclVat + augRoundTotalInclVat +
                                                         sepRoundTotalInclVat + octRoundTotalInclVat + novRoundTotalInclVat + decRoundTotalInclVat;


                    //**************************************************************************************
                    //Calculating excl vat result pr month in total.
                    //**************************************************************************************
                    multiplier = 100;
                    vat = 125;
                    minusVat = multiplier / vat;
                    decimal janRoundTotalExclVat = endResult[i].JanRoundTotal * minusVat;
                    decimal febRoundTotalExclVat = endResult[i].FebRoundTotal * minusVat;
                    decimal marRoundTotalExclVat = endResult[i].MarRoundTotal * minusVat;
                    decimal aprRoundTotalExclVat = endResult[i].AprRoundTotal * minusVat;
                    decimal mayRoundTotalExclVat = endResult[i].MayRoundTotal * minusVat;
                    decimal junRoundTotalExclVat = endResult[i].JunRoundTotal * minusVat;
                    decimal julRoundTotalExclVat = endResult[i].JulRoundTotal * minusVat;
                    decimal augRoundTotalExclVat = endResult[i].AugRoundTotal * minusVat;
                    decimal sepRoundTotalExclVat = endResult[i].SepRoundTotal * minusVat;
                    decimal octRoundTotalExclVat = endResult[i].OctRoundTotal * minusVat;
                    decimal novRoundTotalExclVat = endResult[i].NovRoundTotal * minusVat;
                    decimal decRoundTotalExclVat = endResult[i].DecRoundTotal * minusVat;
                    companyTurnOverPerYearExclVat = janRoundTotalExclVat + febRoundTotalExclVat + marRoundTotalExclVat + aprRoundTotalExclVat +
                                                           mayRoundTotalExclVat + junRoundTotalExclVat + julRoundTotalExclVat + augRoundTotalExclVat +
                                                           sepRoundTotalExclVat + octRoundTotalExclVat + novRoundTotalExclVat + decRoundTotalExclVat;



                    //Printing total pr month for all companies combined incl Vat.
                    Console.Write($"{totalPrMonthVatIncl,-30} ");
                    Console.Write($"{Math.Round(endResult[i].JanRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].FebRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MarRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AprRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].MayRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JunRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].JulRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].AugRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].SepRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].OctRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].NovRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(endResult[i].DecRoundTotal, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYearRound, 2, MidpointRounding.AwayFromZero),25}\n");

                    //Printing line of spacers to make things shiny between the two results (this is getting closerto area 51 for each spacer!)
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");

                    //Printing total pr month for all companies combined Excl. Vat
                    Console.Write($"{totalPrMonthVatExcl,-30} ");
                    Console.Write($"{Math.Round(janRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(febRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(marRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(aprRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(mayRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(junRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(julRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(augRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(sepRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(octRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(novRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(decRoundTotalExclVat, 2, MidpointRounding.AwayFromZero),15} ");
                    Console.Write($"{Math.Round(companyTurnOverPerYearExclVat, 2, MidpointRounding.AwayFromZero),25}\n");
                }

                //Printing two lines of spacers to make things shiny under the result (Double spacers!! Go Gadget Go!!)
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }


                //Printing the errors found in the document
                Console.WriteLine();
                Console.WriteLine($"The following errors was found inthe datafile ({filePath}) and is not included in the calculation\n" +
                    "Some lines might contain more than one error, but will only be listed here for the first error as it has then been filtered away already.\n");


                //Printing spacers to make things shiny
                Console.Write($"{spacer30,-31}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer15,16}");
                Console.Write($"{spacer25,25}\n");


                //Printing the errors found in the datafile
                for (int i = 0; i < rawDataErrors.Count; i++)
                {
                    Console.WriteLine($"{rawDataErrors[i]}");
                }

                //Printing two lines of spacers to make things shiny after running through the file. (phew... This are the last two spacers in this region)
                for (int i = 0; i < 2; i++)
                {
                    Console.Write($"{spacer30,-31}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer15,16}");
                    Console.Write($"{spacer25,25}\n");
                }

                //Making some space before next program run
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine();

                #endregion
                //**************************************************************************************
                //End printing.
                //**************************************************************************************



                //Clearing "ResultLists" before running through next file. Else the data will be mixed.
                DataSetLists.Date.Clear();
                DataSetLists.Company.Clear();
                DataSetLists.Turnover.Clear();
                DataSetLists.TurnoverVatIncluded.Clear();
                DataSetLists.TurnoverVatIncludedRound.Clear();
                DataSetLists.CompanyNumber.Clear();
                DataSetLists.MonthNumber.Clear();



                //Clearing all Lists created within the code based on either "DataSet" or "Result" classes
                result.Clear();
                endResult.Clear();
                rawData.Clear();
                rawDataClean.Clear();
                rawDataErrors.Clear();
                dataSet.Clear();
                unicCompanyNames.Clear();



                //Clearing "ResultLists" before running through next file. Else the data will be mixed.
                ResultLists.CompNr.Clear();
                ResultLists.CompName.Clear();
                ResultLists.MonthNr.Clear();
                ResultLists.Jan.Clear();
                ResultLists.JanRound.Clear();
                ResultLists.JanRoundTotal.Clear();
                ResultLists.Feb.Clear();
                ResultLists.FebRound.Clear();
                ResultLists.FebRoundTotal.Clear();
                ResultLists.Mar.Clear();
                ResultLists.MarRound.Clear();
                ResultLists.MarRoundTotal.Clear();
                ResultLists.Apr.Clear();
                ResultLists.AprRound.Clear();
                ResultLists.AprRoundTotal.Clear();
                ResultLists.May.Clear();
                ResultLists.MayRound.Clear();
                ResultLists.MayRoundTotal.Clear();
                ResultLists.Jun.Clear();
                ResultLists.JunRound.Clear();
                ResultLists.JunRoundTotal.Clear();
                ResultLists.Jul.Clear();
                ResultLists.JulRound.Clear();
                ResultLists.JulRoundTotal.Clear();
                ResultLists.Aug.Clear();
                ResultLists.AugRound.Clear();
                ResultLists.AugRoundTotal.Clear();
                ResultLists.Sep.Clear();
                ResultLists.SepRound.Clear();
                ResultLists.SepRoundTotal.Clear();
                ResultLists.Oct.Clear();
                ResultLists.OctRound.Clear();
                ResultLists.OctRoundTotal.Clear();
                ResultLists.Nov.Clear();
                ResultLists.NovRound.Clear();
                ResultLists.NovRoundTotal.Clear();
                ResultLists.Dec.Clear();
                ResultLists.DecRound.Clear();
                ResultLists.DecRoundTotal.Clear();
                ResultLists.YearCompanyTotalEsclVat.Clear();
                ResultLists.YearCompanyTotalInclVat.Clear();
                ResultLists.YearCompanyTotalInclVatRounded.Clear();
                ResultLists.MonthAllTotalEsclVat.Clear();
                ResultLists.MonthAllTotalInclVat.Clear();
                ResultLists.MonthAllTotalInclVatFromRounded.Clear();
                ResultLists.YearAllTotalEsclVat.Clear();
                ResultLists.YearAllTotalInclVat.Clear();
                ResultLists.YearAllTotalInclVatFromRounded.Clear();



                //Clearing "Endresultlists" before running through next file. Else the data will be mixed.
                EndResultLists.CompNr.Clear();
                EndResultLists.CompName.Clear();
                EndResultLists.MonthNr.Clear();
                EndResultLists.Jan.Clear();
                EndResultLists.JanRound.Clear();
                EndResultLists.JanRoundTotal.Clear();
                EndResultLists.Feb.Clear();
                EndResultLists.FebRound.Clear();
                EndResultLists.FebRoundTotal.Clear();
                EndResultLists.Mar.Clear();
                EndResultLists.MarRound.Clear();
                EndResultLists.MarRoundTotal.Clear();
                EndResultLists.Apr.Clear();
                EndResultLists.AprRound.Clear();
                EndResultLists.AprRoundTotal.Clear();
                EndResultLists.May.Clear();
                EndResultLists.MayRound.Clear();
                EndResultLists.MayRoundTotal.Clear();
                EndResultLists.Jun.Clear();
                EndResultLists.JunRound.Clear();
                EndResultLists.JunRoundTotal.Clear();
                EndResultLists.Jul.Clear();
                EndResultLists.JulRound.Clear();
                EndResultLists.JulRoundTotal.Clear();
                EndResultLists.Aug.Clear();
                EndResultLists.AugRound.Clear();
                EndResultLists.AugRoundTotal.Clear();
                EndResultLists.Sep.Clear();
                EndResultLists.SepRound.Clear();
                EndResultLists.SepRoundTotal.Clear();
                EndResultLists.Oct.Clear();
                EndResultLists.OctRound.Clear();
                EndResultLists.OctRoundTotal.Clear();
                EndResultLists.Nov.Clear();
                EndResultLists.NovRound.Clear();
                EndResultLists.NovRoundTotal.Clear();
                EndResultLists.Dec.Clear();
                EndResultLists.DecRound.Clear();
                EndResultLists.DecRoundTotal.Clear();
                EndResultLists.YearCompanyTotalEsclVat.Clear();
                EndResultLists.YearCompanyTotalInclVat.Clear();
                EndResultLists.YearCompanyTotalInclVatRounded.Clear();
                EndResultLists.MonthAllTotalEsclVat.Clear();
                EndResultLists.MonthAllTotalInclVat.Clear();
                EndResultLists.MonthAllTotalInclVatFromRounded.Clear();
                EndResultLists.YearAllTotalEsclVat.Clear();
                EndResultLists.YearAllTotalInclVat.Clear();
                EndResultLists.YearAllTotalInclVatFromRounded.Clear();

            } //Move this to the buttom of main Belongs to the loop running through the lists it belongs to main loop for fileselection
            #endregion
            //To here, the text will only be written to Console

            fileWriter.Close();
            dataOutput.Close();




            Console.WriteLine("Please press any key to exit the program");
            //Waiting for the user to hit a key to exit the program
            Console.ReadKey();
        }
    }




    //Define the 4 different lists with their different datatypes and applying company numbers and month numbers to be able to sort later
    public class DataSetLists
    {
        public static List<DateTime> Date = new List<DateTime>();
        public static List<string> Company = new List<string>();
        public static List<decimal> Turnover = new List<decimal>();
        public static List<decimal> TurnoverVatIncluded = new List<decimal>();
        public static List<decimal> TurnoverVatIncludedRound = new List<decimal>();
        public static List<int> CompanyNumber = new List<int>();
        public static List<int> MonthNumber = new List<int>();
    }




    //initialising a custom class that can contain all different datatypes from the lists in CompaniesA
    public class DataSet
    {
        public DateTime Date { get; set; }
        public string Company { get; set; }
        public decimal Turnover { get; set; }
        public decimal TurnoverVatIncluded { get; set; }
        public decimal TurnoverVatIncludedRound { get; set; }
        public int CompanyNumber { get; set; }
        public int MonthNumber { get; set; }
    }




    //defining a lot of lists to keep things apart
    public class ResultLists
    {
        public static List<int> CompNr = new List<int>();
        public static List<string> CompName = new List<string>();
        public static List<int> MonthNr = new List<int>();
        public static List<decimal> Jan = new List<decimal>();
        public static List<decimal> JanTotal = new List<decimal>();
        public static List<decimal> JanRound = new List<decimal>();
        public static List<decimal> JanRoundTotal = new List<decimal>();
        public static List<decimal> Feb = new List<decimal>();
        public static List<decimal> FebTotal = new List<decimal>();
        public static List<decimal> FebRound = new List<decimal>();
        public static List<decimal> FebRoundTotal = new List<decimal>();
        public static List<decimal> Mar = new List<decimal>();
        public static List<decimal> MarTotal = new List<decimal>();
        public static List<decimal> MarRound = new List<decimal>();
        public static List<decimal> MarRoundTotal = new List<decimal>();
        public static List<decimal> Apr = new List<decimal>();
        public static List<decimal> AprTotal = new List<decimal>();
        public static List<decimal> AprRound = new List<decimal>();
        public static List<decimal> AprRoundTotal = new List<decimal>();
        public static List<decimal> May = new List<decimal>();
        public static List<decimal> MayTotal = new List<decimal>();
        public static List<decimal> MayRound = new List<decimal>();
        public static List<decimal> MayRoundTotal = new List<decimal>();
        public static List<decimal> Jun = new List<decimal>();
        public static List<decimal> JunTotal = new List<decimal>();
        public static List<decimal> JunRound = new List<decimal>();
        public static List<decimal> JunRoundTotal = new List<decimal>();
        public static List<decimal> Jul = new List<decimal>();
        public static List<decimal> JulTotal = new List<decimal>();
        public static List<decimal> JulRound = new List<decimal>();
        public static List<decimal> JulRoundTotal = new List<decimal>();
        public static List<decimal> Aug = new List<decimal>();
        public static List<decimal> AugTotal = new List<decimal>();
        public static List<decimal> AugRound = new List<decimal>();
        public static List<decimal> AugRoundTotal = new List<decimal>();
        public static List<decimal> Sep = new List<decimal>();
        public static List<decimal> SepTotal = new List<decimal>();
        public static List<decimal> SepRound = new List<decimal>();
        public static List<decimal> SepRoundTotal = new List<decimal>();
        public static List<decimal> Oct = new List<decimal>();
        public static List<decimal> OctTotal = new List<decimal>();
        public static List<decimal> OctRound = new List<decimal>();
        public static List<decimal> OctRoundTotal = new List<decimal>();
        public static List<decimal> Nov = new List<decimal>();
        public static List<decimal> NovTotal = new List<decimal>();
        public static List<decimal> NovRound = new List<decimal>();
        public static List<decimal> NovRoundTotal = new List<decimal>();
        public static List<decimal> Dec = new List<decimal>();
        public static List<decimal> DecTotal = new List<decimal>();
        public static List<decimal> DecRound = new List<decimal>();
        public static List<decimal> DecRoundTotal = new List<decimal>();
        public static List<decimal> YearCompanyTotalEsclVat = new List<decimal>();
        public static List<decimal> YearCompanyTotalInclVat = new List<decimal>();
        public static List<decimal> YearCompanyTotalInclVatRounded = new List<decimal>();
        public static List<decimal> MonthAllTotalEsclVat = new List<decimal>();
        public static List<decimal> MonthAllTotalInclVat = new List<decimal>();
        public static List<decimal> MonthAllTotalInclVatFromRounded = new List<decimal>();
        public static List<decimal> YearAllTotalEsclVat = new List<decimal>();
        public static List<decimal> YearAllTotalInclVat = new List<decimal>();
        public static List<decimal> YearAllTotalInclVatFromRounded = new List<decimal>();
    }




    //defining a lot of lists to keep things apart (Actually the same as "ResultA" but to be sure not to mix things up i made another.
    public class EndResultLists
    {
        public static List<int> CompNr = new List<int>();
        public static List<string> CompName = new List<string>();
        public static List<int> MonthNr = new List<int>();
        public static List<decimal> Jan = new List<decimal>();
        public static List<decimal> JanTotal = new List<decimal>();
        public static List<decimal> JanRound = new List<decimal>();
        public static List<decimal> JanRoundTotal = new List<decimal>();
        public static List<decimal> Feb = new List<decimal>();
        public static List<decimal> FebTotal = new List<decimal>();
        public static List<decimal> FebRound = new List<decimal>();
        public static List<decimal> FebRoundTotal = new List<decimal>();
        public static List<decimal> Mar = new List<decimal>();
        public static List<decimal> MarTotal = new List<decimal>();
        public static List<decimal> MarRound = new List<decimal>();
        public static List<decimal> MarRoundTotal = new List<decimal>();
        public static List<decimal> Apr = new List<decimal>();
        public static List<decimal> AprTotal = new List<decimal>();
        public static List<decimal> AprRound = new List<decimal>();
        public static List<decimal> AprRoundTotal = new List<decimal>();
        public static List<decimal> May = new List<decimal>();
        public static List<decimal> MayTotal = new List<decimal>();
        public static List<decimal> MayRound = new List<decimal>();
        public static List<decimal> MayRoundTotal = new List<decimal>();
        public static List<decimal> Jun = new List<decimal>();
        public static List<decimal> JunTotal = new List<decimal>();
        public static List<decimal> JunRound = new List<decimal>();
        public static List<decimal> JunRoundTotal = new List<decimal>();
        public static List<decimal> Jul = new List<decimal>();
        public static List<decimal> JulTotal = new List<decimal>();
        public static List<decimal> JulRound = new List<decimal>();
        public static List<decimal> JulRoundTotal = new List<decimal>();
        public static List<decimal> Aug = new List<decimal>();
        public static List<decimal> AugTotal = new List<decimal>();
        public static List<decimal> AugRound = new List<decimal>();
        public static List<decimal> AugRoundTotal = new List<decimal>();
        public static List<decimal> Sep = new List<decimal>();
        public static List<decimal> SepTotal = new List<decimal>();
        public static List<decimal> SepRound = new List<decimal>();
        public static List<decimal> SepRoundTotal = new List<decimal>();
        public static List<decimal> Oct = new List<decimal>();
        public static List<decimal> OctTotal = new List<decimal>();
        public static List<decimal> OctRound = new List<decimal>();
        public static List<decimal> OctRoundTotal = new List<decimal>();
        public static List<decimal> Nov = new List<decimal>();
        public static List<decimal> NovTotal = new List<decimal>();
        public static List<decimal> NovRound = new List<decimal>();
        public static List<decimal> NovRoundTotal = new List<decimal>();
        public static List<decimal> Dec = new List<decimal>();
        public static List<decimal> DecTotal = new List<decimal>();
        public static List<decimal> DecRound = new List<decimal>();
        public static List<decimal> DecRoundTotal = new List<decimal>();
        public static List<decimal> YearCompanyTotalEsclVat = new List<decimal>();
        public static List<decimal> YearCompanyTotalInclVat = new List<decimal>();
        public static List<decimal> YearCompanyTotalInclVatRounded = new List<decimal>();
        public static List<decimal> MonthAllTotalEsclVat = new List<decimal>();
        public static List<decimal> MonthAllTotalInclVat = new List<decimal>();
        public static List<decimal> MonthAllTotalInclVatFromRounded = new List<decimal>();
        public static List<decimal> YearAllTotalEsclVat = new List<decimal>();
        public static List<decimal> YearAllTotalInclVat = new List<decimal>();
        public static List<decimal> YearAllTotalInclVatFromRounded = new List<decimal>();

    }




    //Defining fproperties for my own type of list with multiple data types
    public class Result
    {
        public int CompNr { get; set; }
        public int MonthNr { get; set; }
        public string CompName { get; set; }
        public decimal Jan { get; set; }
        public decimal JanTotal { get; set; }
        public decimal JanRound { get; set; }
        public decimal JanRoundTotal { get; set; }
        public decimal Feb { get; set; }
        public decimal FebTotal { get; set; }
        public decimal FebRound { get; set; }
        public decimal FebRoundTotal { get; set; }
        public decimal Mar { get; set; }
        public decimal MarTotal { get; set; }
        public decimal MarRound { get; set; }
        public decimal MarRoundTotal { get; set; }
        public decimal Apr { get; set; }
        public decimal AprTotal { get; set; }
        public decimal AprRound { get; set; }
        public decimal AprRoundTotal { get; set; }
        public decimal May { get; set; }
        public decimal MayTotal { get; set; }
        public decimal MayRound { get; set; }
        public decimal MayRoundTotal { get; set; }
        public decimal Jun { get; set; }
        public decimal JunTotal { get; set; }
        public decimal JunRound { get; set; }
        public decimal JunRoundTotal { get; set; }
        public decimal Jul { get; set; }
        public decimal JulTotal { get; set; }
        public decimal JulRound { get; set; }
        public decimal JulRoundTotal { get; set; }
        public decimal Aug { get; set; }
        public decimal AugTotal { get; set; }
        public decimal AugRound { get; set; }
        public decimal AugRoundTotal { get; set; }
        public decimal Sep { get; set; }
        public decimal SepTotal { get; set; }
        public decimal SepRound { get; set; }
        public decimal SepRoundTotal { get; set; }
        public decimal Oct { get; set; }
        public decimal OctTotal { get; set; }
        public decimal OctRound { get; set; }
        public decimal OctRoundTotal { get; set; }
        public decimal Nov { get; set; }
        public decimal NovTotal { get; set; }
        public decimal NovRound { get; set; }
        public decimal NovRoundTotal { get; set; }
        public decimal Dec { get; set; }
        public decimal DecTotal { get; set; }
        public decimal DecRound { get; set; }
        public decimal DecRoundTotal { get; set; }
        public decimal YearCompanyTotalEsclVat { get; set; }
        public decimal YearCompanyTotalInclVat { get; set; }
        public decimal YearCompanyTotalInclVatRounded { get; set; }
        public decimal MonthAllTotalEsclVat { get; set; }
        public decimal MonthAllTotalInclVat { get; set; }
        public decimal MonthAllTotalInclVatFromRounded { get; set; }
        public decimal YearAllTotalEsclVat { get; set; }
        public decimal YearAllTotalInclVat { get; set; }
        public decimal YearAllTotalInclVatFromRounded { get; set; }
    }


}
