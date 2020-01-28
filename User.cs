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

using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Threading;

namespace aclmatrix
{
	class User : IEqualityComparer<User>, IComparable<User>, IEquatable<User>
	{
		private string accountName;

		public string AccountName
		{
			get
			{
				return accountName;
			}
		}

		private string displayName;

		public string DisplayName
		{
			get
			{
				return displayName;
			}
		}

		public User()
		{
		}

		public User(UserPrincipal userPrincipal)
		{
			int attempts = 20;
			bool done = false;

			while (attempts-- > 0)
			{
				try
				{
					accountName = userPrincipal?.DistinguishedName ?? "";
					if (accountName.Length == 0)
						accountName = userPrincipal?.SamAccountName ?? "";
					if (accountName.Length == 0)
						accountName = userPrincipal?.Guid.ToString() ?? "";

					displayName = userPrincipal.DisplayName ?? "";
					if (displayName.Length == 0 && this.accountName.Length > 0)
						displayName = accountName;

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
				accountName = Guid.NewGuid().ToString();
				displayName = "Unknown principal ";
			}
		}

		public User(string name)
		{
			accountName = name ?? Guid.NewGuid().ToString();
			displayName = name ?? "Unknown identity ";
		}

		public int CompareTo(User other)
		{
			return displayName.CompareTo(other.displayName);
		}

		public bool Equals(User x, User y)
		{
			return x.accountName.Equals(y.accountName);
		}

		public int GetHashCode(User obj)
		{
			return obj.accountName.GetHashCode();
		}

		public bool Equals(User other)
		{
			return accountName.Equals(other.accountName);
		}
	}
}
