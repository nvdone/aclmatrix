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

using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Threading;

namespace aclmatrix
{
	class Subject : IEqualityComparer<Subject>, IComparable<Subject>, IEquatable<Subject>
	{
		public Principal SubjectPrincipal { get; }
		
		public char PrincipalType { get; }

		public string AccountName { get; }

		public string DisplayName { get; }

		public Subject()
		{
			PrincipalType = '?';
		}

		public Subject(Principal subjectPrincipal)
		{
			SubjectPrincipal = subjectPrincipal;

			PrincipalType = '?';

			int attempts = 20;
			bool done = false;

			while (attempts-- > 0)
			{
				try
				{
					AccountName = subjectPrincipal?.DistinguishedName ?? "";
					if (AccountName.Length == 0)
						AccountName = subjectPrincipal?.SamAccountName ?? "";
					if (AccountName.Length == 0)
						AccountName = subjectPrincipal?.Guid.ToString() ?? "";

					DisplayName = subjectPrincipal.DisplayName ?? "";
					if (DisplayName.Length == 0 && this.AccountName.Length > 0)
						DisplayName = AccountName;

					PrincipalType = subjectPrincipal is GroupPrincipal ? 'G' : 'U';

					done = true;
					break;
				}
				catch
				{
					Thread.Sleep(500);
				}
			}

			if (!done)
			{
				AccountName = Guid.NewGuid().ToString();
				DisplayName = "Unknown principal ";
			}
		}

		public Subject(string name)
		{
			PrincipalType = '?';
			AccountName = name ?? Guid.NewGuid().ToString();
			DisplayName = name ?? "Unknown identity ";
		}

		#region IComparable<Subject> - List.Sort
		public int CompareTo(Subject other)
		{
			int result = PrincipalType.CompareTo(other.PrincipalType);
			if(result == 0)
				return DisplayName.CompareTo(other.DisplayName);
			return result;
		}
		#endregion

		#region IEqualityComparer<Subject> - Dictionary.ContainsKey
		public bool Equals(Subject x, Subject y)
		{
			return x.AccountName.Equals(y.AccountName);
		}

		public int GetHashCode(Subject obj)
		{
			return obj.AccountName.GetHashCode();
		}
		#endregion

		#region IEquatable<Subject> - List.Contains
		public bool Equals(Subject other)
		{
			return AccountName.Equals(other.AccountName);
		}
		#endregion
	}
}
