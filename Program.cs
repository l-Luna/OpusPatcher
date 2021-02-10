using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.IO;
using System.Linq;

namespace OpusPatcher {
	class Program {
		static void Main(string[] args) {
			// args[0] is EXE, args[1] is addresses to be dumped
			Console.WriteLine("Loading OM Executable: " + args[0]);
			ReaderParameters @params = new ReaderParameters {
				ReadWrite = true
			};
			using(var module = ModuleDefinition.ReadModule(args[0], @params)) {
				Console.WriteLine("Patching string dumper...");
				// We'll iterate through all numbers to be dumped, and dump them
				string[] dumpingFile = File.ReadAllLines(args[1]);

				var mainMethod = module.FindMethod("#=q_e3xs_z$8e0$rkwhnKecaw==", "#=qikcmb9yEAoK5cFTS8A4N7Q==");
				var proc = mainMethod.Body.GetILProcessor();
				var first = proc.Body.Instructions.First();

				var parse = module.FindMethod("#=qQ3boY4a6o2O2sPtKvJtj_Q6y77XoLuLRv$4EsOcRQr4=", "#=qhwVTryR65imID$n_uKTBPA==");
				var concat = module.ImportReference(typeof(string).GetMethod("Concat", new Type[]{typeof(string), typeof(string), typeof(string)}));
				var streamWriterConstructorInfo = typeof(StreamWriter).GetConstructor(new Type[]{typeof(string)});
				var writeLine = module.ImportReference(typeof(StreamWriter).GetMethod("WriteLine", new Type[]{typeof(string)}));
				var dispose = module.ImportReference(typeof(StreamWriter).GetMethod("Dispose"));
				var writeToConsole = module.ImportReference(typeof(Console).GetMethod("WriteLine", new Type[]{typeof(string)}));

				proc.InsertBefore(first, proc.Create(OpCodes.Ldstr, "./out.csv"));
				proc.InsertBefore(first, proc.Create(OpCodes.Newobj, module.ImportReference(streamWriterConstructorInfo)));
				foreach(string num in dumpingFile) {
					proc.InsertBefore(first, proc.Create(OpCodes.Dup));
					proc.InsertBefore(first, proc.Create(OpCodes.Ldstr, num));
					proc.InsertBefore(first, proc.Create(OpCodes.Ldstr, "~,~"));
					proc.InsertBefore(first, proc.Create(OpCodes.Ldc_I4, int.Parse(num)));
					proc.InsertBefore(first, proc.Create(OpCodes.Call, parse));
					proc.InsertBefore(first, proc.Create(OpCodes.Call, concat));
					proc.InsertBefore(first, proc.Create(OpCodes.Callvirt, writeLine));
				}
				proc.InsertBefore(first, proc.Create(OpCodes.Callvirt, dispose));
				proc.InsertBefore(first, proc.Create(OpCodes.Ret));

				module.Write("./Lightning.exe");
			}
			Console.WriteLine("Dumping entries...");
			// run ./Lightning.exe
			var pr = System.Diagnostics.Process.Start(Environment.CurrentDirectory + "/Lightning.exe");
			pr.WaitForExit();
			// run Modded Opus if it exists here, pass in EXE, mappings, out.csv, to get a decompiled output
			// recompile ourselves (on the correct version with libs etc)
			// args[0] is EXE, args[2] is mappings, ./out.csv is just here, args[3] is patch.diff
			if(args.Count() > 3 && File.Exists(Environment.CurrentDirectory + "/Modded Opus.exe")) {
				Console.WriteLine("Running decompiler...");
				var argstr = "\"" + args[0] + "\" \"" + args[2] + "\" \"./out.csv\" \"" + args[3] + "\"";
				var proc2 = System.Diagnostics.Process.Start(Environment.CurrentDirectory + "/Modded Opus.exe", argstr);
				proc2.WaitForExit();
				Console.WriteLine("Recompilation output: " + proc2.StandardOutput.ReadToEnd());
			}
			Console.WriteLine("Done!");
		}
	}
}
