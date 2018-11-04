﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MALClient.Models.Enums;
using MALClient.Models.Models.ApiResponses;
using MALClient.Models.Models.Forums;
using MALClient.Models.Models.MalSpecific;
using Newtonsoft.Json;

namespace MALClient.Models.Models.Notifications
{
    public class MalNotification
    {
        public MalNotificationsTypes Type { get; protected set; } = MalNotificationsTypes.Generic;
        public string Id { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
        public string Header { get; set; }
        public string LaunchArgs { get; set; }
        public bool IsSupported { get; set; }
        public bool IsRead { get; set; }
        public string ImgUrl { get; set; }

        public string SanitizedLaunchArgs => LaunchArgs.Contains("~") ? LaunchArgs.Split('~').Last() : LaunchArgs;

        public MalNotification(string id)
        {
            Id = id;
        }

        public MalNotification(MalMessageModel malMessageModel)
        {
            IsRead = false;
            Type = MalNotificationsTypes.Messages;
            Header = $"{malMessageModel.Sender} sent you a message!";
            Content = malMessageModel.Content;
            Id = malMessageModel.Id;
            IsSupported = true;
            LaunchArgs = $"https://myanimelist.net/mymessages.php?go=read&id={malMessageModel.Id}|{JsonConvert.SerializeObject(malMessageModel)}";
            Date = malMessageModel.Date;
        }

        public MalNotification(WatchedTopicModel watchedTopicModel)
        {
            Type = MalNotificationsTypes.WatchedTopic;
            Header = "Watched topic replies!";
            Content = $"New activity in {watchedTopicModel.Title} topic.";
            LaunchArgs = $"https://myanimelist.net/forum/?topicid={watchedTopicModel.Id}&goto=lastpost";
            IsSupported = true;
        }

        public MalNotification(MalScrappedNotification notification)
        {
            IsRead = notification.isRead;

            if (IsRead)
                return;

            switch (notification.typeIdentifier)
            {
                case "friend_request":
                    Type = MalNotificationsTypes.FriendRequest;
                    Header = "New friend request";
                    Content = $"{notification.friendName} sent you a friend request!";
                    LaunchArgs = notification.url;
                    IsSupported = false;
                    ImgUrl = notification.friendImageUrl;
                    break;
                case "friend_request_accept":
                    Type = MalNotificationsTypes.FriendRequestAcceptDeny;
                    Header = "Friend request accepted";
                    Content = $"{notification.friendName} accepted your friend request!";
                    IsSupported = false;
                    break;
                case "friend_request_deny":
                    Type = MalNotificationsTypes.FriendRequestAcceptDeny;
                    Header = "Friend request denied";
                    Content = $"{notification.friendName} rejected your friend request.";
                    IsSupported = false;
                    break;
                case "profile_comment":
                    Type = MalNotificationsTypes.ProfileComment;
                    Header = "New profile comment";
                    Content = $"{notification.commentUserName} posted a comment on your profile\n{notification.text}.";
                    LaunchArgs = notification.url+$"|{notification.commentUserName}";
                    ImgUrl = notification.commentUserImageUrl;
                    IsSupported = true;
                    break;
                case "forum_quote":
                    Type = MalNotificationsTypes.ForumQuoute;
                    Header = "New forum quote!";
                    Content = $"{notification.quoteUserName} has quouted your post in the \"{notification.topicTitle}\" thread.";
                    LaunchArgs = notification.url;
                    IsSupported = true;
                    break;
                case "blog_comment":
                    Type = MalNotificationsTypes.BlogComment;
                    Header = "New blog comment.";
                    IsSupported = false;
                    break;
                case "watched_topic_message":
                    Type = MalNotificationsTypes.WatchedTopics;
                    Header = "New reply on your watched topic!";
                    Content = $"{notification.postedUserName} has posted on your watched topic: \"{notification.topicTitle}\"";
                    LaunchArgs = notification.topicUrl + "&goto=lastpost";
                    IsSupported = true;
                    break;
                case "club_mass_message_in_forum":
                    Type = MalNotificationsTypes.ClubMessages;
                    Header = "New club message.";
                    IsSupported = false;
                    break;
                case "user_mention_in_club_comment":
                    Type = MalNotificationsTypes.UserMentions;
                    Header = notification.categoryName;
                    Content = $"{notification.senderName} has mentioned you in club {notification.pageTitle}";
                    LaunchArgs = notification.pageUrl;
                    IsSupported = false;
                    break;
                case "on_air":
                    Type = MalNotificationsTypes.NowAiring;
                    Header = notification.categoryName;
                    Content = $"The anime you plan to watch began airing on {notification.date} {notification.animes.First().title}";
                    LaunchArgs = notification.animes.First().url;
                    IsSupported = true;
                    break;
                case "payment_stripe":
                    Type = MalNotificationsTypes.Payment;
                    Header = "Payment notification.";
                    Content = "(I don't know what does it mean, feel free to let me know about this on github)";
                    IsSupported = false;
                    break;
                case "related_anime_add":
                    Type = MalNotificationsTypes.NewRelatedAnime;
                    Header = notification.categoryName;
                    Content = $"{notification.anime.title} ({notification.anime.mediaType}) has just been added to MAL database!";
                    LaunchArgs = notification.url;
                    IsSupported = true;
                    break;
                case "user_mention_in_forum_message":
                    Type = MalNotificationsTypes.UserMentions;
                    Header = notification.categoryName;
                    Content = $"{notification.senderName} has mentioned you in forum message {notification.pageTitle}";
                    LaunchArgs = notification.url;
                    IsSupported = true;
                    break;
                default:
                    Type = MalNotificationsTypes.Generic;
                    break;
            }
            LaunchArgs = $"{notification.id}~{LaunchArgs}";
            Id = notification.id;
            Date = notification.createdAtForDisplay;
        }


    }
}
