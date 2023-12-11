using Microsoft.Extensions.Options;
using UserManagementService.Application.V1.ProcessUserAchievements.Repository;
using UserManagementService.Domain.Models;
using UserManagementService.Domain.Models.Events;
using UserManagementService.Domain.Util;
using UserManagementService.Infrastructure.AppSettings;
using UserManagementService.Infrastructure.PubSub;
using UserManagementService.Infrastructure.Util;

namespace UserManagementService.Application.V1.ProcessUserAchievements.Model.Strategy;

public class CheckHealthAndWellness : CheckAchievementBaseStrategy
{
    public CheckHealthAndWellness(ISqlAchievementRepository sqlAchievementRepository, IOptions<PubSub>? pubsubConfig = null) : base(sqlAchievementRepository, pubsubConfig)
    {
    }

    public override async Task ProcessAchievement(string userId, Category category, IEventBus eventBus = null)
    {
        var userAchievement = new List<UserAchievement>
        {
            UserAchievement.Cheetah1,
            UserAchievement.Cheetah2,
            UserAchievement.Cheetah3
        };
        await UpdateProgress(userId, userAchievement, category, eventBus);
    }
}