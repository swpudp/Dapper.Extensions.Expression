using Dapper.Extensions.Expression.WebTest.Model;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Expression.WebTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OwnerController : ControllerBase
    {
        private static Counter counter = new Counter { Houses = 0, Populations = 0, Rooms = 0 };

        [HttpPost("init")]
        public async Task<Counter> Init([FromQuery] string communityId)
        {
            if (counter.CommunityIds.Contains(communityId))
            {
                throw new NotSupportedException("已执行");
            }
            using IDbConnection connection = CreateConnection();
            Community community = connection.Query<Community>().Where(f => f.Id == communityId).FirstOrDefault<Community>();
            District town = connection.Query<District>().Where(f => f.Id == community.DistrictId).FirstOrDefault<District>();
            IList<Room> rooms = connection.Query<Room>().Where(f => f.CommunityId == community.Id).ToList<Room>();
            IList<Owner> owners = Convert(rooms, town).ToList();
            if (rooms.Sum(f => f.Populations) != owners.Count)
            {
                throw new NotSupportedException("业主人数异常");
            }
            int total = await connection.InsertBulkAsync(owners);
            if (total != owners.Count)
            {
                throw new NotSupportedException("写入数据数量异常");
            }
            counter.Rooms += rooms.Count;
            counter.Populations += owners.Count;
            counter.CommunityIds.Add(communityId);
            return counter;
        }

        [HttpGet("{id}")]
        public async Task<Owner> Get(string id)
        {
            using IDbConnection connection = CreateConnection();
            return await connection.Query<Owner>().Where(f => f.Id == id).FirstOrDefaultAsync<Owner>();
        }

        private static IEnumerable<Owner> Convert(IList<Room> roomList, District town)
        {
            foreach (var room in roomList)
            {
                //姓
                string name = names[Random.Shared.Next(0, names.Count)];
                //子女age
                int child1Age = Random.Shared.Next(0, 18);
                int child2Age = child1Age + Random.Shared.Next(1, 10);
                int child3Age = child1Age + Random.Shared.Next(1, 10);
                //adult
                int adultMaleAge = child1Age + Random.Shared.Next(22, 30);
                int adultFemaleAge = adultMaleAge + Random.Shared.Next(1, 5);
                //Older
                int olderMaleAge = adultMaleAge + Random.Shared.Next(22, 30);
                int olderFemaleAge = olderMaleAge + Random.Shared.Next(1, 5);
                //随机老人性别
                OwnerSex olderSex = ownerSexes[Random.Shared.Next(0, ownerSexes.Count)];
                //家庭结构类型
                int type = Random.Shared.Next(0, 3);
                //人数
                switch (room.Populations)
                {
                    case 1:
                        //成年：随机
                        yield return CreateRandomOwner(room, town);
                        break;
                    case 2:
                        //成年：随机
                        yield return CreateRandomOwner(room, town);
                        yield return CreateRandomOwner(room, town);
                        break;
                    case 3:
                        //父母：男 女，子女：随机，随父姓
                        yield return CreateOwner(room, name, child1Age, town);
                        yield return CreateOwner(room, name, OwnerSex.Male, adultMaleAge, town);
                        yield return CreateOwner(room, OwnerSex.Female, adultFemaleAge, town);
                        break;
                    case 4:
                        yield return CreateOwner(room, name, OwnerSex.Male, adultMaleAge, town);
                        yield return CreateOwner(room, OwnerSex.Female, adultFemaleAge, town);
                        if (type == 0)
                        {
                            //成年：男、女 老年：男、女
                            yield return CreateOwner(room, name, OwnerSex.Male, olderMaleAge, town);
                            yield return CreateOwner(room, OwnerSex.Female, olderFemaleAge, town);
                        }
                        if (type == 1)
                        {
                            //成年：男、女  儿童：2 随机、随父姓
                            yield return CreateOwner(room, name, child1Age, town);
                            yield return CreateOwner(room, name, child2Age, town);
                        }
                        if (type == 2)
                        {
                            //成年：男、女  老年：男（男性一个姓氏）/随机女性  儿童：随机
                            //老年男或者老年女
                            if (olderSex == OwnerSex.Female)
                            {
                                yield return CreateOwner(room, olderSex, olderFemaleAge, town);
                            }
                            else
                            {
                                yield return CreateOwner(room, name, OwnerSex.Male, olderMaleAge, town);
                            }
                            //儿童：男、女
                            yield return CreateOwner(room, name, child1Age, town);
                        }
                        break;
                    case 5:
                        yield return CreateOwner(room, name, OwnerSex.Male, adultMaleAge, town);
                        yield return CreateOwner(room, OwnerSex.Female, adultFemaleAge, town);
                        if (type == 0)
                        {
                            //成年：男、女  老年：男、女、儿童：随机
                            //老年：男、女
                            yield return CreateOwner(room, name, OwnerSex.Male, olderMaleAge, town);
                            yield return CreateOwner(room, OwnerSex.Female, olderFemaleAge, town);
                            yield return CreateOwner(room, name, child1Age, town);
                        }
                        if (type == 1)
                        {
                            //成年：男、女  儿童：3 随机、随父姓
                            yield return CreateOwner(room, name, child1Age, town);
                            yield return CreateOwner(room, name, child2Age, town);
                            yield return CreateOwner(room, name, child3Age, town);
                        }
                        if (type == 2)
                        {
                            //成年：男、女  老年：男（男性一个姓氏）/随机女性  儿童：2随机
                            //老年男或者老年女
                            if (olderSex == OwnerSex.Female)
                            {
                                yield return CreateOwner(room, olderSex, olderFemaleAge, town);
                            }
                            else
                            {
                                yield return CreateOwner(room, name, OwnerSex.Male, olderMaleAge, town);
                            }
                            yield return CreateOwner(room, name, child1Age, town);
                            yield return CreateOwner(room, name, child2Age, town);
                        }
                        break;
                    default:
                        throw new NotSupportedException("不支持");
                }
            }
        }

        private static readonly List<string> names = ["李", "王", "张", "刘", "陈", "杨", "黄", "周", "罗", "何", "唐", "吴", "赵", "胡", "邓", "曾", "徐", "蒋", "谢", "彭", "朱", "肖", "郑", "马", "廖", "余", "袁", "冯", "郭", "钟", "杜", "林", "宋", "熊", "高", "孙", "梁", "魏", "任", "阿", "谭", "向", "曹", "雷", "邹", "吉", "文", "叶", "田", "蒲", "许", "汪", "范", "邱", "付", "蔡", "夏", "程", "代", "龙", "龚", "苏", "潘", "尹", "姚", "伍", "严", "卢", "吕", "江", "毛", "万", "易", "董", "贾", "沈", "韩", "秦", "丁", "侯", "贺", "赖", "白", "石", "段", "苟", "姜", "兰", "黎", "陶", "方", "舒", "康", "岳", "牟", "甘", "冉", "汤", "欧", "游"];

        private static readonly Dictionary<OwnerSex, List<string>> maleWords = new Dictionary<OwnerSex, List<string>>
        {
            [OwnerSex.Male] = ["钦月", "沂沐", "重博", "奕钧", "隽沂", "若豪", "易泽", "大亭", "芯安", "宜燚", "铂辰", "亚宵", "翰恩", "天义", "宸珩", "洋阳", "梓良", "重然", "宣越", "顾宪", "静宜", "嘉钒", "益龙", "俊西", "鸣泽", "露岚", "志睿", "星泽", "婧玥", "淇峰", "玉彬", "君强", "裕明", "翌峰", "瑞元", "锦乔", "子芃", "浩齐", "健馨", "兆米", "瀚潇", "成朗", "赞群", "星雅", "陈森", "以睿", "晨景", "峻磊", "景帆", "德颢", "桓正", "奕彦", "泽江", "君姝", "睿睿", "云琛", "弘清", "高远", "逸瑄", "博桦", "奕霖", "程鹏", "雨德", "慧楠", "津瑞", "竣宁", "德林", "东秦", "翊雪", "晟炎", "亦姝", "翊霆", "心莹", "骅烨", "博怡", "浩佳", "亦周", "楠胜", "子卿", "润平", "浩宏", "懿凯", "弘锦", "正礼", "安炫", "祥煜", "志兴", "荣茂", "景航", "宸西", "伯灏", "克瞳", "钦贤", "恒元", "煜辰", "冉笙", "珍浩", "若溪", "天昊", "北建", "恒越", "家宸", "杰锌", "俊婷", "宇糯", "欣荣", "瑞宇", "桂衷", "永茂", "锦萧", "青子", "益翰", "熙云", "意波", "嘉琰", "楚文", "文宇", "坦弦", "胤麒", "宸烁", "馨梦", "英博", "夏扬", "月雯", "奕航", "雯媛", "辰渲", "建清", "祺瑞", "兴业", "芷芊", "祖豪", "培安", "春柯", "吉丞", "冠东", "幕杨", "琛震", "承星", "晓帆", "延锐", "奕宗", "平立", "雪荣", "智磊", "英昊", "乔烁", "方志", "子彤", "承羲", "欣茜", "宇煌", "军斯", "天伊", "一洺", "斯瑶", "玉赫", "新景", "嘉轩", "宸溪", "玺麟", "禹茗", "立生", "名佩", "艺浚", "乐成", "锦城", "重筝", "子茜", "展天", "牧瑶", "昊立", "军淡", "璟宸", "恩鑫", "梓锴", "梓灿", "耀洪", "舒琪", "宇喆", "泽忻", "昱文", "松芬", "舒婷", "俊汐", "亦沫", "靖权", "诤炅", "璟清", "锦儒", "皓淼", "俊锋", "桦兴", "则钦", "政宥", "嘉楷", "秋鸣", "施锋", "熙延", "运曜", "安韬", "梓茂", "灿维", "浩佳", "德华", "其嘉", "圣宸", "涛信", "瀚锡", "广鑫", "希恒", "桤泽", "瀚年", "洛汐", "帝琦", "程朗", "盛恒", "励元", "亚奇", "霁华", "永莉", "闻喜", "雨星", "振乐", "岱森", "玉鑫", "瀚博", "重喻", "煜柏", "泽顺", "逸晨", "东北", "昌瑾", "晓亮", "效韬", "安淼", "译衡", "勇清", "庆伟", "宸铠", "昊楠", "晋扬", "啸霖", "智瑾", "禾圣", "小伟", "玉业", "宗译", "兴珍", "元兴", "涵铭", "思溢", "茂容", "艺潇", "云鹄", "少庭", "凯琦", "展德", "沐凡", "帜星", "皓翊", "立翊", "侠端", "凯琳", "韩威", "颐雪", "傲毅", "曼颖", "圣楠", "治平", "瀚钧", "赫霖", "灏彤", "宜航", "宇阔", "允贤", "程锡", "棋松", "琰华", "梓余", "子剑", "禹墨", "捷敬", "曼茹", "政翰", "博名", "怀安", "展宁", "澄宇", "卓群", "安龙", "元之", "微荒", "翊榕", "海峰", "觅涵", "灏洲", "羽晨", "玉华", "宁山", "钦芸", "耀贤", "桦新", "煕哲", "锡伦", "天心", "涵旭", "路旅", "弘彦", "墨琛", "美妮", "楚挚", "亦铭", "锦舒", "和睦", "浩澄", "铭佑", "文博", "昱达", "明军", "诗慧", "怡仁", "朝照", "泽瑄", "金佑", "松荣", "彦龙", "信昊", "美月", "宇祖", "润钰", "乔之", "亚臣", "依扬", "淏清", "晟雅", "炫攸", "熙耿", "麒文", "俊晖", "沁怡", "程琪", "彬洪", "烨鑫", "海澜", "星光", "子忔", "辉姣", "妍欢", "孝明", "忻悦", "裕议", "康桂", "梦柔", "启康", "愉瑜", "奕横", "浩琪", "慕子", "陈子", "浩畅", "奕源", "远舟", "睿伊", "瑞槿", "锦铭", "华安", "浩运", "诗宇", "启伦", "歆瑶", "墨枫", "银楠", "伟裕", "璟昂", "昶达", "昀泽", "正振", "骁珑", "一潇", "芷艺", "艾礼", "珹勤", "翊铭", "弈琛", "皓名", "仁一", "世年", "喆熹", "凯延", "毅言", "雨尘", "世帆", "玺睿", "尚果", "铠榕", "学森", "圆情", "皓朗", "研博", "梓兴", "允新", "祥徐", "思亮", "政亦", "晋嘉", "子水", "杭杜", "子斌", "陈安", "柏盛", "君保", "名轩", "佳骏", "亦雄", "奕星", "如辰", "熙意", "俊龙", "茗铄", "梓依", "锦垚", "成章", "剑宇", "宏祎", "兮澄", "凯乔", "隆方", "懿宜", "铠硕", "亦心", "洛嘉", "辉勰", "艺淇", "连晨", "炎楠", "易阔", "建辰", "景昊", "怡辰", "婉盈", "锦坤", "略洽", "晨熙", "汪思", "泽琛", "中怡", "谷睿", "瑞家", "潇睿", "斯桐", "润麒", "鹏盛", "弘隽", "礼莯", "世成", "祎凡", "泰智", "炎辉", "成栩", "浩志", "清贤", "翰明", "珍灏", "宥希", "令煊", "峻辰", "晋兴", "霖扬", "嘉丰", "屹辰", "咏玮", "友宣", "青彤", "森航", "钦涛", "卓洲", "蓝升", "栩源", "皓中", "霖诺", "宸皓", "业彬", "绍弘", "加安", "意涵", "宇承", "庚泽", "羿辰", "景澈", "芊然", "水博", "若洲", "懿瑶", "哲勋", "璐彤", "乐文", "扬倪", "沐君", "博函", "鸿", "焕", "风", "朗", "浩", "亮", "政", "谦", "航", "弘", "雄", "琛", "钧", "冠", "策", "腾", "楠", "榕", "风", "贵", "福", "元", "国", "胜", "学", "祥", "才", "新", "亨", "奇", "滕", "炅", "炜", "伟", "刚", "勇", "毅", "俊", "峰", "强", "军", "平", "保", "东", "文", "辉", "力", "固", "之", "段", "殿", "泰", "利", "清", "飞", "彬", "富", "顺", "信", "子", "杰", "涛", "昌", "成", "康", "星", "翰", "晸", "时", "泰", "盛", "若", "鸣", "朋", "斌", "梁", "栋", "维", "启", "克", "伦", "翔", "旭", "鹏", "泽", "朗", "伯", "昮", "晋", "晟", "诚", "先", "敬", "震", "振", "壮", "会", "思", "群", "豪", "心", "邦", "承", "乐", "宏", "言", "旲", "旻", "昊", "光", "天", "达", "安", "岩", "中", "茂", "进", "林", "有", "坚", "和", "彪", "博", "泰", "盛", "振", "德", "行", "明", "永", "健", "世", "广", "志", "义", "兴", "良", "海", "山", "仁", "波", "宁", "行", "时", "志", "忠", "思", "绍", "功", "松", "善", "厚", "庆", "磊", "民", "友", "裕", "河", "哲", "江", "超", "炎", "德", "彰", "征", "律", "晨", "辰", "士", "以", "建", "家", "致", "煜", "煊", "炎", "波", "宁", "贵", "福", "生", "龙", "元", "全", "国", "胜", "学", "祥", "才", "发", "武", "新", "利", "清", "飞", "彬", "富", "顺", "信", "子", "杰", "涛", "昌", "成", "康", "星", "光", "天", "达", "安", "岩", "中", "茂", "进", "林", "有", "坚", "和", "彪", "博", "诚", "先", "敬", "震", "振", "壮", "会", "思", "群", "豪", "心", "邦", "承", "乐", "绍", "功", "松", "善", "厚", "庆", "磊", "民", "友", "裕", "河", "哲", "江", "超", "浩", "亮", "政", "谦", "亨", "奇", "固", "之", "轮", "翰", "朗", "伯", "宏", "言", "若", "鸣", "朋", "斌", "梁", "栋", "维", "启", "克", "伦", "翔", "旭", "鹏", "泽", "晨", "辰", "士", "以"],
            [OwnerSex.Female] = ["菡", "惠", "娆", "娴", "婧", "敏", "雪", "菁", "蕊", "娜", "蓓", "莲", "艳", "蔓", "怡", "莺", "香", "艺", "晴", "滟", "玫", "玥", "紫", "秋", "梅", "瑶", "慧", "蓉", "英", "曼", "静", "苹", "璐缦", "瑶旋", "歆云", "缨缦", "缨舒", "笑欣", "佩晶", "钰妙", "梵馨", "茜叶", "玥洋", "滢如", "媚蓝", "茗娴", "易艺", "佩琪", "忆睿", "诗婷", "安梵", "婉馨", "彤薇", "语思", "笑茗", "晴双", "琬玥", "姝悦", "菲采", "蓓梵", "洋雪", "兰怜", "馨若", "碧茗", "伊凌", "南岚", "南影", "聪静", "倩美", "桦洋", "熙元", "新凡", "雅婉", "双梵", "兰岚", "希慕", "蝶露", "绮希", "荷颖", "清玲", "静蕊", "兮素", "茜如", "琪媛", "瑾毓", "薇蓓", "静锦", "彩影", "歆睿", "瑶念", "伊馨", "艺馨", "慧乐", "晴蓝", "淇婵", "晓歆", "汐瑞", "羽珊", "媛姣", "菁婉", "凌美", "桦新", "灵静", "毓茜", "莺菡", "馨蓓", "若乐", "瑾露", "卿梦", "姝娇", "易滢", "涵勤", "笑瑾", "琳毓", "舒颖", "青菲", "初婉", "瑾文", "素薇", "媛雪", "凌旋", "念莉", "绮怡", "菁梦", "雅艺", "玥乐", "熙汐", "天尤", "月涛", "听寒", "姝妍", "玉妍", "雅萱", "向彤", "汐婷", "碧蓉", "菡煜", "飞阳", "瑢晨", "寒嵘", "依玲", "怜晴", "璇曦", "芸宁", "歆琳", "芬妮", "斯玉", "妍青", "依娜", "霞妍", "晓洁", "婧宁", "珊依", "琳淼", "素沁", "翰颖", "婧玟", "思妍", "茹婷", "雯琴", "桐欣", "莉娜", "洁莹", "桑佳", "桑宛", "彩红", "景文", "思洁", "平夏", "凌菱", "纹美", "花尽", "笑珊", "惠贞", "星绮", "艳锦", "琀新", "凤瑞", "荷青", "媖飘", "雯虞", "思慧", "香蓉", "莉羽", "忆辞", "灵槐", "夜兰", "婉瑶", "青慕", "钰嫣", "月灵", "卓婷", "姝媛", "蔡琳", "妍凌", "素姗", "曦秀", "依婷", "幽江", "桐宛", "卿蓉", "雨茹", "娟欣", "婧芸", "喧玉", "婻梓", "正蝶", "君云", "青旋", "诗璇", "偲筱", "涵颖", "黛沫", "岚银", "晗煊", "虹华", "雪丽", "琰珍", "思柳", "子琴", "泓凌", "溪芳", "入画", "丽梅", "青蝶", "书琴", "仪可", "丽语", "虞妮", "映妤", "霞梅", "嘉荷", "林菲", "洛菡", "以梦", "蔓雨", "馨誉", "雯瑄", "钧玥", "伊霏", "娜伊", "娅洁", "彩羽", "颐妙", "蓉婷", "瑾珺", "丽抒", "芊穗", "芯萍", "惟琳", "芊若", "美岩", "映雯", "柯滢", "美垚", "茜瑜", "芊芝", "于岚", "梓芳", "佩姿", "翊颖", "馨朵", "颖溪", "瑾泉", "采婕", "甜恬", "蕾霏", "雁兰", "宛柠", "淑羽", "湉心", "怡斐", "榆汐", "甜瑜", "夏妤", "懿娉", "熙婕", "钰茵", "童娜", "曦岚", "依伶", "靓玲", "岚忆", "依清", "芷函", "茜羽", "颖伊", "楠浠", "兮米", "洺瑄", "霈宸", "雅嫣", "凤蕾", "杭颖", "瑜萱", "睿薇", "玥欢", "兰兮", "菁宇", "洪芹", "宜珺", "媛晴", "子芬", "钰晴", "熙卉", "妤珺", "雁璐", "嫣妮", "沐萱", "淑苒", "馥蓉", "芸轩", "霈宁", "槿诺", "琼雅", "科莹", "轩媛", "俪霏", "夏依", "小杏", "芳雪", "奕娇", "蓉嫣", "蓓宁", "清璇", "丹宸", "宸汐", "景卉", "墨妤"]
        };

        private static readonly List<OwnerSex> ownerSexes = [OwnerSex.Male, OwnerSex.Female];

        private static Owner CreateRandomOwner(Room room, District town)
        {
            OwnerSex ownerSex = ownerSexes[Random.Shared.Next(0, ownerSexes.Count)];
            string name = names[Random.Shared.Next(0, names.Count)];
            int age = Random.Shared.Next(18, 100);
            return CreateOwner(room, name, ownerSex, age, town);
        }

        private static Owner CreateOwner(Room room, OwnerSex sex, int age, District town)
        {
            string name = names[Random.Shared.Next(0, names.Count)];
            return CreateOwner(room, name, sex, age, town);
        }

        private static Owner CreateOwner(Room room, string name, int age, District town)
        {
            OwnerSex sex = ownerSexes[Random.Shared.Next(0, ownerSexes.Count)];
            return CreateOwner(room, name, sex, age, town);
        }

        private static Owner CreateOwner(Room room, string name, OwnerSex sex, int age, District town)
        {
            Owner owner = new Owner
            {
                Id = ObjectId.GenerateNewId().ToString(),
                Code = room.Code,
                Name = name + maleWords[sex][Random.Shared.Next(0, maleWords[sex].Count)],
                RoomId = room.Id,
                Sex = sex.ToString().ToLower(),
                Age = age,
                IdNo = GetIdCode(town.Code, age),
                Tel = GetRandomTel(),
                Hometown = town.Name,
                Workplace = rdCityList[Random.Shared.Next(0, rdCityList.Count)],
                CreateTime = DateTime.Now,
                Version = 1
            };
            return owner;
        }

        private static readonly List<string> rdCityList = ["杭州市", "宁波市", "温州市", "嘉兴市", "湖州市", "绍兴市", "金华市", "衢州市", "舟山市", "台州市", "丽水市", "合肥市", "芜湖市", "蚌埠市", "淮南市", "淮北市", "铜陵市", "安庆市", "黄山市", "滁州市", "阜阳市", "宿州市", "六安市", "亳州市", "池州市", "宣城市", "福州市", "厦门市", "莆田市", "三明市", "泉州市", "漳州市", "南平市", "龙岩市", "宁德市", "南昌市", "萍乡市", "九江市", "新余市", "鹰潭市", "赣州市", "吉安市", "宜春市", "抚州市", "上饶市", "济南市", "青岛市", "淄博市", "枣庄市", "东营市", "烟台市", "潍坊市", "济宁市", "泰安市", "威海市", "日照市", "临沂市", "德州市", "聊城市", "滨州市", "菏泽市", "郑州市", "开封市", "洛阳市", "安阳市", "鹤壁市", "新乡市", "焦作市", "濮阳市", "许昌市", "漯河市", "南阳市", "商丘市", "信阳市", "周口市", "济源市", "武汉市", "黄石市", "十堰市", "宜昌市", "襄阳市", "鄂州市", "荆门市", "孝感市", "荆州市", "黄冈市", "咸宁市", "随州市", "仙桃市", "潜江市", "天门市", "长沙市", "株洲市", "湘潭市", "衡阳市", "邵阳市", "岳阳市", "常德市", "益阳市", "郴州市", "永州市", "怀化市", "娄底市", "广州市", "韶关市", "深圳市", "珠海市", "汕头市", "佛山市", "江门市", "湛江市", "茂名市", "肇庆市", "惠州市", "梅州市", "汕尾市", "河源市", "阳江市", "清远市", "东莞市", "中山市", "潮州市", "揭阳市", "云浮市", "南宁市", "柳州市", "桂林市", "梧州市", "北海市", "钦州市", "贵港市", "玉林市", "百色市", "贺州市", "河池市", "来宾市", "崇左市", "海口市", "三亚市", "三沙市", "儋州市", "琼海市", "文昌市", "万宁市", "东方市", "定安县", "屯昌县", "澄迈县", "临高县", "成都市", "自贡市", "泸州市", "德阳市", "绵阳市", "广元市", "遂宁市", "内江市", "乐山市", "南充市", "眉山市", "宜宾市", "广安市", "达州市", "雅安市", "巴中市", "资阳市", "贵阳市", "遵义市", "安顺市", "毕节市", "铜仁市", "昆明市", "曲靖市", "玉溪市", "保山市", "昭通市", "丽江市", "普洱市", "临沧市", "拉萨市", "昌都市", "林芝市", "山南市", "那曲市", "西安市", "铜川市", "宝鸡市", "咸阳市", "渭南市", "延安市", "汉中市", "榆林市", "安康市", "商洛市", "兰州市", "金昌市", "白银市", "天水市", "武威市", "张掖市", "平凉市", "酒泉市", "庆阳市", "定西市", "陇南市", "西宁市", "海东市", "银川市", "吴忠市", "固原市", "中卫市", "哈密市", "北屯市", "双河市", "昆玉市", "新星市"];

        private static string GetIdCode(string district, int age)
        {
            DateTime birthDay = DateTime.Now.AddYears(-1 * age);
            //PIN = District + Year(50-92) + Month(01-12) + Date(01-30) + Seq(001-600)
            string _pinCode = string.Format("{0}{1}{2:00}{3:00}{4:000}", district[..6], birthDay.Year, birthDay.Month, birthDay.Day, Random.Shared.Next(1, 600));

            char[] _chrPinCode = _pinCode.ToCharArray();
            //校验码字符值
            char[] _chrVerify = ['1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2'];
            //i----表示号码字符从由至左包括校验码在内的位置序号；
            //ai----表示第i位置上的号码字符值；
            //Wi----示第i位置上的加权因子，其数值依据公式intWeight=2（n-1）(mod 11)计算得出。
            int[] _intWeight = [7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2, 1];
            int _craboWeight = 0;
            for (int i = 0; i < 17; i++)//从1 到 17 位,18为要生成的验证码
            {
                _craboWeight = _craboWeight + System.Convert.ToUInt16(_chrPinCode[i].ToString()) * _intWeight[i];
            }
            _craboWeight = _craboWeight % 11;
            _pinCode += _chrVerify[_craboWeight];
            return _pinCode;
        }

        private static readonly string[] telStarts = "134,135,136,137,138,139,150,151,152,157,158,159,130,131,132,155,156,133,153,180,181,182,183,185,186,176,187,188,189,177,178".Split(',');

        /// <summary>
        /// 随机生成电话号码
        /// </summary>
        /// <returns></returns>
        private static string GetRandomTel()
        {
            int n = Random.Shared.Next(10, 1000);
            int index = Random.Shared.Next(0, telStarts.Length - 1);
            string first = telStarts[index];
            string second = (Random.Shared.Next(100, 888) + 10000).ToString().Substring(1);
            string thrid = (Random.Shared.Next(1, 9100) + 10000).ToString().Substring(1);
            return first + second + thrid;
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }

    public class Counter
    {
        public int Rooms { get; set; }

        public int Houses { get; set; }

        public int Populations { get; set; }

        public List<string> CommunityIds { get; set; } = new List<string>();
    }
}




