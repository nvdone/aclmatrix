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
using System.Linq;
using System.Security.AccessControl;

namespace aclmatrix
{
	class PathACL : IEquatable<PathACL>
	{
		private Dictionary<User, AccessRights> userRights = null;

		public string Path { get; }

		public PathACL(string path)
		{
			userRights = new Dictionary<User, AccessRights>(new User());
			Path = path;
		}

		public void AddEntry(User user, AccessControlType act, FileSystemRights fsr)
		{
			if (!userRights.ContainsKey(user))
			{
				AccessRights ac;

				if (act == AccessControlType.Allow)
					ac = new AccessRights { Allow = fsr };
				else
					ac = new AccessRights { Deny = fsr };

				userRights.Add(user, ac);
			}
			else
			{
				AccessRights ac = userRights[user];

				if (act == AccessControlType.Allow)
					ac.Allow |= fsr;
				else
					ac.Deny |= fsr;
			}
		}

		public List<User> AllUsers
		{
			get
			{
				return userRights.Keys.ToList<User>();
			}
		}

		public AccessRights this[User user]
		{
			get
			{
				if (!userRights.ContainsKey(user))
					return null;

				return
					userRights[user];
			}
		}

		public bool Equals(PathACL other)
		{
			if (userRights.Count != other.userRights.Count)
				return false;

			foreach (User user in userRights.Keys)
			{
				if (!other.userRights.ContainsKey(user))
					return false;

				if (!userRights[user].Equals(other.userRights[user]))
					return false;
			}

			return true;
		}
	}
}
