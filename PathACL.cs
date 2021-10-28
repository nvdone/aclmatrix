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
using System.Linq;
using System.Security.AccessControl;

namespace aclmatrix
{
	class PathACL : IEquatable<PathACL>
	{
		private readonly Dictionary<Subject, AccessRights> subjectRights = null;

		public string Path { get; }

		public PathACL(string path)
		{
			subjectRights = new Dictionary<Subject, AccessRights>(new Subject());
			Path = path;
		}

		public void AddEntry(Subject subject, AccessControlType act, FileSystemRights fsr)
		{
			if (!subjectRights.ContainsKey(subject))
			{
				AccessRights ar;

				if (act == AccessControlType.Allow)
					ar = new AccessRights { Allow = fsr };
				else
					ar = new AccessRights { Deny = fsr };

				subjectRights.Add(subject, ar);
			}
			else
			{
				AccessRights ar = subjectRights[subject];

				if (act == AccessControlType.Allow)
					ar.Allow |= fsr;
				else
					ar.Deny |= fsr;
			}
		}

		public List<Subject> AllSubjects
		{
			get
			{
				return subjectRights.Keys.ToList();
			}
		}

		public AccessRights this[Subject subject]
		{
			get
			{
				if (!subjectRights.ContainsKey(subject))
					return null;

				return
					subjectRights[subject];
			}
		}

		public bool Equals(PathACL other)
		{
			Dictionary<Subject, AccessRights> mySubstantialRights = subjectRights.Where(i => i.Value.Substantial).ToDictionary(j => j.Key, j => j.Value, new Subject());
			Dictionary<Subject, AccessRights> otherSubstantialRights = other.subjectRights.Where(k => k.Value.Substantial).ToDictionary(l => l.Key, l => l.Value, new Subject());

			if (mySubstantialRights.Count() != otherSubstantialRights.Count())
				return false;

			foreach (Subject subject in mySubstantialRights.Keys)
			{
				if (!otherSubstantialRights.ContainsKey(subject))
					return false;
				if (!mySubstantialRights[subject].Equals(other.subjectRights[subject]))
					return false;
			}

			return true;
		}
	}
}
