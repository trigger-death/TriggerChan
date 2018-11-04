﻿using Newtonsoft.Json;
using System;

namespace JikanDotNet
{
	/// <summary>
	/// Model class for MyAnimeList forum topic.
	/// </summary>
	public class ForumTopic
	{
		/// <summary>
		/// Topic's MyAnimeList Id.
		/// </summary>
		[JsonProperty(PropertyName = "topic_id")]
		public long? TopicId { get; set; }

		/// <summary>
		/// Topic's URL.
		/// </summary>
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		/// <summary>
		/// Topic's title.
		/// </summary>
		[JsonProperty(PropertyName = "Title")]
		public string Title { get; set; }

		/// <summary>
		/// Date of topic start.
		/// </summary>
		[JsonProperty(PropertyName = "date_posted")]
		public DateTime? DatePosted { get; set; }

		/// <summary>
		/// Topic's author username.
		/// </summary>
		[JsonProperty(PropertyName = "author_name")]
		public string AuthorName { get; set; }

		/// <summary>
		/// URL to profile of topic author.
		/// </summary>
		[JsonProperty(PropertyName = "author_url")]
		public string AuthorURL { get; set; }

		/// <summary>
		/// Number of replies.
		/// </summary>
		[JsonProperty(PropertyName = "replies")]
		public int? Replies { get; set; }

		/// <summary>
		/// Basic information about last post in the topic.
		/// </summary>
		[JsonProperty(PropertyName = "last_post")]
		public ForumPostSnippet LastPost { get; set; }
	}
}