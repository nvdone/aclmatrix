//NVD ACLMatrix
//Copyright © 2016-2019, Nikolay Dudkin

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
		private static UsersCache usersCache;

		private static PrincipalContext domainPrincipalContext;
		private static PrincipalContext machinePrincipalContext;

		public static void Main(string[] args)
		{
			Console.Out.WriteLine(string.Format("{0} {1}\r\n{2}", ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(AssemblyTitleAttribute))).Title, Assembly.GetExecutingAssembly().GetName().Version, ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(System.Reflection.Assembly.GetExecutingAssembly(), typeof(AssemblyCopyrightAttribute))).Copyright));

			if (args.Length < 2)
			{
				Console.Out.WriteLine("\r\nUsage: ACLMatrix.exe root_path output_file.xlsx [ShowAccountNames]");
				return;
			}

			bool addAccountNames = (args.Length == 3) && args[2].Equals("showaccountnames", StringComparison.InvariantCultureIgnoreCase);

			groupsCache = new GroupsCache();
			usersCache = new UsersCache();

			Console.Out.WriteLine();

			ToHere();
			To("Evaluating: ");
			ToHere();

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

			List<User> users = new List<User>();

			foreach (PathACL pathACL in acls)
			{
				List<User> pathUsers = pathACL.AllUsers;

				foreach (User user in pathUsers)
				{
					if (!users.Contains(user))
						users.Add(user);
				}
			}

			users.Sort();

			To("done.");

			Console.Out.WriteLine();

			ToHere();
			To("Exporting: ");
			ToHere();
			To("0%");

			if (users.Count > 50000)
			{
				To("Error: too many users!");
				return;
			}

			if (acls.Count > 1000000)
			{
				To("Error: too many folders!");
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

			for (int j = 0; j < users.Count; j++)
			{
				ws.Cells[1, j + 2].Value = users[j].DisplayName + (addAccountNames ? " (" + users[j].AccountName + ")" : "");
				ws.Cells[1, j + 2].Style.TextRotation = 90;
				ws.Cells[1, j + 2].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
				ws.Cells[1, j + 2].Style.Font.Bold = true;
				ws.Cells[1, j + 2].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
				ws.Cells[1, j + 2].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue);
				ws.Column(j + 2).Width = 3;
			}

			for (int i = 0; i < acls.Count; i++)
			{
				for (int j = 0; j < users.Count; j++)
				{
					AccessRights ar = acls[i][users[j]];

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

					To(string.Format("{0}%", (int)(((double)(i * users.Count + j)) / ((double)(acls.Count * users.Count)) * 100.0)));
				}
			}

			ws.Row(1).Height = 200;
			ws.Column(1).Width = 100;
			ws.View.FreezePanes(2, 2);

			using (FileStream fs = new FileStream(args[1], FileMode.Create, FileAccess.ReadWrite))
			{
				excelPackage.SaveAs(fs);
			}

			To("done.");
		}

		static void getTreeAcl(List<PathACL> acl, string path, PathACL parentACL)
		{
			PathACL pathACL = getPathACL(path);

			if ((pathACL != null) && (parentACL == null || !pathACL.Equals(parentACL)))
			{
				acl.Add(pathACL);
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

		private static PathACL getPathACL(string path)
		{
			To(path);
			PathACL pathACL = new PathACL(path);

			try
			{
				AuthorizationRuleCollection arc = new DirectorySecurity(path, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount));

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
			User user = usersCache[ar.IdentityReference.Value];

			if (user != null)
			{
				pathACL.AddEntry(user, ar.AccessControlType, ar.FileSystemRights);
			}
			else
			{
				Principal principal = Principal.FindByIdentity(ar.IdentityReference.Value.StartsWith("BUILTIN\\") ? machinePrincipalContext : domainPrincipalContext, ar.IdentityReference.Value);

				if (principal == null)
				{
					pathACL.AddEntry(new User(ar.IdentityReference.Value), ar.AccessControlType, ar.FileSystemRights);
				}
				else
				{
					if (principal is UserPrincipal)
					{
						user = new User(principal as UserPrincipal);
						usersCache.Add(ar.IdentityReference.Value, user);
						pathACL.AddEntry(user, ar.AccessControlType, ar.FileSystemRights);
					}

					if (principal is GroupPrincipal)
						groupsCache[principal as GroupPrincipal].ForEach(u =>
						{
							pathACL.AddEntry(u, ar.AccessControlType, ar.FileSystemRights);
						});
				}
			}
		}

		private static int console_len = 0;
		private static int console_row = 0;
		private static int console_col = 0;

		private static void ToHere()
		{
			console_len = 0;
			console_row = Console.CursorTop;
			console_col = Console.CursorLeft;
		}

		private static void To(string str)
		{
			Console.SetCursorPosition(console_col, console_row);
			Console.Out.Write(str);
			Console.Write(new String(' ', Math.Max(0, console_len - str.Length)));
			console_len = str.Length;
		}
	}
}
