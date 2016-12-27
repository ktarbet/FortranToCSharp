using System;
using System.Text.RegularExpressions;

namespace FortranConverter
{
	/// <summary>
	/// Summary description for FortranStatement.
	/// </summary>
	public class FortranStatement
	{
		public string firstLine;
		string firstLineToLower;
		public Module function;
		public SourceFile statements;
		int index;
		string[] tokens;
		public int Index { get { return index;}}
		/// <summary>
		/// basic info about fortran statement.
		/// passing array, to handle multi line statements
		/// later...
		/// </summary>
		/// <param name="lines"></param>
		/// <param name="index"></param>
		public FortranStatement(SourceFile statements, int index)
		{
			this.statements = statements;
			this.index = index;
			//statements.Lines[index] = PreProcess(statements.Lines[index]);
			firstLine = statements.Lines[index]; //.ToLower();//.Replace("\t","      ");
			tokens = firstLine.Split(new char[]{',',' '});
			firstLineToLower = firstLine.ToLower();
		}

		/// <summary>
		/// simplify processing with some basic
		/// first steps:
		/// 
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		//string PreProcess(string str)
		//{
		 // should check for comment first?
			//str = str.ToLower();
//		return str.Replace("double precision","double_precision");
	//	}

		public int LineIndex
		{
			get { return this.index;}
		}

		public string[] NonBlankTokens
		{
			get
			{
				System.Collections.ArrayList list = new System.Collections.ArrayList();
				for(int i=0; i<tokens.Length; i++)
				{	
					//tokens[i] = tokens[i].Replace(
					if( tokens[i] != "" && tokens[i] != "\t")
					{
						list.Add(tokens[i].Trim());
					}
				}
				string[] rval = new string[list.Count];
				list.CopyTo(rval);
				return rval;
			}
		}

		public bool HasLineNumber()
		{
			string[] words = this.NonBlankTokens;
			if( this.NonBlankTokens.Length ==0)
				return false;
			string firstWord = this.NonBlankTokens[0];
			
			for(int i=0;i<firstLine.Length; i++)
			{
				if( !Char.IsNumber(firstLine,i))
					return false;
			}

			return true;
		}

	public bool	IsSubroutineDeclaration()
	{
		string str1 = this.firstLine.ToLower();
		str1 = str1.Replace("\t","         ").Trim();//remove tabs
		int idxsub = str1.IndexOf("subroutine");
		if( idxsub <0 )
			return false;

		
		string[] declaration = str1.Split();
		// fisrt word should be 'subroutine'
		if( !(declaration[0].Trim() == "subroutine"))
			return false;
	  return true;
	}


		public bool IsTypeDeclaration()
		{
			if(IsFunctionDeclaration())
				return false;
		if(this.NonBlankTokens.Length<=0	)
			return false;
			string s = this.NonBlankTokens[0];
		return FortranConverter.IsValidType(s);
		}

		/// <summary>
		/// beginning statement in Fortran program
		/// example:
		///         program Jacobian
		/// </summary>
		/// <returns></returns>
		public bool IsProgramDeclaration()
		{
			Regex rex = new Regex(@"\sprogram\s");
			return rex.IsMatch(this.firstLineToLower);
		
		}
		public bool IsFunctionDeclaration()
		{
			if( HasLineNumber())
				return false;

			/* some examples..
			double precision FUNCTION ASAREA(IAS)
			LOGICAL FUNCTION INAS(CZ,NUMNODES,IAS)
			complex*16 FUNCTION CDSNKT(CZ,IAS)
			Integer Function NewCircle(iObjectID)
			FUNCTION RNEXTNUM(ARRAY,LENGTH)
			*/
			int idx = firstLine.IndexOf("function");
			if(idx <6)
				return false;

			// check that everything before
			// the word 'function' is a valid fortran type.
			//adfasdfasdfsdf start heare...
			string returnType = firstLine.Substring(0,idx).Trim();
			
			if( returnType == "")
				return true; // implicit type
			if (!FortranConverter.IsValidType(returnType))
				return false;
			return true;
		}

		public bool IsIOStatement()
		{
			
			Regex rex = new Regex(@"\s\s*write\s*\(");
			if( rex.IsMatch(firstLineToLower))
			return true;

			return false;
		}


		void TrimTokens(string[] tok)
		{
			for(int i=0; i<tok.Length; i++)
				tok[i] = tok[i].Trim();
		}

		public bool IsInclude()
		{
		if(NonBlankTokens.Length==0)
			return false;
			if(NonBlankTokens[0].Trim() == "include")
				return true;
			return false;
		}

		


		/// <summary>
		/// search forward starting at idx
		/// </summary>
		/// <param name="label"></param>
		/// <param name="idx1"></param>
		/// <returns></returns>
		public int FindNextLabel(string label)
		{
			int idx1 = this.index+1;
			int idx2;
			
            //if( this.function == null)
            //{
            //    Console.WriteLine("Error.. trying to find label "+label);
            //    Console.WriteLine(this.index+" "+this.firstLine);
            //    return -1;
            //}
            do
            {
                idx2 = statements.IndexOf(label, idx1, false);
                if (idx2 >= statements.Lines.Length || idx2 < 0)
                {
                    Console.WriteLine("Error: finding label " + label);
                    Console.WriteLine(statements.Lines[idx1 - 1]);
                    Console.WriteLine(statements.filename + " started at " + idx1);
                    Console.WriteLine("now at index " + idx2);
                    //throw new Exception("error. finding label " + label);
                    return -1;
                }
                string str2 = statements.Lines[idx2].Replace("\t", " ").Trim().ToLower();
                string[] endTokens = str2.Split(new char[] { ' ' });
                int icont = Array.IndexOf(endTokens, "continue");
                int ilabel = Array.IndexOf(endTokens, label);
                if (ilabel >= 0 && icont > 0)
                    return idx2;
                if (ilabel == 0)
                    return idx2;

                if (Array.IndexOf(endTokens, "l_" + label + ":") >= 0) // label allready converted
                    return idx2;
                idx1++;
            } while (idx2 < statements.Lines.Length); // function.index2);

			return -1; //this.index2+1;
		}

	}
}
