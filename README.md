# Alloy MVC Template

## Setup the Development Environment

1. Build the solution in Visual Studio

# Udate the key for the insights, tracking and profile 
    <add key="episerver:profiles.InsightApiBaseUrl" value="changethis"/>
    <add key="episerver:profiles.InsightApiSubscriptionKey" value="changethis"/>
    <add key="episerver:profiles.CloudUIBaseUrl" value="changethis"/>
    <add key="episerver:profiles.TrackingApiBaseUrl" value="changethis"/>
    <add key="episerver:tracking.Enabled" value="true"/>
    <add key="episerver:profiles.ProfileStoreTrackingEnabled" value="true"/>
    <add key="episerver:profiles.TrackingApiSubscriptionKey" value="changethis"/>
    <add key="episerver:profiles.ProfileStoreTrackingEnabled" value="true"/>
    <add key="episerver:profiles.Scope" value="defaultscope"/>
    <add key="episerver:profiles.ProfileApiBaseUrl" value="changethis"/>
    <add key="episerver:profiles.ProfileApiSubscriptionKey" value="changethis"/>
    <add key="episerver:RecommendationServiceKey" value="changethis"/>
    <add key="episerver:RecommendationServiceSecret" value="changethis"/>
    <add key="episerver:RecommendationServiceUri" value="changethis"/>

##Pageviews
1. Activate the schdeuled Job "Index Recent Hits" .
2. SELECT TOP (1000) [pkId]
      ,[Row]
      ,[StoreName]
      ,[ItemType]
      ,[PageId]
      ,[ViewsCount]
      ,[LastViewdDateTime]
      ,[LanguageCode]
  FROM [alloy.cms].[dbo].[tblInsightPageViewsDataStoreBigTable]  this table will store all the  records.



