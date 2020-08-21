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
using System.Drawing;
using System.Security.AccessControl;

namespace aclmatrix
{
	class AccessRights : IEquatable<AccessRights>
	{
		public FileSystemRights Allow;
		public FileSystemRights Deny;

		public bool Equals(AccessRights other)
		{
			return (Allow & FileSystemRights.FullControl) == (other.Allow & FileSystemRights.FullControl) && (Deny & FileSystemRights.FullControl) == (other.Deny & FileSystemRights.FullControl);
		}

		public override string ToString()
		{
			FileSystemRights fsr = Allow & (~Deny);

			List<string> buf = new List<string>();

			if ((fsr & FileSystemRights.ReadData) == FileSystemRights.ReadData)
				buf.Add(".RD.");
			if ((fsr & FileSystemRights.ListDirectory) == FileSystemRights.ListDirectory)
				buf.Add(".LD.");
			if ((fsr & FileSystemRights.WriteData) == FileSystemRights.WriteData)
				buf.Add(".WD.");
			if ((fsr & FileSystemRights.CreateFiles) == FileSystemRights.CreateFiles)
				buf.Add(".CF.");
			if ((fsr & FileSystemRights.AppendData) == FileSystemRights.AppendData)
				buf.Add(".AD.");
			if ((fsr & FileSystemRights.CreateDirectories) == FileSystemRights.CreateDirectories)
				buf.Add(".CD.");
			if ((fsr & FileSystemRights.ReadExtendedAttributes) == FileSystemRights.ReadExtendedAttributes)
				buf.Add(".REA.");
			if ((fsr & FileSystemRights.WriteExtendedAttributes) == FileSystemRights.WriteExtendedAttributes)
				buf.Add(".WEA.");
			if ((fsr & FileSystemRights.ExecuteFile) == FileSystemRights.ExecuteFile)
				buf.Add(".EF.");
			if ((fsr & FileSystemRights.Traverse) == FileSystemRights.Traverse)
				buf.Add(".T.");
			if ((fsr & FileSystemRights.DeleteSubdirectoriesAndFiles) == FileSystemRights.DeleteSubdirectoriesAndFiles)
				buf.Add(".DSAF.");
			if ((fsr & FileSystemRights.ReadAttributes) == FileSystemRights.ReadAttributes)
				buf.Add(".RA.");
			if ((fsr & FileSystemRights.WriteAttributes) == FileSystemRights.WriteAttributes)
				buf.Add(".WA.");
			if ((fsr & FileSystemRights.Delete) == FileSystemRights.Delete)
				buf.Add(".D.");
			if ((fsr & FileSystemRights.ReadPermissions) == FileSystemRights.ReadPermissions)
				buf.Add(".RP.");
			if ((fsr & FileSystemRights.ChangePermissions) == FileSystemRights.ChangePermissions)
				buf.Add(".CP.");
			if ((fsr & FileSystemRights.TakeOwnership) == FileSystemRights.TakeOwnership)
				buf.Add(".TO.");
			if ((fsr & FileSystemRights.Synchronize) == FileSystemRights.Synchronize)
				buf.Add(".S.");
			if ((fsr & FileSystemRights.FullControl) == FileSystemRights.FullControl)
				buf.Add(".FC.");
			if ((fsr & FileSystemRights.Read) == FileSystemRights.Read)
				buf.Add(".R.");
			if ((fsr & FileSystemRights.ReadAndExecute) == FileSystemRights.ReadAndExecute)
				buf.Add(".RAE.");
			if ((fsr & FileSystemRights.Write) == FileSystemRights.Write)
				buf.Add(".W.");
			if ((fsr & FileSystemRights.Modify) == FileSystemRights.Modify)
				buf.Add(".M.");

			return String.Join(", ", buf);
		}

		public Color Color
		{
			get
			{
				FileSystemRights fsr = Allow & (~Deny);

				if ((fsr & FileSystemRights.TakeOwnership) == FileSystemRights.TakeOwnership)
					return Color.LightSalmon;
				if ((fsr & FileSystemRights.FullControl) == FileSystemRights.FullControl)
					return Color.LightSalmon;
				if ((fsr & FileSystemRights.ChangePermissions) == FileSystemRights.ChangePermissions)
					return Color.LightSalmon;

				if ((fsr & FileSystemRights.WriteData) == FileSystemRights.WriteData)
					return Color.LightGoldenrodYellow;
				if ((fsr & FileSystemRights.CreateFiles) == FileSystemRights.CreateFiles)
					return Color.LightGoldenrodYellow;
				if ((fsr & FileSystemRights.AppendData) == FileSystemRights.AppendData)
					return Color.LightGoldenrodYellow;
				if ((fsr & FileSystemRights.DeleteSubdirectoriesAndFiles) == FileSystemRights.DeleteSubdirectoriesAndFiles)
					return Color.LightGoldenrodYellow;
				if ((fsr & FileSystemRights.Delete) == FileSystemRights.Delete)
					return Color.LightGoldenrodYellow;
				if ((fsr & FileSystemRights.Write) == FileSystemRights.Write)
					return Color.LightGoldenrodYellow;
				if ((fsr & FileSystemRights.Modify) == FileSystemRights.Modify)
					return Color.LightGoldenrodYellow;
				if ((fsr & FileSystemRights.CreateDirectories) == FileSystemRights.CreateDirectories)
					return Color.LightGoldenrodYellow;

				if ((fsr & FileSystemRights.ReadData) == FileSystemRights.ReadData)
					return Color.LightGreen;
				if ((fsr & FileSystemRights.Read) == FileSystemRights.Read)
					return Color.LightGreen;
				if ((fsr & FileSystemRights.ReadAndExecute) == FileSystemRights.ReadAndExecute)
					return Color.LightGreen;
				if ((fsr & FileSystemRights.ExecuteFile) == FileSystemRights.ExecuteFile)
					return Color.LightGreen;

				if ((fsr & FileSystemRights.ListDirectory) == FileSystemRights.ListDirectory)
					return Color.LightSteelBlue;
				if ((fsr & FileSystemRights.ReadExtendedAttributes) == FileSystemRights.ReadExtendedAttributes)
					return Color.LightSteelBlue;
				if ((fsr & FileSystemRights.WriteExtendedAttributes) == FileSystemRights.WriteExtendedAttributes)
					return Color.LightSteelBlue;
				if ((fsr & FileSystemRights.Traverse) == FileSystemRights.Traverse)
					return Color.LightSteelBlue;
				if ((fsr & FileSystemRights.ReadAttributes) == FileSystemRights.ReadAttributes)
					return Color.LightSteelBlue;
				if ((fsr & FileSystemRights.WriteAttributes) == FileSystemRights.WriteAttributes)
					return Color.LightSteelBlue;
				if ((fsr & FileSystemRights.ReadPermissions) == FileSystemRights.ReadPermissions)
					return Color.LightSteelBlue;
				if ((fsr & FileSystemRights.Synchronize) == FileSystemRights.Synchronize)
					return Color.LightSteelBlue;

				return Color.White;
			}
		}
	}
}
