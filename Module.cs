using System;
using System.Diagnostics;
using System.Collections;
namespace FortranConverter
{
	/// <summary>
	/// Determines parameters, return type, and scope
	/// of Module
	/// </summary>
	public class Module
	{
		public enum ModuleType {Function,Subroutine,Main};
		//bool IsSubroutine=false; 
		ModuleType moduleType;
		public string name;
		public bool HasParameters;
		public string[] parameters;
		public string[] lines;
		public string access = "public";
		public string returnType = "void";
		public bool IsStatic;
		public int pritateCallers=0;
		public int publicCallers=0;
		ArrayList  callers = new ArrayList();
		ArrayList  declarations = new ArrayList();
		public int index1;
		public int index2;
		SourceFile statements;
		public Module(SourceFile statements, int index, ModuleType moduleType)
		{
		this.moduleType = moduleType;
	    this.statements = statements;
			this.IsStatic = true;
		index1 = index;
		index2 = this.FindEnd(statements,index1);
			if(index2 <0)
			{
			  Console.WriteLine("Error... didn't find 'end' "+this.name);
			  Console.WriteLine("started on line "+(index1+1));
			  Console.WriteLine(statements.Lines[index1]);
			  throw new Exception("Error... can't find end of subroutine/function");
			}
		ParseNameAndParameters();
		DeterminePublicOrPrivate();
		}

		
		/// <summary>
		/// returns 'public' or 'private' or 'not used'
		/// for the subroutineName specified.
		/// searches all source files to determine
		/// if the subroutine is used.
		/// </summary>
		/// <param name="subroutineName"></param>
		/// <returns></returns>
		void DeterminePublicOrPrivate()
		{
			if(this.moduleType == Module.ModuleType.Main)
			{
				this.access ="public";
				return;
			}
			// this.access = "private"
			bool QuitEarly=false;
			for(int i=0; i<FortranConverter.fortranFiles.Length; i++)
			{
				if(QuitEarly)
					break;
				//Console.WriteLine("Searching for "+this.name+" in "+Fortran.fortranFiles[i]);
				SourceFile db = new SourceFile(FortranConverter.fortranFiles[i]);

			int line=-1; 
				int sz = db.Lines.Length;
				while(line<sz)
				{
					line++;
					int idx = db.IndexOf(this.name,line,false);
					//if (this.name.ToLower().IndexOf("c2j")>=0)
					//	Console.WriteLine("hi c2j");
					if( idx <0)
						break; // done with this file.
					if(idx >= this.index1 && idx <= this.index2 && this.statements.filename==db.filename)
					{
						line = idx;
						continue; // don't count declaration as a call
					}
					// make sure not similar name
					// character before should be ' ' or '\t'
					string s = db.Lines[idx].ToLower();
					int idx2 = s.IndexOf(this.name.ToLower());
					
					int idx3 = idx2+this.name.Length-1;
					//if(  !(s[idx2-1] == ' ' || s[idx2-1] == '\t') )
					if( Char.IsLetter(s[idx2-1]))
					{
						line = idx;
						continue;
					}
					if( s.Length >idx3+1)
						if( Char.IsLetter(s,idx3+1))  //!(s[idx3+1] == ' ' || s[idx3+1] == '(') )
						{
							line = idx;
							continue;
						}

					// character after should be '(' or ' '
					



				  this.callers.Add(db.filename+" : "+idx.ToString());
					if(this.statements.filename == db.filename)
					{
						// private calls...
						this.pritateCallers++;
						
					}
					else
					{// public calls.
						this.publicCallers++;
						QuitEarly = true;
						break;
					}
					line = idx;
				}
			}
		if( this.publicCallers >0)
			this.access = "public";
		  else
			this.access = "private";
		}

