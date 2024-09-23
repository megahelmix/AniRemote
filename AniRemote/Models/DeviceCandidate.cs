using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AniRemote.Models
{
	public class DeviceCandidate
	{
		public Guid Id { get; internal set; }
		public string Name { get; internal set; } = string.Empty;
	}
}
