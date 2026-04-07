using ComputerUtils.Logging;
using OculusDB.Database;
using OculusGraphQLApiLib;
using OculusGraphQLApiLib.Results;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace OculusDB
{
    public class OculusInteractor
    {
        public static bool logOculusRequests = false;
        public static void Init()
        {
            GraphQLClient.forcedLocale = "en_US";
            GraphQLClient.throwException = false;
            GraphQLClient.log = logOculusRequests;
        }

        public static IEnumerable<Application> EnumerateAllApplications(Headset headset)
        {
            Data<AppStoreAllAppsSection> s = GraphQLClient.AllApps(headset, null, 100);
            var data = s.data;
            if(data == null || data.node == null)
            {
                yield break;
            }
            if(data.node == null || data.node.all_items.edges == null)
            {
                throw new Exception("Could not get data to enumerate applications.");
            }

            string cursor = null;
            while (s.data.node.all_items.edges.Count > 0)
            {
                foreach (Node<Application> e in s.data.node.all_items.edges)
                {
                    if (e.node != null)
                    {
                        cursor = e.cursor;
                        yield return e.node;
                    }
                }

                if (s.data.node.all_items.page_info.has_next_page) break;
                if (data.node.all_items.page_info.end_cursor == null) break;
                s = GraphQLClient.AllApps(headset, cursor, 100);
                data = s.data;
                if (data == null || data.node == null) break;
            }
        }
        
        public static IEnumerable<IAPItem> EnumerateAllDLCs(string groupingId)
        {
            Data<ApplicationGrouping?> s = GraphQLClient.GetDLCsDeveloper(groupingId);
            var data = s.data;
            if(data == null || data.node == null)
            {
                yield break;
            }
            if(data.node.add_ons == null || data.node.add_ons.edges == null)
            {
                yield break;
            }
            while (true)
            {
                foreach (Node<IAPItem> e in s.data.node.add_ons.edges)
                {
                    if (e.node != null)
                        yield return e.node;
                }

                if (!data.node.add_ons.page_info.has_next_page) break;
                if (data.node.add_ons.page_info.end_cursor == null) break;
                s = GraphQLClient.GetDLCsDeveloper(groupingId, s.data.node.add_ons.page_info.end_cursor);
                data = s.data;
                if (data == null || data.node == null) break;
            }
        }
        
        public static IEnumerable<AchievementDefinition> EnumerateAllAchievements(string appId)
        {
            Data<Application?> s = GraphQLClient.GetAchievements(appId);
            Logger.Log(JsonSerializer.Serialize(s));
            if(s.data.node == null || s.data.node.grouping == null)
            {
                throw new Exception("Could not get data to enumerate achievements.");
            }
            foreach (AchievementDefinition e in s.data.node.grouping.achievement_definitions.nodes)
            {
                yield return e;
            }
        }

        public static IEnumerable<OculusBinary> EnumerateAllVersions(string appId)
        {
            Data<EdgesPrimaryBinaryApplication> s = GraphQLClient.AllVersionOfAppCursor(appId);
            var data = s.data;
            if(data == null || data.node == null)
            {
                yield break;
            }
            if(data.node.primary_binaries == null || data.node.primary_binaries.edges == null)
            {
                yield break;
            }
            while (true)
            {
                foreach (Node<OculusBinary>? e in data.node.primary_binaries.edges)
                {
                    if (e.node != null)
                        yield return e.node;
                }

                if (!data.node.primary_binaries.page_info.has_next_page) break;
                if (data.node.primary_binaries.page_info.end_cursor == null) break;
                s = GraphQLClient.AllVersionOfAppCursor(appId, data.node.primary_binaries.page_info.end_cursor);
                data = s.data;
                if (data == null || data.node == null) break;
            }
        }
    }
}
