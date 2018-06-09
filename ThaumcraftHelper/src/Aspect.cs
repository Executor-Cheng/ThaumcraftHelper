using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ThaumcraftHelper
{
    [DebuggerDisplay("{AspectName_Chn} - {AspectName_Eng}")]
    public class Aspect
    {
        /// <summary>
        /// 搜索语言枚举
        /// </summary>
        [FlagsAttribute]
        public enum SearchOptionEnum
        {
            /// <summary>
            /// 使用英语
            /// </summary>
            English = 0x1,
            /// <summary>
            /// 使用中文
            /// </summary>
            Chinese = 0x2
        }
        /// <summary>
        /// 元素英文名
        /// </summary>
        public string AspectName_Eng { get; }
        /// <summary>
        /// 元素中文名
        /// </summary>
        public string AspectName_Chn { get; }
        /// <summary>
        /// 元素友好名
        /// </summary>
        public string AspectName_Friendly => $"{AspectName_Eng} - {AspectName_Chn}";
        /// <summary>
        /// 元素完整名
        /// </summary>
        public string AspectName_Full => $"{AspectName_Eng}{AspectName_Chn}";
        /// <summary>
        /// 可以连接的元素
        /// </summary>
        public List<Aspect> CanLinkAspects { get; } = new List<Aspect>();
        /// <summary>
        /// 构成该元素的两个元素
        /// </summary>
        public List<Aspect> Contributors { get; }
        public Aspect(string aspectName_Eng, string aspectName_Chn, List<Aspect> contributors)
        {
            AspectName_Eng = aspectName_Eng.ToLower();
            AspectName_Chn = aspectName_Chn;
            Contributors = contributors;
        }
        /// <summary>
        /// 判断给定的元素名称是否与此元素名称相等
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="aspectName">元素名称</param>
        /// <param name="searchType">搜索语言</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns>是否相等</returns>
        public bool Equals(string aspectName, SearchOptionEnum searchType, bool ignoreCase = false)
        {
            bool canUseChn = false, canUseEng = false;
            if ((searchType & SearchOptionEnum.English) != 0) canUseEng = true;
            if ((searchType & SearchOptionEnum.Chinese) != 0) canUseChn = true;
            if (!canUseChn && !canUseEng) throw new ArgumentOutOfRangeException("searchType", searchType, "必须至少包含一个值");
            if (ignoreCase)
            {
                aspectName = aspectName.ToLower();
            }
            return (canUseEng && AspectName_Eng == aspectName) || (canUseChn && AspectName_Chn == aspectName);
        }
        /// <summary>
        /// 模糊确认是否为此元素
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <param name="fuzzyName">模糊昵称</param>
        /// <param name="searchType">搜索语言</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        /// <returns></returns>
        public bool FuzzyEquals(string fuzzyName, SearchOptionEnum searchType, bool ignoreCase = false)
        {
            bool canUseChn = false, canUseEng = false;
            if ((searchType & SearchOptionEnum.English) != 0) canUseEng = true;
            if ((searchType & SearchOptionEnum.Chinese) != 0) canUseChn = true;
            if (!canUseChn && !canUseEng) throw new ArgumentOutOfRangeException("searchType", searchType, "必须至少包含一个值");
            if (ignoreCase)
            {
                fuzzyName = fuzzyName.ToLower();
            }
            return (canUseEng && AspectName_Eng.StartsWith(fuzzyName)) || (canUseChn && AspectName_Chn.StartsWith(fuzzyName));
        }
    }
}