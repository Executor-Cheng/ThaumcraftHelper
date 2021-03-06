﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ThaumcraftHelper
{
    public class Thaumcraft_Research_Calculator_1710
    {
        public static List<Aspect> Aspects { get; } = new List<Aspect>();

        public static bool LoadAspects(string aspectsPath)
        {
            List<List<string>> tempContributors = new List<List<string>>();
            try
            {
                foreach (JToken jt in JArray.Parse(File.ReadAllText(aspectsPath)))
                {
                    Aspects.Add(new Aspect(jt["AspectName_Eng"].ToString(), jt["AspectName_Chn"].ToString(), new List<Aspect>()));
                    tempContributors.Add(jt["Contributors"].Select(p => p.ToString()).ToList());
                }
            }
            catch (NullReferenceException)
            {
                ConsoleUtils.PrintWithColor("错误:要素文件的键不正确.请确保\"AspectName_Eng\"键、\"AspectName_Chn\"键存在且其值为字符串,\"Contributors\"键存在且其值为字符串数组", ConsoleColor.Red);
                return false;
            }
            catch (Newtonsoft.Json.JsonReaderException)
            {
                ConsoleUtils.PrintWithColor("错误:要素文件非标准Json格式,请检查要素文件", ConsoleColor.Red);
                return false;
            }
            for (int i = 0; i < Aspects.Count; i++)
            {
                Aspect current = Aspects[i];
                List<string> currentStrs = tempContributors[i];
                current.Contributors.AddRange(currentStrs.Select(p => Aspects.Find(q => q.Equals(p, Aspect.SearchOptionEnum.Chinese | Aspect.SearchOptionEnum.English))));
                if (currentStrs.Count == 1)
                {
                    ConsoleUtils.PrintWithColor($"警告:要素:{current.AspectName_Friendly} 的组合要素只有一个,请检查给定的组合要素名称是否有遗漏.(\"{string.Join(",", currentStrs)}\")", ConsoleColor.Yellow);
                }
                if (current.Contributors.Count == 0 && currentStrs.Count != 0)
                {
                    ConsoleUtils.PrintWithColor($"警告:要素:{current.AspectName_Friendly} 的组合要素未在要素列表中找到,请检查给定的组合要素名称\"{string.Join(",", currentStrs)}\"是否填写正确", ConsoleColor.Yellow);
                }
            }
            return true;
        }

        public static void GenerateCanLinkAspects()
        {
            foreach (Aspect aspect in Aspects)
            {
                aspect.CanLinkAspects.AddRange(Aspects.FindAll(p => p.Contributors.Contains(aspect)));
                aspect.CanLinkAspects.AddRange(aspect.Contributors);
            }
        }
        /// <summary>
        /// 计算链路
        /// </summary>
        /// <param name="startAspect">起始要素</param>
        /// <param name="endAspect">终止要素</param>
        /// <param name="count">连接要素个数</param>
        /// <param name="exceptAspects">不会参与计算的要素</param>
        /// <param name="currentAspect">方法自动填写,用户不得填写</param>
        /// <param name="result">方法自动填写,用户不得填写</param>
        /// <returns>链路列表</returns>
        public static List<List<Aspect>> Calculate(Aspect startAspect, Aspect endAspect, int count, List<Aspect> exceptAspects = null, List<Aspect> currentAspect = null, List<List<Aspect>> result = null)
        {
            if (currentAspect == null)
            {
                currentAspect = new List<Aspect>(count + 2);
                for (int i = 0; i < count + 2; i++)
                {
                    currentAspect.Add(null);
                }
                currentAspect[0] = startAspect;
                currentAspect[count + 1] = endAspect;
            }
            if (result == null)
            {
                result = new List<List<Aspect>>();
            }
            if (exceptAspects == null)
            {
                exceptAspects = new List<Aspect>();
            }
            if (count == 0)
            {
                if (startAspect.CanLinkAspects.Contains(endAspect))
                {
                    List<Aspect> taspect = new List<Aspect>(currentAspect);
                    result.Add(taspect);
                }
            }
            else
            {
                foreach (Aspect aspect in startAspect.CanLinkAspects.Except(exceptAspects))
                {
                    currentAspect[currentAspect.Capacity - count - 1] = aspect;
                    Calculate(aspect, endAspect, count - 1, exceptAspects, currentAspect, result);
                }
                return result;
            }
            return null;
        }

        public static void WriteResult(List<List<Aspect>> result)
        {
            using (StreamWriter sw = new StreamWriter("result.txt"))
            {
                foreach (List<Aspect> subResult in result)
                {
                    sw.WriteLine($"{subResult[0].AspectName_Full}→{string.Join(" ", subResult.GetRange(1, subResult.Count - 2).Select(p => $"{p.AspectName_Full}"))}←{subResult[subResult.Count - 1].AspectName_Full}");
                }
            }
        }

        public static Aspect FindAspect(string name, Aspect.SearchOptionEnum searchOption = Aspect.SearchOptionEnum.Chinese | Aspect.SearchOptionEnum.English, bool ignoreCase = false)
        {
            return Aspects.Find(p => p.Equals(name, searchOption, ignoreCase));
        }

        public static Aspect FuzzyFindAspect(string name, Aspect.SearchOptionEnum searchOption = Aspect.SearchOptionEnum.Chinese | Aspect.SearchOptionEnum.English, bool ignoreCase = false)
        {
            return Aspects.Find(p => p.FuzzyEquals(name, searchOption, ignoreCase));
        }

        public static void Main(string[] args)
        {
            Console.Title = "神秘时代4研究笔记要素链路计算器 v1.1";
            Console.WriteLine("欢迎使用 神秘时代4研究笔记要素链路计算器 v1.1");
            Console.WriteLine("当前神秘时代版本:4.2.3.5 (for mc 1.7.10)");
            Console.WriteLine("包括禁忌魔法的合成表");
            Console.WriteLine("作者:起名废丶西井(FishPool_CSharp)");
            Console.WriteLine("正在载入要素列表...");
            while (!LoadAspects("Aspects.cfg"))
            {
                Console.WriteLine("按回车键重试...");
                Aspects.Clear();
                Console.ReadLine();
            }
            GenerateCanLinkAspects();
            Aspects.TrimExcess();
            Console.WriteLine($"要素列表已载入,共计{Aspects.Count}个要素");
            Console.WriteLine("计算结束后，会在当前目录生成 txt, 内含计算结果");
            while (true)
            {
                Console.Write(">");
                string command = Console.ReadLine();
                CmdInfo ci = new CmdInfo(command);
                if (ci.Parameters.Count < 1)
                {
                    Console.WriteLine("命令错误,请重新输入");
                    continue;
                }
                Aspect firstAspect = FuzzyFindAspect(ci.Parameters[0]);
                if (firstAspect == null)
                {
                    Console.WriteLine("未找到输入的起始要素名称，请重新输入");
                    continue;
                }
                if (ci.Parameters.Count < 2)
                {
                    IDictionary<Aspect, int> counter = new Dictionary<Aspect, int>(6);
                    if (firstAspect.IsPrimary)
                    {
                        counter[firstAspect] = 1;
                    }
                    else
                    {
                        void Decompose(Aspect aspect)
                        {
                            if (aspect.IsPrimary)
                            {
                                if (!counter.ContainsKey(aspect))
                                {
                                    counter[aspect] = 0;
                                }
                                counter[aspect]++;
                            }
                            else
                            {
                                foreach (Aspect countributor in aspect.Contributors)
                                {
                                    Decompose(countributor);
                                }
                            }
                        }
                        Decompose(firstAspect);
                    }
                    Console.WriteLine($"{firstAspect.AspectName_Full}{(firstAspect.IsPrimary ? " 元始要素" : firstAspect.ContributorsStr)}");
                    Console.WriteLine($"分解成元始要素后的结果(供充能节点参考):");
                    Console.WriteLine(string.Join("; ", counter.Select(p => $"{p.Key.AspectName_Full}:{p.Value}")));
                    continue;
                }
                Aspect lastAspect = FuzzyFindAspect(ci.Parameters[1]);
                if (lastAspect == null)
                {
                    Console.WriteLine("未找到输入的终止要素名称，请重新输入");
                    continue;
                }
                List<Aspect> exceptAspects = null;
                if (ci.ArgPairs.ContainsKey("e"))
                {
                    string[] exceptAspectsStrs = ci.ArgPairs["e"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    exceptAspects = new List<Aspect>(exceptAspectsStrs.Select(p => FuzzyFindAspect(p)));
                }
                int limit = 0;
                if (ci.ArgPairs.ContainsKey("l"))
                {
                    if (!int.TryParse(ci.ArgPairs["l"], out limit) || limit < 1)
                    {
                        Console.WriteLine("输入超出限制，请重新输入");
                        continue;
                    }
                }
                int count = 0;
                if (ci.Parameters.Count > 2)
                {
                    if (!(int.TryParse(ci.Parameters[2], out count) && count > 0 && count < 9))
                    {
                        Console.WriteLine("输入超出限制，请重新输入");
                        continue;
                    }
                }
                else
                {
                    Console.Write("请输入连接要素个数(1 - 8):");
                    while (count < 1 || count > 8)
                    {
                        try
                        {
                            count = int.Parse(Console.ReadLine());
                            if (count < 1 || count > 8)
                            {
                                Console.Write("输入超出限制，请输入 1 - 8 之间的数字:");
                            }
                        }
                        catch (FormatException)
                        {
                            Console.Write("非法输入，请输入数字:");
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{firstAspect.AspectName_Full} → {lastAspect.AspectName_Full}");
                Console.ResetColor();
                Console.WriteLine(" 可链接的要素:");
                Stopwatch sw = Stopwatch.StartNew();
                List<List<Aspect>> result = Calculate(firstAspect, lastAspect, count, exceptAspects);
                sw.Stop();
                if (limit != 0)
                {
                    result = result.GetRange(0, limit > result.Count ? result.Count : limit);
                }
                ConsoleUtils.PrintResult(result);
                WriteResult(result);
                Console.WriteLine();
                Console.WriteLine($"计算结束,耗时:{sw.ElapsedMilliseconds}ms,{sw.ElapsedTicks}ticks");
                Console.WriteLine("若控制台显示不全，可在当前目录生成的 result.txt 文件查看结果");
            }
        }

        public static void Main_2(string[] args)
        {
            Console.WriteLine("欢迎使用 神秘时代4研究笔记要素链路计算器 v1.0");
            Console.WriteLine("当前神秘时代版本:4.2.3.5 (for mc 1.7.10)");
            Console.WriteLine("包括禁忌魔法的合成表");
            Console.WriteLine("作者:起名废丶西井(FishPool_CSharp)");
            Console.WriteLine("正在载入要素列表...");
            while (!LoadAspects("Aspects.cfg"))
            {
                Console.WriteLine("按回车键重试...");
                Aspects.Clear();
                Console.ReadLine();
            }
            GenerateCanLinkAspects();
            Aspects.TrimExcess();
            Console.WriteLine($"要素列表已载入,共计{Aspects.Count}个要素");
            Console.WriteLine("计算结束后，会在当前目录生成 txt, 内含计算结果");
            while (true)
            {
                Aspect firstAspect = null;
                Console.Write("请输入起始要素(中英文均可，英文大小写均可，支持模糊搜索):");
                while (firstAspect == null)
                {
                    String firstAspectStr = Console.ReadLine().ToLower();
                    firstAspect = FuzzyFindAspect(firstAspectStr);
                    if (firstAspect == null)
                    {
                        Console.Write("未找到输入的要素名称，请重新输入:");
                    }
                }
                Console.WriteLine($"找到的起始要素为:{firstAspect.AspectName_Eng} - {firstAspect.AspectName_Chn}");
                Console.Write("请输入终止要素:");
                Aspect lastAspect = null;
                while (lastAspect == null)
                {
                    String lastAspectStr = Console.ReadLine().ToLower();
                    lastAspect = FuzzyFindAspect(lastAspectStr);
                    if (lastAspect == null)
                    {
                        Console.Write("未找到输入的要素名称，请重新输入:");
                    }
                }
                Console.WriteLine($"找到的终止要素为:{lastAspect.AspectName_Eng} - {lastAspect.AspectName_Chn}");
                Console.Write("请输入连接要素个数(1 - 8):");
                int count = 0;
                while (count < 1 || count > 8)
                {
                    try
                    {
                        count = int.Parse(Console.ReadLine());
                        if (count < 1 || count > 8)
                        {
                            Console.Write("输入超出限制，请输入 1 - 8 之间的数字:");
                        }
                    }
                    catch (FormatException)
                    {
                        Console.Write("非法输入，请输入数字:");
                    }
                }
                Console.WriteLine("可链接的要素:");
                Stopwatch sw = Stopwatch.StartNew();
                List<List<Aspect>> result = Calculate(firstAspect, lastAspect, count);
                sw.Stop();
                ConsoleUtils.PrintResult(result);
                WriteResult(result);
                Console.WriteLine();
                Console.WriteLine($"计算结束,耗时:{sw.ElapsedMilliseconds}ms,{sw.ElapsedTicks}ticks");
                Console.WriteLine("若控制台显示不全，可在当前目录生成的 result.txt 文件查看结果");
            }
        }
    }

    public static class ConsoleUtils
    {
        public static void PrintResult(List<List<Aspect>> result)
        {
            foreach (List<Aspect> subResult in result)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{subResult[0].AspectName_Full} → ");
                Console.ResetColor();
                Console.Write(string.Join(" → ", subResult.GetRange(1, subResult.Count - 2).Select(p => $"{p.AspectName_Full}")));
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($" → {subResult[subResult.Count - 1].AspectName_Full}");
                Console.ResetColor();
            }
        }

        public static void PrintWithColor(string str, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ResetColor();
        }
    }

    public class CmdInfo
    {
        public string MainCommand => Parameters.Count == 0 ? null : Parameters[0];
        public Dictionary<string, string> ArgPairs { get; } = new Dictionary<string, string>();
        public List<string> Parameters { get; } = new List<string>();
        public CmdInfo() { }
        public CmdInfo(string cmd)
        {
            string[] args = Regex.Matches(cmd, @"/\s*("".+?""|[^:\s])+((\s*:\s*("".+?""|[^\s])+)|)|("".+?""|[^""\s])+").OfType<Match>().Select(p => p.ToString().Trim(new char[] { '"' })).ToArray();
            char[] kEqual = new char[] { ':', '=' };
            char[] kArgStart = new char[] { '-', '/' };
            int ii = -1;
            string token = NextToken(args, ref ii);
            while (token != null)
            {
                if (IsArg(token))
                {
                    string arg = token.TrimStart(kArgStart).TrimEnd(kEqual);
                    string value = null;
                    bool is2 = token.StartsWith("--");
                    if (arg.Contains(":") || arg.Contains("="))
                    {
                        // arg was specified with an '=' sign, so we need
                        // to split the string into the arg and value, but only
                        // if there is no space between the '=' and the arg and value.
                        string[] r = arg.Split(kEqual, 2);
                        if (r.Length == 2 && r[1] != string.Empty)
                        {
                            arg = r[0];
                            value = r[1];
                        }
                    }
                    if (!is2)
                    {
                        while (value == null)
                        {
                            string next = NextToken(args, ref ii);
                            if (next != null)
                            {
                                if (IsArg(next))
                                {
                                    // push the token back onto the stack so
                                    // it gets picked up on next pass as an Arg
                                    ii--;
                                    value = "true";
                                }
                                else if (next != ":")
                                {
                                    // save the value (trimming any '=' from the start)
                                    value = next.TrimStart(kEqual);
                                }
                            }
                            else
                            {
                                this.Parameters.Add(arg);
                                break;
                            }
                        }
                    }
                    else if (string.IsNullOrEmpty(value))
                    {
                        this.Parameters.Add(arg);
                        token = NextToken(args, ref ii);
                        continue;
                    }
                    // save the pair
                    this.ArgPairs.Add(arg, value);
                }
                else if (token != string.Empty)
                {
                    // this is a stand-alone parameter.
                    this.Parameters.Add(token);
                }
                token = NextToken(args, ref ii);
            }
        }
        /// <summary>
        /// Returns True if the passed string is an argument (starts with
        /// '-', '--', or '\'.)
        /// </summary>
        /// <param name="arg">the string token to test</param>
        /// <returns>true if the passed string is an argument, else false if a parameter</returns>
        private static bool IsArg(string arg) => arg.StartsWith("-") || arg.StartsWith("/");
        /// <summary>
        /// Returns the next string token in the argument list
        /// </summary>
        /// <param name="args">list of string tokens</param>
        /// <param name="ii">index of the current token in the array</param>
        /// <returns>the next string token, or null if no more tokens in array</returns>
        private static string NextToken(string[] args, ref int ii)
        {
            ii++; // move to next token
            while (ii > -1 && ii < args.Length)
            {
                string cur = args[ii].Trim();
                if (cur != string.Empty)
                {
                    // found valid token
                    return cur;
                }
                ii++;
            }
            // failed to get another token
            return null;
        }
    }
}