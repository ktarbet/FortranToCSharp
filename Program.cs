using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace FortranConverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 2 && args.Length != 3)
            {
                Console.WriteLine("Usage: input.for out.cs [include.txt]");
                return;
            }
            SourceFile sf = new SourceFile(args[0]);
            FortranConverter.Convert(sf);

            // custom for fcsub.

            FortranConverter.CustomReplaceAll(sf.Lines,@"freq\(([a-zA-Z]+,[a-zA-Z]+)\)", "freq[${1}]");
            FortranConverter.CustomReplaceAll(sf.Lines, "cb == \"([a-zA-Z]+)\\s*\"", "cb == \"${1}\"");
            FortranConverter.CustomReplaceAll(sf.Lines, @"call (poly\([\w,]+,)req\)","${1}out req);");

            List<string> lst = new List<string>();
            lst.Add("static class "+Path.GetFileName(Path.GetFileNameWithoutExtension(args[1])));
            lst.Add(" {");

            if (args.Length == 3)
            {
                lst.AddRange(File.ReadAllLines(args[2]));
            }
            lst.AddRange(sf.Lines);
            lst.Add(" }");
            File.WriteAllLines(args[1], lst.ToArray());
        }

        
    }
}
