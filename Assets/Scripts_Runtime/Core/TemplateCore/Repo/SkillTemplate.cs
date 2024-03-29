using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using TiedanSouls.Generic;

namespace TiedanSouls.Template {

    public class SkillTemplate {

        Dictionary<int, SkillTM> all;

        public SkillTemplate() {
            all = new Dictionary<int, SkillTM>();
        }

        public async Task LoadAll() {
            AssetLabelReference label = new AssetLabelReference();
            label.labelString = AssetLabelCollection.SO_SKILL;
            var list = await Addressables.LoadAssetsAsync<SkillSO>(label, null).Task;
            foreach (var item in list) {
                var tm = item.tm;
                all.Add(tm.typeID, tm);
            }
        }

        public bool TryGet(int typeID, out SkillTM tm) {
            if (!all.TryGetValue(typeID, out tm)) {
                return false;
            }

            return true;
        }

    }

}