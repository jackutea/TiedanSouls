using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using TiedanSouls.Generic;

namespace TiedanSouls.Template {

    public class TemplateCore {

        RoleTemplate roleTemplate;
        public RoleTemplate RoleTemplate => roleTemplate;

        SkillorTemplate skillorTemplate;
        public SkillorTemplate SkillorTemplate => skillorTemplate;

        WeaponTemplate weaponTemplate;
        public WeaponTemplate WeaponTemplate => weaponTemplate;

        FieldTemplate fieldTemplate;
        public FieldTemplate FieldTemplate => fieldTemplate;

        ItemTemplate itemTemplate;
        public ItemTemplate ItemTemplate => itemTemplate;

        AITemplate aiTemplate;
        public AITemplate AITemplate => aiTemplate;

        GameConfigTM gameConfigTM;
        public GameConfigTM GameConfigTM => gameConfigTM;

        public TemplateCore() {
            roleTemplate = new RoleTemplate();
            skillorTemplate = new SkillorTemplate();
            weaponTemplate = new WeaponTemplate();
            fieldTemplate = new FieldTemplate();
            itemTemplate = new ItemTemplate();
            aiTemplate = new AITemplate();
        }

        public async Task Init() {
            await roleTemplate.LoadAll();
            await skillorTemplate.LoadAll();
            await weaponTemplate.LoadAll();
            await fieldTemplate.LoadAll();
            await itemTemplate.LoadAll();
            await aiTemplate.LoadAll();

            // GameConfig
            AssetLabelReference label = new AssetLabelReference();
            label.labelString = AssetsLabelCollection.SO_CONFIG;
            var gameConfigSO = await Addressables.LoadAssetAsync<GameConfigSO>(label).Task;
            this.gameConfigTM = gameConfigSO.tm;
        }

    }

}