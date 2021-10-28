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

using System.Collections.Generic;

namespace aclmatrix
{
	class SubjectsCache
	{
		private readonly Dictionary<string, Subject> cache = new Dictionary<string, Subject>();

		public Subject this[string identity]
		{
			get
			{
				if (cache.ContainsKey(identity.ToLower()))
					return cache[identity.ToLower()];

				return null;
			}
		}

		public void Add(string identity, Subject subject)
		{
			if (!cache.ContainsKey(identity.ToLower()))
				cache.Add(identity.ToLower(), subject);
		}
	}
}
