using Microsoft.Extensions.DependencyInjection;

namespace EventManagementService.Application.FetchCategories;

public static class FetchCategoriesExtension
{
    public static IServiceCollection AddFetchCategories(this IServiceCollection services)
    {
        return services;
    }
}