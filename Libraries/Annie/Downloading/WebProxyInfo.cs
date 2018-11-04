using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Annie {
	public class WebProxyInfo : WebProxy {
		public string Name { get; }
		public string Location { get; }

		public WebProxyInfo(string name, string location, string address) : base(address) {
			Name = name;
			Location = location;
			UseDefaultCredentials = true;
		}
	}
}
