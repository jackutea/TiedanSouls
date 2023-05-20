using System;
using TiedanSouls.Generic;
using UnityEngine;

namespace TiedanSouls.Client.Entities {

    public class HUDSlotComponent {

        Transform hudRoot;
        public Transform HudRoot => hudRoot;

        HPBarHUD hpBarHUD;
        public HPBarHUD HPBarHUD => hpBarHUD;

        DamageFloatTextHUD damageFloatTextHUD;
        public DamageFloatTextHUD DamageFloatTextHUD => damageFloatTextHUD;

        public HUDSlotComponent() { }

        public void Inject(Transform hudRoot) {
            this.hudRoot = hudRoot;
        }

        public void Tick(RoleAttributeComponent attrCom, float dt){
            hpBarHUD.SetGP(attrCom.GP);
            hpBarHUD.SetHP(attrCom.HP);
            hpBarHUD.SetHPMax(attrCom.HPMax);
            hpBarHUD.Tick(dt);
        }

        public void SetHPBarHUD(HPBarHUD hpBarHUD) {
            this.hpBarHUD = hpBarHUD;
            hpBarHUD.transform.SetParent(hudRoot, false);
        }

        public void SetDamageFloatTextHUD(DamageFloatTextHUD damageFloatTextHUD) {
            this.damageFloatTextHUD = damageFloatTextHUD;
            damageFloatTextHUD.transform.SetParent(hudRoot, false);
        }

        public void Reset(float gp, float hp, float hpMax) {
            hpBarHUD.SetGP(gp);
            hpBarHUD.SetHP(hp);
            hpBarHUD.SetHPMax(hpMax);
        }

        public void HideHUD() {
            hpBarHUD.gameObject.SetActive(false);
        }

        public void ShowHUD() {
            hpBarHUD.gameObject.SetActive(true);
        }

    }

}