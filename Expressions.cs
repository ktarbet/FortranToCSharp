using System;
using System.Text.RegularExpressions;

namespace FortranConverter
{
	/// <summary>
	/// Summary description for Expressions.
	/// </summary>
	public static class Expressions
	{

        internal static string IfStatements(string line)
        {

            // ELSE IF( ... ) THEN
            if (Regex.IsMatch(line, @"else\s+if\(.*\)\s*then", RegexOptions.IgnoreCase))
            {
                line = Regex.Replace(line, @"else\s+if\(", "} else if(", RegexOptions.IgnoreCase);
                line = Regex.Replace(line, "then", "{ ", RegexOptions.IgnoreCase);
            }
            else // IF (...) THEN
                if (Regex.IsMatch(line, @"\s*if\(.*\)\s*then", RegexOptions.IgnoreCase))
                {
                    line = Regex.Replace(line, @"if\(", "if(", RegexOptions.IgnoreCase);
                    line = Regex.Replace(line, @"then", " {", RegexOptions.IgnoreCase);
                }
                else // ELSE
                    if (Regex.IsMatch(line, @"\s*else", RegexOptions.IgnoreCase))
                    {
                        line = Regex.Replace(line, @"\s*else", "} else { ", RegexOptions.IgnoreCase);
                    }
                    else  // IF
                        if (Regex.IsMatch(line, @"if\s*\(", RegexOptions.IgnoreCase))
                        {
                            line = Regex.Replace(line, @"if\(", "if(", RegexOptions.IgnoreCase);
                        }
                        else
                            if (Regex.IsMatch(line, "end if|endif", RegexOptions.IgnoreCase))
                            {
                                line = Regex.Replace(line, "endif", "} ", RegexOptions.IgnoreCase);
                                line = Regex.Replace(line, "end if", "} ", RegexOptions.IgnoreCase);
                            }

            return line;
        }

                public static string SingleQuotesToDouble(string line)
        {
            string rval = Regex.Replace(line,
                @"\'", "\"", RegexOptions.IgnoreCase);
            return rval;
        }

        public static string LogicalOperators(string inputLine)
        {
            string rval = Regex.Replace(inputLine,
                @"\.eq\.", " == ", RegexOptions.IgnoreCase);

            rval = Regex.Replace(rval, @"\.eq\.", " == ", RegexOptions.IgnoreCase);
            rval = Regex.Replace(rval, @"\.gt\.", " > ", RegexOptions.IgnoreCase);
            rval = Regex.Replace(rval, @"\.lt\.", " < ", RegexOptions.IgnoreCase);
            rval = Regex.Replace(rval, @"\.le\.", " <= ", RegexOptions.IgnoreCase);
            rval = Regex.Replace(rval, @"\.ge\.", " >= ", RegexOptions.IgnoreCase);
            rval = Regex.Replace(rval, @"\.ne\.", " != ", RegexOptions.IgnoreCase);

            rval = Regex.Replace(rval, @"\.and\.", " && ", RegexOptions.IgnoreCase);
            rval = Regex.Replace(rval, @"\.or\.", " || ", RegexOptions.IgnoreCase);
            rval = Regex.Replace(rval, @"\.not\.", " ! ", RegexOptions.IgnoreCase);
            return rval;
        }

        
        public static string RemoveContinuationCharacter(string input)
        {
            return Regex.Replace(input,
                @"(^\t\d)|(^[ ]{5}\w)",
                "       ",
                RegexOptions.IgnoreCase);
        }

		public static bool IsCommentLine(string input, out string output)
		{
			string s;
			bool rval = Regex.IsMatch(input,@"^[c!]",RegexOptions.IgnoreCase);
			if( rval)
			{
				s = "//"+input.Substring(1);
				output = s;
			}
			else output = input;
			return rval;
		}

		public static string CommentEndOfLine(string input)
		{
			int idx = input.IndexOf("!");
			if( idx >0)
				return input.Substring(0,idx);// delete comments at end of line
			return input;
		}

		public static string Stop(string input)
		{
		  return Regex.Replace(input,
				"(?<stop>stop)",
				"//${stop}",
				RegexOptions.IgnoreCase);
		}

		public static string Goto(string input)
		{
			return Regex.Replace(input,
				@"(?<goto>goto\s+|go\s+to\s+)(?<label>\d+)","goto l_${label};",
				RegexOptions.IgnoreCase);

		}

		public static string LineNumber(string input)
		{

			return Regex.Replace(input,
				@"(?<label>^\d+)\s",
				"l_${label}: ",
				RegexOptions.IgnoreCase);
		}

		// just putting ';' on the end of assignment
		public static string Assignment(string[]lines , int index )//string input)
		{
            string input = lines[index];
            string expr = @"\s*[\w(),]+\s*\=\s*";
			
			Regex rx = new Regex(expr);
			if( rx.IsMatch(input))
			{
                /// TO DO multi line assignment.
				if( input.IndexOf(";") >=0)
					return input;
			return input+";";	

			}

			return input;			
		}




        internal static string FloatingPointNumber(string line)
        {
            var s =  Regex.Replace(line, @"(\D\d+\.)(\D|$)", "${1}0${2}", RegexOptions.IgnoreCase);
            // perform twice
            return Regex.Replace(s, @"(\D\d+\.)(\D|$)", "${1}0${2}", RegexOptions.IgnoreCase);
        }

        public static string ReturnStatement(string line)
        {
            return Regex.Replace(line, @"(\sreturn(\s|$))", "${1};", RegexOptions.IgnoreCase);
        }


        public static string ReplaceExponent(string line)
        {
            // simple case...  includes paranethesis (x2**3)
            return Regex.Replace(line, @"\((\w+)\*\*(\d+)\)", "Math.Pow(${1},${2})", RegexOptions.IgnoreCase);
        }

    }
}
