using System;

namespace FortranConverter
{
	/// <summary>
	/// Summary description for DoLoop.
	/// </summary>
	public class DoLoop
	{
		FortranStatement fortranStatement;
		SourceFile statements;
		int idxEqual;
		int idxFirstcomma;
		int idxSecondcomma;
		string[] tokens;
		string firstLine;
		string str;
		public DoLoop(FortranStatement fs)
		{
		statements = fs.statements;
		this.fortranStatement = fs;
		firstLine = fs.firstLine;
		}

		public bool IsDoLoop()
		{
			// do 20 i=3,2
			// DO 22 IASF=1,NASF
			// DO 21 IASF2=IASF,NASF
			// DO 25,IASS=1,NASS
			// 233   do 12,i=3,55  // NOT IMPLEMENTED...

			if(firstLine.IndexOf("do")<0)
				return false;
			if(firstLine.IndexOf(",") <0) 
				return false;
			str = firstLine.Replace("\t"," ");
			str = str.Trim();
			tokens = str.Split();
			if(tokens.Length == 0) 
				return false;

			//bool modString = false; //
			string modMessage = "";
			if(tokens.Length ==1 || tokens[1].IndexOf(",") >0) // label is token[1]
			{// example: do 20,angle=0,(2*pi)-zinc,zinc
				// no space after label
				// replace first comma with space 
				int ic = str.IndexOf(",");
//				modString = true;
				string str2 = str.Substring(0,ic)+" "+str.Substring(ic+1);
				modMessage = "changed "+str+" to "+str2;
				str = str2;
				tokens = str2.Split();
			}
			//int idxDo = str.IndexOf("do");
			idxEqual = str.IndexOf("=");
			idxFirstcomma = str.IndexOf(",");
			idxSecondcomma = str.IndexOf(",",idxFirstcomma+1);
			
			if(  Array.IndexOf(tokens,"do")>=0 
				&& idxFirstcomma>=0
				&& idxEqual >=0	)
				return true;
			
			return false;
		}

		public void Convert()
		{
		  string label = tokens[1];
			int idxLabel = str.IndexOf(label);
			string varName = str.Substring(idxLabel+label.Length,idxEqual-(idxLabel+label.Length));
			string initialValue = str.Substring(idxEqual+1,idxFirstcomma-idxEqual-1);
			string stopValue="";
			string incrementValue = "1";
			string newLabel = "l_"+label+":";
			if (idxSecondcomma >0)
			{
				stopValue = str.Substring(idxFirstcomma+1,idxSecondcomma-idxFirstcomma);
				incrementValue = str.Substring(idxSecondcomma+1);
			}
			else
				stopValue = str.Substring(idxFirstcomma+1);
			int idx = fortranStatement.FindNextLabel(newLabel);
			if( idx < 0)
			{
			 Console.WriteLine("error didnt' find label "+label);
             return;
				//throw new Exception("error. finding label "+label);
			}

			// now modify the code.
			// replace  line numbers with labels:
			// ':' will be added to Split(':') to make as
			// if it isn't there...
			//prefix all labels with l_
			// ie. replace
			// 30  write(*,*) 'hi'    
			//with
			// l_30: write(*,*)' hi') //continue

			// DO 21 IASF2=IASF,NASF
			//string s = this.firstLine.ToLower().Replace("\t"," ");
		//	int idxDo = s.IndexOf("do");
			string forStatement = "       ";
		//for(int c=0; c<=idxDo; c++)
		//		forStatement += " "; // indent like original
			forStatement += "for( "+FortranConverter.getImplicitType(varName);
			forStatement += varName + " = ";
			forStatement += initialValue+"; "+varName+" <= ";
			forStatement += stopValue+"; "+varName+" += "+incrementValue;
			forStatement += ") { ";
			statements.EOLComment[fortranStatement.LineIndex] =" // "+statements.Lines[fortranStatement.LineIndex];
			statements.Lines[fortranStatement.LineIndex] = forStatement;
			//Console.WriteLine(db.Lines[i]); 
			
			string strEndDo = statements.Lines[idx];
			
			if( strEndDo.IndexOf(newLabel) <0 )
			{
				strEndDo =strEndDo.Replace(label,newLabel).ToLower();
			}
				int idxContinue  = strEndDo.IndexOf("continue");
				if( idxContinue >0)
				{
					strEndDo =strEndDo.Replace("continue","}"); 
					statements.EOLComment[idx] = " // continue";
				}
				else 
					strEndDo += " }";

				statements.Lines[idx] =strEndDo;
			
			// insert closing bracket '}' of for loop.
			// change label name  from 30 to l_30:
			//Console.WriteLine(db.Lines[idx]);

			}



	}
}
