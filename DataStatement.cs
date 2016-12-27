using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FortranConverter
{
    class DataStatement
    {
        /*        //data ipass /0/

                C	C1 FOR FORECAST 100 TO 500 KAF
            DATA C1 /-.8615714E+02, 0.5722038E+00, 0.4079687E+00
            1       ,-.1940498E-02, -.1167166E-02, -.7284199E-03
            2       ,0.2760540E-05, 0.2550345E-06, 0.3057543E-05
            3       ,0.2197219E-06, 0.,0.,0.,0.,0.,0.,0.,0.,0.,0.
            4,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0./
        C	C2 FOR FORECAST 500 TO 1000 KAF
            DATA C2 /0.2500182E+03, -.1111612E+01, -.4806429E+00
            1       ,0.1068387E-02, 0.1595443E-02, 0.1139242E-03
            2       ,-.1020313E-05, -.3357045E-06, 0.1640095E-05
            3       ,-.1729406E-06, 0.,0.,0.,0.,0.,0.,0.,0.,0.,0.
            4,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0.,0./
                    DIMENSION FC(12),FCA(12)
        C
        C     average month-july natural runoff
        C
              DATA FCA /134200.,130200.,126100.,121500.,117200.,113100.,
             1 105200.,75600.,39400.,12000.,0.,0./
        C
         * 
         * 
         *       DATA NAME /			'HEII,HEII,JCK1,PAL,2047000,',
             1 'JCK,JCK,JCK,847000,',		'ISLI,ISLI,ISL,135200,',
             2 'RIR,RIR,RIR,80500,',		'WODI,WODI,WOD,30000,',
             3 'LUC,LUC,AND,ARK,LUC,949700,',	'ANDI,ANDI,AND,413100,',
             4 'HRSI,HRSI,CSC,DED,800452,',	'CSCI,CSCI,CSC,646460,',
             5 'DEDI,DEDI,DED,153992,',		'PRVO,PRVO,PRV,148633,',
             6 'OWY,OWY,OWY,715000,',		'HGH,HGH,HGH,3467179,',
             8 'PARW,PARW,KEE,KAC,CLE,BUM,RIM,1065700,',
             9 'YUMW,YUMW,KEE,KAC,CLE,833700,',	'KEE,KEE,KEE,157800,',
             1 'KAC,KAC,KAC,239000,',		'CLE,CLE,CLE,436900,',
             3 'NACW,NACW,BUM,RIM,232000,',	'BUM,BUM,BUM,34000,',
             4 'RIM,RIM,RIM,198000,',		'PODO,PHL,PHL,73500,',
             5 'AGWA,AGWA,WAR,BEU,250900,',	'WARO,WARO,WAR,169600,',
             6 'BEUO,BEUO,BEU,59900,',		'BUL,BUL,BUL,23700,',
             7 'MCKO,MCKO,MCK,65500,',		'UNY,UNY,UNY,25200,', !jdoty 3/12/97
             8 'SCOO,SCOO,SCO,53600,',		'WLDN,WLDN,WLD,71500,',
             9 'WICO,WICO,CRA,WIC,255300,',	'CREO,CREO,CRE,86900,',
             1 'BENO,BENO,CRA,WIC,CRE,342200,',	'OCHO,OCHO,OCH,44460,',
             3 'AMFI,AMFI,AMF,1672590,',	'TDAO,GCL,GCL,5185000,', !jdoty 4/16/97
             4 'MFDO,MFDO,EMI,39000,'/

                */
        internal static void Convert(SourceFile src, int index)
        {

            if (src.Lines[index].IndexOf("data") < 0)
            {
                return;
            }

            Regex re1 = new Regex(@"\s+data\s+(?<name>\w+)\s+/\s*(?<data>.+$)");

            var m = re1.Match(src.Lines[index]);

            if (m.Success)
            {
                string data = m.Groups["data"].Value;
                string name = m.Groups["name"].Value;

                var cstype = EstimateDataType(data);
                if (data.IndexOf(",") < 0) // single data point
                {
                    src.Lines[index] = cstype + " "
                          + name + " = " + data + " ;";
                }
                else
                { // comma separated.

                    src.Lines[index] = cstype + "[] " + name + " = { " + data + " ";
                    if (src.Lines[index].IndexOf("/") > 0)
                    {
                        src.Lines[index] = src.Lines[index].Replace("/","};");  // single line Data Statement.
                    }
                    else // multi line data statement.
                    {
                        int idx2 = src.IndexOf("/", index + 1, true);
                        if (idx2 > index && idx2 < src.Lines.Length)
                            src.Lines[idx2] = src.Lines[idx2].Replace("/", "};");
                    }
                }
            }
        }

        private static object EstimateDataType(string data)
        {
            if (Regex.IsMatch(data, "[\"\']"))
                return "string";
            if (Regex.IsMatch(data, @"\."))
                return "double";

                return "int";

        }
    }
}