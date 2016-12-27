using System;
using System.Text.RegularExpressions;
namespace FortranConverter
{
	public class BasicConversions
	{
		/// <summary>
		/// Handles Single Line statements:
		///    Comments
		///    I/O
		///    line numbers
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		public static void SingleLine(SourceFile db)
		{
			int sz = db.Lines.Length;
			for(int i=0; i<sz; i++)
			{
				if( Expressions.IsCommentLine(db.Lines[i], out db.Lines[i]))
				{ 
					db.IsWholeLineComment[i] =true;
                    continue;
				}

				if(!db.IsWholeLineComment[i])
				{
					db.Lines[i] = Expressions.CommentEndOfLine(db.Lines[i]);
					db.Lines[i] = Expressions.LineNumber(db.Lines[i]);
					db.Lines[i] = Expressions.Goto(db.Lines[i]);
					db.Lines[i] = Expressions.Stop(db.Lines[i]);
                    db.Lines[i] = Expressions.RemoveContinuationCharacter(db.Lines[i]);
                    db.Lines[i] = Expressions.Assignment(db.Lines,i);
                    db.Lines[i] = Expressions.LogicalOperators(db.Lines[i]);
                    db.Lines[i] = Expressions.SingleQuotesToDouble(db.Lines[i]);
                    db.Lines[i] = Expressions.IfStatements(db.Lines[i]);
                    db.Lines[i] = Expressions.FloatingPointNumber(db.Lines[i]);
                    db.Lines[i] = Expressions.ReturnStatement(db.Lines[i]);
                    db.Lines[i] = Expressions.ReplaceExponent(db.Lines[i]);
                    
				}
			}

			//return db;
		}




        internal static void ToLower(string[] lines)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].ToLower();
            }
        }

        internal static void CommentCommonBlocks(string[] lines)
        {

            for (int i = 0; i < lines.Length; i++)
            {
                if( Regex.IsMatch(lines[i],@"\s*common",RegexOptions.IgnoreCase))
                  lines[i]= "//"+lines[i];
            }
        }
        internal static void CommentDimension(string[] lines)
        {

            for (int i = 0; i < lines.Length; i++)
            {
                 if( Regex.IsMatch(lines[i],@"\s*dimension",RegexOptions.IgnoreCase))
                  lines[i]= "//"+lines[i];

            }
        }
    }
}
