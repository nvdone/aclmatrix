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
	class GroupsCache
	{
		private readonly Dictionary<string, List<Subject>> cache = new Dictionary<string, List<Subject>>();

		public List<Subject> this[GroupPrincipal groupPrincipal]
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

				List<Subject> subjects = new List<Subject>
				{
					new Subject(groupPrincipal)
				};

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
					return subjects;
				}

				foreach (Principal principal in members)
				{
					try
					{
						if (principal is UserPrincipal)
						{
							Subject subject = new Subject(principal);
							if (!subjects.Contains(subject))
								subjects.Add(subject);
						}

						if (principal is GroupPrincipal)
						{
							List<Subject> subMembers = this[principal as GroupPrincipal];
							foreach (Subject subMember in subMembers)
							{
								if (!subjects.Contains(subMember))
									subjects.Add(subMember);
							}
						}
					}
					catch { }
				}

				if (!cache.ContainsKey(key))
					cache.Add(key, subjects);

				return subjects;
			}
		}
	}
}
