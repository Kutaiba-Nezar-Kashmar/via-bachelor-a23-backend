using FirebaseAdmin;
using Google.Api.Gax;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace EventManagementService.Infrastructure;

public class Firestore
{
    private const string ServiceAccountKeyEnvironmentKey = "SERVICE_ACCOUNT_KEY_JSON_FIREBASE";
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static FirestoreDb Get()
    {
        var credentials =
            GoogleCredential.FromJson(Environment.GetEnvironmentVariable(ServiceAccountKeyEnvironmentKey));

        return new FirestoreDbBuilder
        {
            ProjectId = "bpr-app-a44a8",
            Credential = credentials
        }.Build();
    }
    
    /// <summary>
    /// Initializes Firebase App. Use this if you need to interact with Auth
    /// <code>
    /// var defaultInstance = FirebaseAuth.DefaultInstance;
    /// if (defaultInstance == null)
    /// {
    ///     Firestore.CreateFirebaseApp();
    /// }
    /// UserRecord firebaseUser = await FirebaseAuth.DefaultInstance.GetUserAsync(id);
    /// </code>
    /// </summary>
    public static FirebaseApp CreateFirebaseApp()
    {
        var app = FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromJson(Environment.GetEnvironmentVariable(ServiceAccountKeyEnvironmentKey))
        });
        return app;
    }
}