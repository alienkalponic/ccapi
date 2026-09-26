using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Project.Application.Common.Repository;
using Project.Infastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        private readonly IConfiguration _configuration;

        #region::Authentication

        public IRoleRepository roleRepository { get; private set; }

        public IContactRepository contactRepository { get; private set; }

        public IUsersRepository usersRepository { get; private set; }



        #endregion

        #region::Banner
        public IBannerRepository bannerRepository { get; private set; }
        #endregion

        #region:: ClubDescription
        public IClubDescriptionRepository clubDescriptionRepository { get; private set; }
        public IClubDescriptionImageRepository clubDescriptionImageRepository { get; private set; }
        #endregion

        #region::Activity Details
        public IActivitieDetailsRepository activitieDetailsRepository { get; private set; }
        #endregion

        #region::Activity Details Image
        public IActivitieDetailsImageRepository activitieDetailsImageRepository { get; private set; }
        #endregion

        #region::Achievement Details
        public IAchievementDetailsRepository achievementDetailsRepository { get; private set; }
        public IAchievementDetailsGalleryRepository achievementDetailsGalleryRepository { get; private set; }
        #endregion

        #region::Activity Registration
        public IActivityRegistrationRepository activityRegistrationRepository { get; private set; }

        #endregion

        #region::About Page
        public IAboutPageRepository aboutPageRepository { get; private set; }
        public IAboutPersonRepository aboutPersonRepository { get; private set; }
        public IAboutPageSectionRepository aboutPageSectionRepository { get; private set; }
        #endregion

        #region::Category
        public ICategoryRepository categoryRepository { get; private set; }
        #endregion

        #region::CourseManagement
        public ICourseManagementRepository courseManagementRepository { get; private set; }
        #endregion
        public UnitOfWork(IConfiguration configuration, ApplicationDbContext db)
        {
            _db = db;
            _configuration = configuration;
            roleRepository = new RoleService(configuration, _db);
            contactRepository = new ContactService(configuration,_db);
            usersRepository=new UsersService(configuration,_db);
            bannerRepository=new BannerService(configuration, _db);
            clubDescriptionRepository=new ClubDescriptionService(configuration, _db);
            clubDescriptionImageRepository=new ClubDescriptionImageService(configuration, _db);
            activitieDetailsRepository=new ActivitieDetailsService(configuration, _db);
            activitieDetailsImageRepository=new ActivitieDetailsImageService(configuration, _db);
            achievementDetailsRepository=new AchievementDetailsService(configuration, _db);
            achievementDetailsGalleryRepository=new AchievementDetailsGalleryService(configuration, _db);
            activityRegistrationRepository=new ActivityRegistrationService(configuration, _db);
            aboutPageRepository=new AboutPageService(configuration, _db);
            aboutPersonRepository=new AboutPersonService(configuration, _db);
            aboutPageSectionRepository=new AboutPageSectionService(configuration, _db);
            categoryRepository=new CategoryService(configuration, _db);
            courseManagementRepository = new CourseManagementService(configuration, _db);
        }

        
    }
}
