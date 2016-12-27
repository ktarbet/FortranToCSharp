using System;
using System.Text.RegularExpressions;
namespace FortranConverter
{
	public static class FortranConverter
	{
		/// <summary>
		/// Converts Fortran statements to C#
		/// </summary>
		/// <param name="lines"></param>
		public static void Convert(SourceFile statements)
		{
			int sz = statements.Lines.Length;
		
			Module module = null;

            BasicConversions.CommentCommonBlocks(statements.Lines);
            BasicConversions.CommentDimension(statements.Lines);
            BasicConversions.ToLower(statements.Lines);
			BasicConversions.SingleLine(statements);

			for(int i=0; i<sz; i++)
			{
				if(statements.IsWholeLineComment[i])
					continue;

                if (statements.Lines[i].Trim().Length == 0)
					continue;
				FortranStatement fs = new FortranStatement(statements,i);

				if( fs.IsTypeDeclaration())
				{
                    statements.Lines[i] = Declaration.Convert(statements.Lines[i]);
				}

                DataStatement.Convert(statements, i);



				bool IsFunc = fs.IsFunctionDeclaration();
				bool IsSub  = fs.IsSubroutineDeclaration();
				bool IsMain = fs.IsProgramDeclaration();
				if( IsFunc || IsSub || IsMain)
				{
                    //// find end of function
                    //// search other files to know
                    //// if  function is unused, or public, or private
                    //// if function is unused comment it out.
                    //if( IsFunc)
                    //module = new Module(statements,i,Module.ModuleType.Function);
                    if (IsSub)
                    {
                        module = new Module(statements, i, Module.ModuleType.Subroutine);
                        module.Convert();
                    }
                    //else if( IsMain)
                    //module = new Module(statements,i,Module.ModuleType.Main);
	
                    
					//line = statements.Lines[i];
					continue;
				}
	
			/*  	
				if(fs.IsIOStatement())
				{
					IO.Convert(fs);
					IsConverted[i] = true;
					continue;
				}
			*/

				

				DoLoop doLoop = new DoLoop(fs);
                if (doLoop.IsDoLoop())
                {
                    doLoop.Convert();
                    //line = statements.Lines[i];
                }
                //statements.Lines[i]  = line;

				//statements.Lines[i] = ConvertComment(line); // catch any end of statement comment '!'
			}
		}



		
		/// <summary>
		/// 
		/// checks if type is a valid fortran type
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool IsValidType(string type)
		{
			string cType = FortranConverter.ConvertType(type);
			if( cType == "unknown")
				return false;
			return true;
		}

		public static string[] Types ={ "real","character","character*","integer","double precision","complex","double complex","complex*8","complex*16","logical"};

		/// <summary>
		/// converts fotran type to c# type.
		/// example:   'Integer'  int
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string ConvertType(string type)
		{
			//System.Dr
			string rval = "unknown";
			string s = type.Trim().ToLower();
			if(s.IndexOf("character*")==0)
				return "string";
			switch( s){
				case  "integer":
					rval = "int";
					break;

				case "double precision":
					rval = "double";
					break;
				case "double_precision":
					rval = "double";
					break;
				case "logical" :
					rval = "bool";
					break;
				case "real" :
					rval = "float";
					break;
				case "complex":
					 rval = "complex";
					break;
				case "double complex":
					rval = "complex";
					break;
				case "complex*16":
					rval = "complex";
					break;
				case "complex8":
					rval = "complex8";
					break;
			}
		return rval;
		}

		/// <summary>
		/// retruns  variable type based on fortran converntion
		/// example: isum --> int
		/// </summary>
		/// <param name="varableName"></param>
		/// <returns></returns>
		public static string getImplicitType(string varableName)
		{
		 string var = varableName.Trim().ToLower()[0].ToString();
			// int (I,j,k,l,m,n) 
			if(var.IndexOfAny(new char[]{'i','j','k','l','m','n'})>=0)
				return "int";
			else
				return "double";
		}

		public static string[] fortranFiles ={};


        internal static void CustomReplaceAll(string a, string p_2)
        {

            
        }
    
    internal static void CustomReplaceAll(string[] lines,string pattern,string replacement)
    {
        for (int i = 0; i < lines.Length; i++)
			    {
                    lines[i] = Regex.Replace(lines[i], pattern, replacement, RegexOptions.IgnoreCase);			 
			    }
    }

    }
}
