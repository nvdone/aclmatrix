//NVD ACLMatrix
//Copyright © 2016-2021, Nikolay Dudkin

//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<https://www.gnu.org/licenses/>.

using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;

namespace aclmatrix
{
	class Program
	{
		private static GroupsCache groupsCache;
		private static SubjectsCache subjectsCache;

		private static PrincipalContext domainPrincipalContext;
		private static PrincipalContext machinePrincipalContext;

		private static bool checkFiles;

		public static void Main(string[] args)
		{
			Console.Out.WriteLine(string.Format("{0} {1}\r\n{2}", ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute))).Title, Assembly.GetExecutingAssembly().GetName().Version, ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute))).Copyright));

			if (args.Length < 2)
			{
				Console.Out.WriteLine("\r\nUsage: ACLMatrix.exe root_path output_file.xlsx [HideUsers] [HideGroups] [HideUnrecognized] [ShowAccountNames] [BypassACL] [CheckFiles]");
				return;
			}

			bool hideUsers = false;
			bool hideGroups = false;
			bool hideUnrecognized = false;
			bool addAccountNames = false;
			bool bypassACL = false;
			checkFiles = false;

			for(int i = 2; i < args.Length; i++)
			{
				if (args[i].Equals("hideusers", StringComparison.InvariantCultureIgnoreCase))
					hideUsers = true;
				if (args[i].Equals("hidegroups", StringComparison.InvariantCultureIgnoreCase))
					hideGroups = true;
				if (args[i].Equals("hideunrecognized", StringComparison.InvariantCultureIgnoreCase))
					hideUnrecognized = true;
				if (args[i].Equals("showaccountnames", StringComparison.InvariantCultureIgnoreCase))
					addAccountNames = true;
				if (args[i].Equals("bypassacl", StringComparison.InvariantCultureIgnoreCase))
					bypassACL = true;
				if (args[i].Equals("checkfiles", StringComparison.InvariantCultureIgnoreCase))
					checkFiles = true;
			}

			if (bypassACL)
			{
				if(TokenPrivileges.SetBackupPrivilege())
					Console.Out.WriteLine("\r\nBypassACL: SUCCESS");
				else
					Console.Out.WriteLine("\r\nBypassACL: FAILURE");
			}

			groupsCache = new GroupsCache();
			subjectsCache = new SubjectsCache();

			Console.Out.WriteLine();

			toHere();
			to("Evaluating: ");
			toHere();

			List<PathACL> acls = new List<PathACL>();

			try
			{
				machinePrincipalContext = new PrincipalContext(ContextType.Machine);
			}
			catch
			{
				Console.Out.WriteLine("\r\nInternal error $01.");
				return;
			}

			try
			{
				domainPrincipalContext = new PrincipalContext(ContextType.Domain);
			}
			catch
			{
				domainPrincipalContext = machinePrincipalContext;
			}

			try
			{
				getTreeAcl(acls, args[0], null);
			}
			catch
			{
				Console.Out.WriteLine("\r\nInternal error $02.");
				return;
			}

			List<Subject> subjects = new List<Subject>();

			foreach (PathACL pathACL in acls)
			{
				List<Subject> pathSubjects = pathACL.AllSubjects;

				foreach (Subject subject in pathSubjects)
				{
					if (!subjects.Contains(subject))
					{
						switch (subject.PrincipalType)
						{
							case 'G':
								if(!hideGroups)
									subjects.Add(subject);
								break;
							case 'U':
								if(!hideUsers)
									subjects.Add(subject);
								break;
							default:
								if (!hideUnrecognized)
									subjects.Add(subject);
								break;
						}
					}
						
				}
			}

			subjects.Sort();

			to("done.");

			Console.Out.WriteLine();

			toHere();
			to("Exporting: ");
			toHere();
			to("0%");

			if (subjects.Count > 50000)
			{
				to("Error: too many users!");
				return;
			}

			if (acls.Count > 1000000)
			{
				to("Error: too many folders!");
				return;
			}

			ExcelPackage excelPackage = new ExcelPackage();
			ExcelWorksheet ws = excelPackage.Workbook.Worksheets.Add("ACLMatrix");

			for (int i = 0; i < acls.Count; i++)
			{
				ws.Cells[i + 2, 1].Value = acls[i].Path;
				ws.Cells[i + 2, 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
				ws.Cells[i + 2, 1].Style.Font.Bold = true;
				ws.Cells[i + 2, 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
				ws.Cells[i + 2, 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
			}

			for (int j = 0; j < subjects.Count; j++)
			{
				ws.Cells[1, j + 2].Value = subjects[j].PrincipalType + ": " + subjects[j].DisplayName + (addAccountNames ? " (" + subjects[j].AccountName + ")" : "");
				ws.Cells[1, j + 2].Style.TextRotation = 90;
				ws.Cells[1, j + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
				ws.Cells[1, j + 2].Style.Font.Bold = true;
				ws.Cells[1, j + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
				ws.Cells[1, j + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
				ws.Column(j + 2).Width = 3;
			}

			for (int i = 0; i < acls.Count; i++)
			{
				for (int j = 0; j < subjects.Count; j++)
				{
					AccessRights ar = acls[i][subjects[j]];

					if (ar != null)
					{
						string str = ar.ToString();

						ws.Cells[i + 2, j + 2].Value = str;
						ws.Cells[i + 2, j + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
						ws.Cells[i + 2, j + 2].Style.Fill.BackgroundColor.SetColor(ar.Color);
						ws.Cells[i + 2, j + 2].Style.Font.Color.SetColor(ar.Color);
						ws.Cells[i + 2, j + 2].Style.WrapText = false;
						ws.Cells[i + 2, j + 2].Style.ShrinkToFit = true;
					}

					ws.Cells[i + 2, j + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);

					to(string.Format("{0}%", (int)(((double)(i * subjects.Count + j)) / ((double)(acls.Count * subjects.Count)) * 100.0)));
				}
			}

			ws.Row(1).Height = 200;
			ws.Column(1).Width = 100;
			ws.View.FreezePanes(2, 2);

			using (FileStream fs = new FileStream(args[1], FileMode.Create, FileAccess.ReadWrite))
			{
				excelPackage.SaveAs(fs);
			}

			to("done.");
		}

		static void getTreeAcl(List<PathACL> acl, string path, PathACL parentACL)
		{
			PathACL pathACL = getPathACL(path, false);

			if ((pathACL != null) && (parentACL == null || !pathACL.Equals(parentACL)))
			{
				acl.Add(pathACL);
			}

			if (checkFiles)
			{
				try
				{
					foreach (string filePath in Directory.GetFiles(path))
					{
						PathACL fileACL = getPathACL(filePath, true);

						if (fileACL != null && !fileACL.Equals(pathACL))
						{
							acl.Add(fileACL);
						}
					}
				}
				catch { }
			}

			try
			{
				foreach (string subPath in Directory.GetDirectories(path))
				{
					getTreeAcl(acl, subPath, pathACL);
				}
			}
			catch { }
		}

		private static PathACL getPathACL(string path, bool isFile)
		{
			to(path);
			PathACL pathACL = new PathACL(path);

			try
			{
				AuthorizationRuleCollection arc = (isFile ? new FileSecurity(path, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount)) : new DirectorySecurity(path, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount)));

				foreach (FileSystemAccessRule ar in arc)
				{
					addPrincipalACL(pathACL, ar);
				}
			}
			catch { }

			return pathACL;
		}

		private static void addPrincipalACL(PathACL pathACL, FileSystemAccessRule ar)
		{
			Subject subject = subjectsCache[ar.IdentityReference.Value];

			if(subject == null)
			{
				Principal principal = null;

				try
				{
					principal = Principal.FindByIdentity(ar.IdentityReference.Value.StartsWith("BUILTIN\\") ? machinePrincipalContext : domainPrincipalContext, ar.IdentityReference.Value);
				}
				catch { }

				if (principal == null && machinePrincipalContext != domainPrincipalContext)
				{
					try
					{
						principal = Principal.FindByIdentity(machinePrincipalContext, ar.IdentityReference.Value);
					}
					catch { }
				}

				if (principal == null)
				{
					subject = new Subject(ar.IdentityReference.Value);
				}
				else
				{
					if (principal is UserPrincipal)
					{
						subject = new Subject(principal as UserPrincipal);
					}

					if (principal is GroupPrincipal)
					{
						subject = new Subject(principal as GroupPrincipal);
					}
				}
			}

			if (subject.PrincipalType == 'G')
			{
				groupsCache[subject.SubjectPrincipal as GroupPrincipal].ForEach(u =>
				{
					subjectsCache.Add(ar.IdentityReference.Value, subject);
					pathACL.AddEntry(u, ar.AccessControlType, ar.FileSystemRights);
				});
			}
			else
			{
				subjectsCache.Add(ar.IdentityReference.Value, subject);
				pathACL.AddEntry(subject, ar.AccessControlType, ar.FileSystemRights);
			}
		}

		private static int console_len = 0;
		private static int console_row = 0;
		private static int console_col = 0;

		private static void toHere()
		{
			console_len = 0;
			console_row = Console.CursorTop;
			console_col = Console.CursorLeft;
		}

		private static void to(string str)
		{
			Console.SetCursorPosition(console_col, console_row);
			Console.Out.Write(str);
			Console.Write(new String(' ', Math.Max(0, console_len - str.Length)));
			console_len = str.Length;
		}
	}
}