		void ParseNameAndParameters()
		{
			string text="";
			
			if (this.moduleType == ModuleType.Function)
			{
				text = "function";
			}
			else if( this.moduleType == ModuleType.Subroutine)
			{
				text = "subroutine";
				this.returnType = "void";
			}
			else if( this.moduleType == ModuleType.Main)
			{
			this.returnType = "void";
			this.name = "Main";
			this.HasParameters= true;
			this.parameters = new string[]{"args[]"};
				return;
			}
			else
				throw new Exception("Error... "+this.moduleType+" not defined");
			
			string firstLine = statements.Lines[index1].Trim();
			string strLower = firstLine.ToLower();
			int idx = strLower.IndexOf(text);
			firstLine = firstLine.Replace("\t"," ");

			string nameAndParameters = firstLine.Substring(idx+text.Length);
		
			int open = nameAndParameters.IndexOf("(");
			int close = nameAndParameters.IndexOf(")");
			if( open<0 )
			{
				this.HasParameters= false;
				this.name= nameAndParameters.Trim();
			}
			else if( close==(open+1) ) // '()' empty parens
			{	
			this.HasParameters = false;
				name = nameAndParameters.Substring(0,open).Trim();
			}
			else
			{// parse the parameters.
				this.HasParameters = true;
				if( close <0)
					throw new Exception("no closing ')'"+firstLine);
				// 'f1(1,3)'
				//    23456  
				name = nameAndParameters.Substring(0,open).Trim();
				string parms = nameAndParameters.Substring(open+1,close-open-1);
				this.parameters = parms.Split(new char[]{','});
			}
			if( this.moduleType == ModuleType.Function)
			{//complex*16 FUNCTION CZERO
				// FUNCTION RFASXTX(I)
				string r = firstLine.Substring(0,idx);
				//double precision
				if( r == "")
					this.returnType = FortranConverter.getImplicitType(name); // return type based on function name
				else
					this.returnType = FortranConverter.ConvertType(r);
			}


		}

		public void Convert() // changes lines in SourceFile to C# language
		{
			string line1 = "\t"+this.access+" "+this.returnType+" "+this.name+"(";
			if (this.moduleType == ModuleType.Main)
			{
				line1 = "\t"+this.access+" static "+this.returnType+" "+this.name+"(";
				line1 =line1+" string[] args) {";
				statements.Lines[index1]= line1;
				statements.Lines[index2] = "     } //" +statements.Lines[index2];
				return;
			}
			if( this.HasParameters)
			{
				
				for(int i=0; i<parameters.Length; i++)
				{
				line1 = line1 
					  + FortranConverter.getImplicitType(parameters[i]) 
					  + " "
					  + parameters[i]+" ";
                if (i < parameters.Length - 1)
                    line1 += ", ";
				}
			}
				line1 = line1+") {  // ";
			   
            //if( this.callers.Count == 0 )
            //{
            //    Console.WriteLine((this.index2-this.index1+1)+" lines dead:  subroutine/function "+this.name +" isn't called ");
            //    line1 = "/******  " +line1 + " No Callers!!! ";
            //}
            //else
			{
			
					line1 = line1 + " callers: ";
				for(int c=0; c<this.callers.Count; c++)
				{
					line1 = line1 + callers[c].ToString()+", ";
				}
			}
			 
			statements.Lines[index1]= line1;

			//if( this.callers.Count == 0)
				//statements.Lines[index2] = "     } //" +statements.Lines[index2]+" *******/";
			//else
			    statements.Lines[index2] = "     } //" +statements.Lines[index2];
		}


		
		/// <summary>
		/// Finds 'End' 
		/// </summary>
		/// <param name="db"></param>
		/// <param name="startIndex"></param>
		/// <returns></returns>
		int FindEnd(SourceFile db,int startIndex)
		{
			int idx2;
			int i=startIndex;
			int sz = db.Lines.Length;
			while(i <sz)
			{
				idx2 = db.IndexOf("end",i,false);
                if (idx2 < 0)
                    return -1;
				i=idx2+1;
				// make sure this isn't an end if.
				 string str = db.Lines[idx2].ToLower();
				if (str.IndexOf("end if") >=0) 
					continue;
				if( str.IndexOf("endif") >=0)
					continue;
				string[] parts = str.Split(new char[]{','});
				if (parts[0].Trim().Length != 3)
					continue; // something besides end..
				return idx2;
			}

			return -1;	      

		} 
	}
}
