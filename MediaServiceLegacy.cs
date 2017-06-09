using InstaSharp;
using InstaSharp.Endpoints;
using InstaSharp.Models.Responses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using System.Net.Http;
using InstaSharp.Extensions;
using MediaModule.Web.Services;
using AutoMapper;
using MediaModule.Web.Domain.Media;
using System.Reflection;
using MediaModule.Web.Cache;
using MediaModule.Web.Core;
using MediaModule.Web.Domain.Account;
using MediaModule.Web.Services.Interfaces;
using System.Threading;

namespace MediaModule.Services
{
    public class MediaServiceLegacy
    {
        /*******************Why Legacy?************************************************************************************
   *  Orignally this API was accessed through Instasharp functions and in order to make the service more
   *  flexable, all their classes were mapped to local classes. But this became legacy when more server side
   *  authication was immplemented in the MediaController. The crucial resaon was Instasharp required a userId 
   *  to get their reccent media. The Above method, using Restsharp, does the same funcationality without needing the userId
   *  as the orginal Instagram API intended.
   *  
   *  Although these methods are not being used, there's still some useful immplementation code for getting comments..etc
   *  **************************************************************************************************************/

        private enum MapperTypes : int
        {
            InstagramConfig
           , Media
           , CommentsResponse
           , MediaResponse
           , Attribution
               , UserInfo
              , Caption
               , Comments
               , Image
               , Resolution
               , Likes
               , Location
               , User
               , UserInPhoto
               , Video
               , Comment
              , ListComments
               , IUserInfo


        }

        private static Dictionary<MapperTypes, IMapper> _mappers = new Dictionary<MapperTypes, IMapper>();


