using System;
using System.Collections;
using System.Text.RegularExpressions;
namespace FortranConverter
{
	/// <summary>
	/// A Declaraion defines the data type.
	/// int, double, complex, string
	/// 
	/// A Declaration might also have an assignment
	/// on the right hand side.  
	/// 
	///
	/****  Examples *******
	complex*16 CWL(0:NW)
	logical LWellDef
	common /points/    // multi line common statement.
	.	NWL,			!number of points
	.   abc
	INTEGER            IA,IB,N,MA,M,IJOB,IER                          
	double precision               A(IA,N),B(IB,M),WK(N,3) 
			   
	i=12  // requires checking if  i is allready defined...
	if allready define.
	i=12;
	else
	int i = 12;
    ***************/
	/// </summary>
	public class Declaration
	{


		public static string Convert(string input)
		{

			//input = input.ToLower();
			//Console.WriteLine("input: "+input);
			for(int i=0; i<FortranConverter.Types.Length; i++)
			{
				string strTypeExp = FortranConverter.Types[i].Replace("*",@"\*\d*");
				string exp = @"\s*"+strTypeExp+@"\s";
				Regex r = new Regex(exp);
				Regex r2 = new Regex(@"\sfunction\s"); 

				if( r.IsMatch(input) )
				{ 
					if(r2.IsMatch(input))
					{
						// function declaration.
					}
					else
					{
						// basic declaration
						string newType = FortranConverter.ConvertType(FortranConverter.Types[i]);
						//Console.WriteLine(" declaration of "+FortranConverter.Types[i]);
						
						//string output = input.Replace(Fortran.Types[i],newType)+";";
						Regex r3 = new Regex(strTypeExp);
						string output = r3.Replace(input,newType);

						if( newType == "string")
						{
							// change   string  str*4,  name , address*55
							// to       string   str, name, address
							Regex r4 = new Regex(@"\*\d*");
						//Console.WriteLine(	r4.IsMatch(output));
						output =  r4.Replace(output,"");
						}

						//Console.WriteLine("modified: "+output);
						return output+";";
					}
				}

			}
			return input;

		}
		/*
		public string type=""; // "constant", "double", "string"
							//"double[]"
		                      // "double[][]", complex
		public string name="";

		public Declaration(string name, string type)
		{
			this.type = type;
			this.name = name;
		}
		public static void AddDeclarations(FortranStatement fs, ArrayList declarations)
		{
			
			if( fs.NonBlankTokens.Length ==0)
				return;
			// is first word is a valid fortran type
			// explicit definition
			 // example :  integer a,b,c
			//    int a,b,c;
			// hard example:   complex*16 CASK(NAP,0:NA),CTRSK(0:NA)

			// complex[,] cask = new complex[nap+1,na]; complex[,] ctrsk = new complex[na];
			if(Fortran.IsValidType(fs.NonBlankTokens[0].Trim()))
			{	
				int idx1 = fs.firstLine.IndexOf("(");
				if( idx1 <0)
				{ // no array size declaration here.
					string type =Fortran.ConvertType(fs.NonBlankTokens[0].Trim()); 
					// add to list of declarations.
					for(int i=1; i<fs.NonBlankTokens.Length; i++)
					{
						string name =fs.NonBlankTokens[i];
						declarations.Add(new Declaration(name ,type));
						Console.WriteLine(type+" "+name);
					}
				}
				else
				{


				}
			}


		}

		public string[] Tokens(string input)
		{
			return new string[] { "a"}	;

		}
		*/
	}
}
