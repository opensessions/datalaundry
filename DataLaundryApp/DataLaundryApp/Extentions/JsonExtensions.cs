using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLaundryApp.Extentions
{
    public static class JsonExtensions
    {
        public static List<JToken> FindTokens(this JToken containerToken, string name)
        {
            var matches = new List<JToken>();
            FindTokens(containerToken, name, matches);
            return matches;
        }

        private static void FindTokens(JToken containerToken, string name, List<JToken> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Name == name)
                    {
                        matches.Add(child.Value);
                    }
                    FindTokens(child.Value, name, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokens(child, name, matches);
                }
            }
        }


        public static List<Tuple<JToken, string, string, int>> FindTokens_V2ByLevel(this JToken containerToken, string name)
        {
            var matches = new List<Tuple<JToken, string, string, int>>();
            FindTokens_V2(containerToken, name, matches);

            matches = matches.OrderBy(x => x.Item4).ToList();
            
            return matches;
        }

        private static void FindTokens_V2(JToken containerToken, string name, List<Tuple<JToken, string, string, int>> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Name == name)
                    {
                        string customJsonPath = DataLaundryDAL.CommonFunctions.GetCustomJsonPath(child.Path);

                        int level = customJsonPath.Split('>').Length;

                        var tuple = new Tuple<JToken, string, string, int>(child.Value, customJsonPath, child.Path, level);
                        matches.Add(tuple);
                    }
                    FindTokens_V2(child.Value, name, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokens_V2(child, name, matches);
                }
            }
        }


        public static List<string> FindTokenKeys(this JToken containerToken)
        {
            var matches = new List<string>();
            FindTokenKeys(containerToken, matches);
            return matches;
        }

        private static void FindTokenKeys(JToken containerToken, List<string> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    string customJsonPath = DataLaundryDAL.CommonFunctions.GetCustomJsonPath(child.Path);
                    matches.Add(customJsonPath);
                    FindTokenKeys(child.Value, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokenKeys(child, matches);
                    break;
                }
            }
        }

        public static List<Tuple<JToken, string, string>> FindTokenKeys_V2(this JToken containerToken)
        {
            var matches = new List<Tuple<JToken, string, string>>();
            FindTokenKeys_V2(containerToken, matches);
            return matches;
        }

        public static List<Tuple<JToken, string, string>> FindTokenKeys_V2ByLevel(this JToken containerToken)
        {
            var matches = new List<Tuple<JToken, string, string>>();
            FindTokenKeys_V2(containerToken, matches);

            //sort matches by level
            matches = matches.OrderBy(x => x.Item2.Split('>').Length).ToList();

            return matches;
        }
        
        private static void FindTokenKeys_V2(JToken containerToken, List<Tuple<JToken, string, string>> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    string customJsonPath = DataLaundryDAL.CommonFunctions.GetCustomJsonPath(child.Path);

                    var tuple = new Tuple<JToken, string, string>(child.Name, customJsonPath, child.Path);
                    matches.Add(tuple);

                    FindTokenKeys_V2(child.Value, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokenKeys_V2(child, matches);
                    break;
                }
            }
        }

        public static List<dynamic> FindTokenKeys_LeafNodes(this JToken containerToken)
        {
            var matches = new List<dynamic>();
            FindTokenKeys_LeafNodes(containerToken, matches);
            return matches;
        }

        private static void FindTokenKeys_LeafNodes(JToken containerToken, List<dynamic> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    if (child.Value.Type != JTokenType.Object
                        && child.Value.Type != JTokenType.Array)
                    {
                        string customJsonPath = DataLaundryDAL.CommonFunctions.GetCustomJsonPath(child.Path);

                        //var tuple = new Tuple<JToken, string, string>(child.Name, customJsonPath, child.Path);
                        dynamic match = new System.Dynamic.ExpandoObject();
                        match.Name = child.Name;
                        match.CustomJsonPath = customJsonPath;
                        match.ActualJsonPath = child.Path;

                        matches.Add(match);
                    }

                    FindTokenKeys_LeafNodes(child.Value, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                foreach (JToken child in containerToken.Children())
                {
                    FindTokenKeys_LeafNodes(child, matches);
                    break;
                }
            }
        }

        public static List<Tuple<JToken, string, string>> FindArrays(this JToken containerToken)
        {
            var matches = new List<Tuple<JToken, string, string>>();
            FindTokenArrays(containerToken, matches);
            
            return matches;
        }

        public static List<Tuple<JToken, string, string>> FindArraysByLevel(this JToken containerToken)
        {
            var matches = new List<Tuple<JToken, string, string>>();
            FindTokenArrays(containerToken, matches);

            //sort matches by level
            matches = matches.OrderBy(x => x.Item2.Split('>').Length).ToList();

            return matches;
        }

        private static void FindTokenArrays(JToken containerToken, List<Tuple<JToken, string, string>> matches)
        {
            if (containerToken.Type == JTokenType.Object)
            {
                foreach (JProperty child in containerToken.Children<JProperty>())
                {
                    FindTokenArrays(child.Value, matches);
                }
            }
            else if (containerToken.Type == JTokenType.Array)
            {
                string customJsonPath = DataLaundryDAL.CommonFunctions.GetCustomJsonPath(containerToken.Path);

                var tuple = new Tuple<JToken, string, string>(containerToken, customJsonPath, containerToken.Path);
                matches.Add(tuple);
                
                foreach (JToken child in containerToken.Children())
                {
                    FindTokenArrays(child, matches);
                    break;
                }
            }
        }
    }
}