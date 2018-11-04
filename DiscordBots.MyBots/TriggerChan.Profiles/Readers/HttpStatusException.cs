using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TriggersTools.DiscordBots.TriggerChan.Profiles {
	/// <summary>
	/// An exception thrown when a 404 status code is returned.
	/// </summary>
	public class HttpStatusException : HttpRequestException {
		/// <summary>
		/// Gets the exception Http status code
		/// </summary>
		public HttpStatusCode StatusCode { get; }

		public HttpStatusException(HttpStatusCode statusCode)
			: base($"An Http status code of {statusCode} was returned!")
		{
			StatusCode = statusCode;
		}
		public HttpStatusException(HttpStatusCode statusCode, string message)
			: base(message)
		{
			StatusCode = statusCode;
		}
		public HttpStatusException(HttpStatusCode statusCode, string message, Exception innerException)
			: base(message, innerException)
		{
			StatusCode = statusCode;
		}
	}
}
