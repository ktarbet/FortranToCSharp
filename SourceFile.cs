using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Data;

namespace FortranConverter
{
	/// <summary>
	/// Class for reading flat text files. 
	/// 	/// </summary>
	public class SourceFile 
	{
		string[] lines;
		public string[] EOLComment;
		public bool[] HasEOLComment;
		public bool[] IsWholeLineComment;
		public string filename;
		public string[] Lines
		{
			get
			{
				return lines;
			}
		}
		/// <summary>
		/// returns line number index to string
		/// </summary>
		/// <param name="s"></param>
		/// <param name="CaseSensitive"></param>
		/// <returns></returns>
		public int IndexOf(string s, int startIndex,bool CaseSensitive)
		{
			int sz = lines.Length;
			for(int i=startIndex; i<sz; i++)
			{
			string str;
				if( !CaseSensitive)
				{
					s = s.ToLower();
					str = lines[i].ToLower();
				}
				else
					str = lines[i];

			if(str.IndexOf(s)>=0)
				return i;
				
			}
			return -1;
		}
	
		public SourceFile(string filename)
		{
			ReadFile(filename);
		}

        public SourceFile(string[] data)
        {
            this.lines = data;
            Init();
        }


		void ReadFile(string filename)
		{
			this.filename = filename;
            lines = File.ReadAllLines(filename);
            Init();
		}

        private void Init()
        {
            this.IsWholeLineComment = new Boolean[lines.Length];
            this.HasEOLComment = new Boolean[lines.Length];
            this.EOLComment = new string[lines.Length];
        }
		public void WriteFile(string filename)
		{
            File.WriteAllLines(filename, lines);
		}
		
	}
}
