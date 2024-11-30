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
    public class CommunityController : ControllerBase
    {
        [HttpPost("init")]
        public async Task<bool> Init([FromQuery] string cityId)
        {
            using IDbConnection connection = CreateConnection();
            District city = connection.Query<District>().Where(f => f.Id == cityId).FirstOrDefault<District>();
            IList<District> districtList = connection.Query<District>().Where(f => f.Code.StartsWith(city.Code)).ToList<District>();
            IList<HousePlanning> housePlannings = connection.Query<HousePlanning>().Where(f => f.CityId == cityId).ToList<HousePlanning>();
            IList<Community> communityList = Convert(housePlannings, city, districtList).ToList();
            await connection.InsertBulkAsync(communityList);
            return true;
        }

        private static IEnumerable<Community> Convert(IList<HousePlanning> housePlannings, District city, IList<District> districtList)
        {
            foreach (HousePlanning housePlanning in housePlannings.Where(f => f.Level == DistrictLevel.Town))
            {
                int totalHouses = 0;
                while (housePlanning.Houses > totalHouses)
                {
                    District town = districtList.First(f => f.Id == housePlanning.DistrictId);
                    int communityHouseCount = Random.Shared.Next(6, 15);
                    Community community = new Community
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        DistrictId = housePlanning.DistrictId,
                        CityId = housePlanning.CityId,
                        Name = GetRandomName(),
                        Location = $"{city.Name}{town.Name}{GetRandomStreet()}",
                        Version = 1,
                        Houses = communityHouseCount,
                        Rooms = housePlanning.Rooms,
                        Floors = housePlanning.Floors,
                        HouseType = housePlanning.HouseType
                    };
                    yield return community;
                    totalHouses += communityHouseCount;
                }
            }
        }

        private static string GetRandomStreet()
        {
            string nameString = "栖霞路|琴台路|抚琴路|春熙路|桃蹊路|逸都路|望江路|桃蹊路|逸都路|玉林路|芷泉街|芳沁街|督院街|方池街|黉门街|林荫街|梨花街|柳荫街|九思巷|科甲巷|桓侯巷|石室巷|红墙巷|人民大道|科技路|教育路|文化路|体育路|中山路|鲁迅路|李白路|杜甫路|孔子路|镗钯街|天仙桥|送仙桥|合江亭|梧桐街|杏花街|盐市口|乌衣巷|草鞋巷|五块石|九里堤|八里桥|十里店|高笋塘|高升塘|小菜园|荷花池|迎仙桥|水津街|烟台道|中山道|朱雀大街|太乙路|太白路|太华路|长乐坊|长樱路|案芦隐板街|竹笆市|骡马市|东木头市|西木头市|安仁坊|端履门|德福巷|洒金桥|冰窖巷|菊花园|下马陵|粉巷|索罗巷|后宰门|书院门|炭市街|马厂子|景龙池|甜水井|柏树林|桃花坞大街|山塘街|锦帆路|苏公路|十梓街|十全街|景德路|干将路|莫邪路|平遥南大街|松涛路|琴台路|梧桐街|乌衣巷";
            string[] names = nameString.Split('|');
            return $"{names[Random.Shared.Next(0, names.Length)]}{Random.Shared.Next(1, 999)}号";
        }

        private static string GetRandomName()
        {
            string prefix = "美然|百度|达富|宝润|群芳|龙腾|西富|正阳|华龙|花木|渤海|锦灏|映山|香葶|暖馨|枫林|鹂湖|莲花|御墅|临枫|西岸|御水|韦伯|水郡|尚|理想|原生|龙湖|香樟|西山|林语|禧福|花语|孔雀|华西|美庐|圣安卓|天宝|世纪|金鼎|含辉|四宜|碧桐花|法源|清旷|储水|烟雨|见山|倒影|环翠|若帆|清音|冷香|远翠|浮翠|留听|白梅|修竹|濯缨|凌虚|中和|集福|蔚藻|清夏|畅和|沉心|慎德|澹怀|含经|泽兰|兰雪|远香|绣绮|立雪|心远|绿满|多嫁|深晨|君子|三支|闻木樨|揖峰|听雨|小山丛|竹外|虹|石板|绿荫|枕流|青枫|凌波|九孔|颐波|迎客|小飞|引静|涵芳|烟霞|落虹|文庭，美景|田园|牧歌|水木|顶秀|青溪|中景|枫蓝|顺驰|蓝调|阳光|荣尊|达龙|国典|枫桥|京艺|天朗|阳光|上东|天和|蓝山|宝源|复兴|万泉|碧水|云天|新康|恒华|美林|水语|鑫兆|夏都|富泉|朝阳|博雅|永泰|新风|翡翠|燕沙|丽港|博士| 东领|定福|远洋|汤泉|珠江|朗琴|卡布其诺|万豪|纳帕|百旺|庄胜|金隅|莱茵河|亮马|万和|金地|双惠|嘉和|麒麟|幻星|光彩| 怡禾|梧桐|晶都|昆泰|观澜|倚林|峻峰|石榴|世桥|顺驰|锋尚";
            string suffix = "小镇|里|园|府|阁|豪庭|湾|亭|岛|邸|院|庄|世家|国际|城|馆|山庄|枫林|森林|云林|半山|山房|山庄|庄园|果岭|凤岭|枫岭|绿茵|绿景|碧园|绿都|麓府|谷|树|林|岭|山|峰|堡|庄|园|河岸|蓝岸|江岸|海岸|蓝湾|丽湾|龙湾|江湾|半岛|绿岛|湖景|海景|江景|湖畔|江畔|源|岛|泉|江|湖|海|滩|湾|堤|岸|丽舍|雅苑|嘉园|怡园|雅园|寓言|精英|学府|心语|心曲|海派|经典|丽庭|小街|小镇|尚品|美地|春景|春色|春天|丽景|阳光|时光|时代|年代|岁月|空间|人间|人家|华彩|乐章|里|苑|斋|坊|居|巢|筑|宿|景|中心|广场|大厦|公寓|小区|社区|新区|新村|大院|大楼|公馆|公园|花园|家园|集团|厦|楼|院";
            string[] prefixNames = prefix.Split("|");
            string[] suffixNames = suffix.Split("|");
            return $"{prefixNames[Random.Shared.Next(0, prefixNames.Length)]}{suffixNames[Random.Shared.Next(0, suffixNames.Length)]}";
        }

        private static IDbConnection CreateConnection()
        {
            IDbConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;database=big_data_tutorial;uid=root;pwd=Q1@we34r;charset=utf8");
            return connection;
        }
    }
}