        static MediaService()
        {
            IMapper mapMedia = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Media, IMediaCharacteristics>()


            // We ignore these because they are class properties, they will be mapped indviiually and their corresponding class properties will be ignored and further mapped indivudally
            .ForMember
        ("Attribution", dest => dest.Ignore())
        .ForMember
        ("Caption", dest => dest.Ignore())
        .ForMember
        ("Comments", dest => dest.Ignore())
        .ForMember
        ("Images", dest => dest.Ignore())
        .ForMember
        ("Likes", dest => dest.Ignore())
         .ForMember
        ("Location", dest => dest.Ignore())
        .ForMember
        ("User", dest => dest.Ignore())
        .ForMember
        ("UsersInPhoto", dest => dest.Ignore())
         .ForMember
        ("Videos", dest => dest.Ignore())
        ).CreateMapper();
            IMapper mappAtrr = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Attribution, Attribution>()).CreateMapper();
            IMapper mappCaption = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Caption, Caption>().ForMember
                  ("From", dest => dest.Ignore())).CreateMapper();
            IMapper mapUserInfo = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.UserInfo, UserInfo>()).CreateMapper();
            IMapper mapUserInPhoto = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.UserInPhoto, UserInPhoto>()).CreateMapper();
            IMapper mapListComment = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Comment, Comment>()).CreateMapper();
            IMapper mapComments = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Comments, Domain.Media.Comments>().ForMember
               ("Data", dest => dest.Ignore())).CreateMapper();

            IMapper mapImage = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Image, Domain.Media.Image>().ForMember
               ("LowResolution", dest => dest.Ignore()).ForMember
               ("StandardResolution", dest => dest.Ignore()).ForMember
               ("Thumbnail", dest => dest.Ignore())
               ).CreateMapper();
            IMapper mapResolution = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Resolution, Resolution>()).CreateMapper();
            IMapper mapLikes = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Likes, Domain.Media.Likes>().ForMember
               ("Data", dest => dest.Ignore())).CreateMapper();
            IMapper mapLocation = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Location, Location>()).CreateMapper();
            IMapper mapUser = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.User, Domain.Media.User>().ForMember
               ("Counts", dest => dest.Ignore())).CreateMapper();

            IMapper mapVideo = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Video, Domain.Media.Video>().ForMember
                ("LowResolution", dest => dest.Ignore()).ForMember
                ("StandardResolution", dest => dest.Ignore())).CreateMapper();

            _mappers.Add(MapperTypes.Media, mapMedia);
            _mappers.Add(MapperTypes.Attribution, mappAtrr);
            _mappers.Add(MapperTypes.UserInfo, mapUserInfo);
            _mappers.Add(MapperTypes.UserInPhoto, mapUserInPhoto);
            _mappers.Add(MapperTypes.Caption, mappCaption);

            _mappers.Add(MapperTypes.Comments, mapComments);
            _mappers.Add(MapperTypes.Image, mapImage);
            _mappers.Add(MapperTypes.Resolution, mapResolution);
            _mappers.Add(MapperTypes.Likes, mapLikes);
            _mappers.Add(MapperTypes.Location, mapLocation);
            _mappers.Add(MapperTypes.User, mapUser);
            _mappers.Add(MapperTypes.Video, mapVideo);


            #region Mapping Comment Response
            IMapper mapCommentsResponse = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Responses.CommentsResponse, ICommentsResponse>()
            .ForMember
            ("Data", dest => dest.Ignore())).CreateMapper();
            IMapper mapComment = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Comment, Comment>().ForMember
            ("From", dest => dest.Ignore())).CreateMapper();

            IMapper mapListComments = new MapperConfiguration(cfg => cfg.CreateMap<InstaSharp.Models.Comment, IComment>().ForMember
            ("From", dest => dest.Ignore())).CreateMapper();

            _mappers.Add(MapperTypes.Comment, mapComment);
            _mappers.Add(MapperTypes.CommentsResponse, mapCommentsResponse);
            _mappers.Add(MapperTypes.ListComments, mapListComments);

            #endregion
        }

        public async Task<List<IMediaCharacteristics>> MyFeed()
        {
            List<IMediaCharacteristics> list = null;
            InstaSharp.Models.Responses.OAuthResponse resp = GetOAuthResp();

            Users usersEndPoint = new Users(_configService.GetMediaConfig, resp);

            var feed = await usersEndPoint.RecentSelf();

            var key = "instagram_" + feed.Data[0].Id;
            DateTimeOffset expiration = DateTimeOffset.Now.AddMinutes(20);

            if (!_cacheService.Contains(key))
            {
                if (feed != null && feed.Data != null)
                {
                    list = new List<IMediaCharacteristics>();
                    foreach (var item in feed.Data)
                    {
                        IMediaCharacteristics m = _mappers[MapperTypes.Media].Map<IMediaCharacteristics>(item);
                        m = IterateMedia(m, item);
                        list.Add(m);
                    }
                }

                if (key != null)
                {
                    _cacheService.Add(key, list, expiration);

                }

                return list;
            }


            list = _cacheService.Get<List<IMediaCharacteristics>>(key);
            return (list);

        }
        public async Task<List<IComment>> GetComments(string mediaId)
        {
            InstaSharp.Models.Responses.CommentsResponse c = null;
            List<IComment> comments = null;

            InstaSharp.Models.Responses.OAuthResponse resp = GetOAuthResp();

            var commentEndPoint = new InstaSharp.Endpoints.Comments(_configService.GetMediaConfig, resp);
            c = await commentEndPoint.Get(mediaId);
            if (c != null && c.Data != null) // The Power of Null Check, it could be null if user does not enter valid id so this check allows this case to be successful response with items: null
            {
                int i = 0;
                foreach (var item in c.Data)
                {
                    comments = _mappers[MapperTypes.ListComments].Map<List<IComment>>(c.Data);
                    if (comments != null)
                    {
                        comments[i] = _mappers[MapperTypes.Comment].Map<Comment>(item);
                        comments[i].From = _mappers[MapperTypes.UserInfo].Map<UserInfo>(item.From);
                    }
                    i++;
                }
            }
            return comments;
        }

        public async Task<IMediaCharacteristics> GetMedia(string mediaId)
        {
            IMediaCharacteristics media = null;

            InstaSharp.Models.Responses.OAuthResponse resp = GetOAuthResp();
            InstaSharp.Models.Responses.MediaResponse m = null;
            var mEndPoint = new InstaSharp.Endpoints.Media(_configService.GetMediaConfig, resp);
            m = await mEndPoint.Get(mediaId);
            if (m != null && m.Data != null)
            {
                media = _mappers[MapperTypes.Media].Map<IMediaCharacteristics>(m.Data);
                #region Map Insta Media to Imedia
                if (m.Data.Attribution != null)
                {
                    media.Attribution = _mappers[MapperTypes.Attribution].Map<Attribution>(m.Data.Attribution);
                }

                media.Caption = _mappers[MapperTypes.Caption].Map<Caption>(m.Data.Caption);
                if (m.Data.Caption != null)
                {
                    media.Caption.From = _mappers[MapperTypes.UserInfo].Map<UserInfo>(m.Data.Caption.From);
                }
                media.Comments = _mappers[MapperTypes.Comments].Map<Domain.Media.Comments>(m.Data.Comments);

                media.Images = _mappers[MapperTypes.Image].Map<Domain.Media.Image>(m.Data.Images);
                media.Images.Thumbnail = _mappers[MapperTypes.Resolution].Map<Domain.Media.Resolution>(m.Data.Images.Thumbnail);
                media.Images.LowResolution = _mappers[MapperTypes.Resolution].Map<Domain.Media.Resolution>(m.Data.Images.LowResolution);
                media.Images.StandardResolution = _mappers[MapperTypes.Resolution].Map<Resolution>(m.Data.Images.StandardResolution);
                media.Likes = _mappers[MapperTypes.Likes].Map<Domain.Media.Likes>(m.Data.Likes);
                if (m.Data.Location != null)
                {
                    media.Location = _mappers[MapperTypes.Location].Map<Domain.Media.Location>(m.Data.Location);

                }

                media.User = _mappers[MapperTypes.UserInfo].Map<UserInfo>(m.Data.User);

                if (m.Data.GetType() == typeof(List<UserInPhoto>)) /// TODO Check if this works/necesssary
                {
                    foreach (var u in m.Data.UsersInPhoto)
                    {
                        media.UsersInPhoto.Add(_mappers[MapperTypes.UserInPhoto].Map<UserInPhoto>(u));
                    }

                }
                if (m.Data.Type == "video")
                {
                    media.Videos = _mappers[MapperTypes.Video].Map<Video>(m.Data.Videos);
                    media.Videos.LowResolution = _mappers[MapperTypes.Resolution].Map<Resolution>(m.Data.Videos.LowResolution);
                    media.Videos.StandardResolution = _mappers[MapperTypes.Resolution].Map<Resolution>(m.Data.Videos.StandardResolution);
                }
                #endregion

            }
            return media;


        }

        // Utils //
        private IMediaCharacteristics IterateMedia(IMediaCharacteristics m, InstaSharp.Models.Media item)
        {


            m.Attribution = _mappers[MapperTypes.Attribution].Map<Attribution>(item.Attribution);


            m.Caption = _mappers[MapperTypes.Caption].Map<Caption>(item.Caption);
            if (m.Caption != null)
            {
                m.Caption.From = _mappers[MapperTypes.UserInfo].Map<UserInfo>(item.Caption.From);
            }
            m.Comments = _mappers[MapperTypes.Comments].Map<Domain.Media.Comments>(item.Comments);

            m.Images = _mappers[MapperTypes.Image].Map<Domain.Media.Image>(item.Images);
            m.Images.Thumbnail = _mappers[MapperTypes.Resolution].Map<Domain.Media.Resolution>(item.Images.Thumbnail);
            m.Images.LowResolution = _mappers[MapperTypes.Resolution].Map<Domain.Media.Resolution>(item.Images.LowResolution);
            m.Images.StandardResolution = _mappers[MapperTypes.Resolution].Map<Resolution>(item.Images.StandardResolution);
            m.Likes = _mappers[MapperTypes.Likes].Map<Domain.Media.Likes>(item.Likes);
            if (item.Location != null)
            {
                m.Location = _mappers[MapperTypes.Location].Map<Domain.Media.Location>(item.Location);

            }

            m.User = _mappers[MapperTypes.UserInfo].Map<UserInfo>(item.User);

            if (item.GetType() == typeof(List<UserInPhoto>)) /// TODO Check if this works/necesssary
            {
                foreach (var u in m.UsersInPhoto)
                {
                    m.UsersInPhoto.Add(_mappers[MapperTypes.UserInPhoto].Map<UserInPhoto>(u));
                }

            }
            if (item.Type == "video")
            {
                m.Videos = _mappers[MapperTypes.Video].Map<Video>(item.Videos);
                m.Videos.LowResolution = _mappers[MapperTypes.Resolution].Map<Resolution>(item.Videos.LowResolution);
                m.Videos.StandardResolution = _mappers[MapperTypes.Resolution].Map<Resolution>(item.Videos.StandardResolution);
            }
            return m;
        }
        private InstaSharp.Models.Responses.OAuthResponse GetOAuthResp()
        {

            InstaSharp.Models.Responses.OAuthResponse resp = new InstaSharp.Models.Responses.OAuthResponse();
            InstaSharp.Endpoints.Users user = new Users(_configService.GetMediaConfig);


            InstaSharp.Models.Responses.UserResponse a = await user.GetSelf();

            Task.WaitAll(user.GetSelf());
            resp.User = GetUser();

            resp.AccessToken = _configService.MediaToken();
            return resp;
        }

        private static InstaSharp.Models.UserInfo GetUser()
        {
            var user = new InstaSharp.Models.UserInfo
            {
                FullName = "Will Wendell",
                Id = 203199314,
                ProfilePicture = "https://scontent.cdninstagram.com/t51.2885-19/s150x150/12338847_470881829703548_513626600_a.jpg",
                Username = "phimbe"
            };

            return user;
        }
    }
}
