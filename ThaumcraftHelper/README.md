#只是一个简单的链路计算器，偷懒用的  
##要素配置文件名为Aspects.cfg，使用Json格式，需要遵从以下规则  
整体使用Array存储每一个Object,一个Object下应当有3个键:  
- AspectName_Eng 要素英文名称,大写字符会被转为小写
- AspectName_Chn 要素中文名称
- Contributors 组成该要素的两个要素,元始要素请留空 []

##若有一项不符合规则，程序加载时会输出警告/错误