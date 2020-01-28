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
	class GroupsCache
	{
		private Dictionary<string, List<User>> cache = new Dictionary<string, List<User>>();

		public List<User> this[GroupPrincipal groupPrincipal]
		{
			get
			{
				string key = groupPrincipal.DistinguishedName?.ToLower() ?? "";
				if (key.Length == 0)
					key = groupPrincipal.SamAccountName?.ToLower() ?? "";
				if (key.Length == 0)
					key = "Unknown group " + Guid.NewGuid().ToString().ToLower();

				if (cache.ContainsKey(key))
					return cache[key];

				List<User> users = new List<User>();

				PrincipalSearchResult<Principal> members = null;

				int attempts = 20;

				while (attempts-- > 0)
				{
					try
					{
						members = groupPrincipal.GetMembers();
						break;
					}
					catch (Exception)
					{
						Thread.Sleep(500);
					}
				}

				if (members == null)
				{
					return users;
				}

				try
				{
					foreach (Principal principal in members)
					{
						try
						{
							if (principal is UserPrincipal)
							{
								User user = new User(principal as UserPrincipal);
								if (!users.Contains(user))
									users.Add(user);
							}
						}
						catch { }

						if (principal is GroupPrincipal)
						{
							try
							{
								List<User> subUsers = this[principal as GroupPrincipal];
								foreach (User subUser in subUsers)
								{
									if (!users.Contains(subUser))
										users.Add(subUser);
								}
							}
							catch { }
						}
					}
				}
				catch { }

				if (!cache.ContainsKey(key))
					cache.Add(key, users);

				return users;
			}
		}
	}
}
