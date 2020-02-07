using System;
using Pikl;
using System.Collections;
using System.Collections.Generic;
using Pikl.Enemies;
using Pikl.States.Components;
using Pikl.UI;
using UnityEngine;

namespace Pikl.Data {
    public enum ToolEffect { Shockblade, Magshield, Detector, EMDetector, Teleport }
    
    [System.Serializable]
    [CreateAssetMenu(fileName = "New Tool", menuName = "Data/Items/New Tool")]
    public class Tool : Item {

        public ToolEffect effect;
        bool magshield;
        
        internal void Init(string name, Sprite sprite, string description, ItemType type, int quantity, int maxStack, ToolEffect effect) {
            Init(name, sprite, description, type, quantity, maxStack);
            this.effect = effect;
        }

        
        public override void Use() {
            if (effect == ToolEffect.Shockblade)
                Player.Player.I.knife.hasShockblade = true;
            else return;
            
            base.Use();
        }

        public void Update() {
            switch (effect) {
                case ToolEffect.Magshield:
                    if (Player.Player.I.input.AimAxis) {
                        if (!magshield)
                            ShowMagshield();
                    } else if (magshield) {
                        HideMagshield();
                    }
                    break;
                case ToolEffect.Detector: 
                    Detect("Trap");
                    break;
                case ToolEffect.EMDetector:
                    Detect("Trap");
                    Detect("Enemy"); 
                    break;
            }
        }

        public override void Drop() {
            if (effect == ToolEffect.Detector || effect == ToolEffect.EMDetector)
                ItemLabelMgr.I.HideAllMarkers();
            base.Drop();
        }

        void ShowMagshield() {
            magshield = true;
            Player.Player.I.powerup.magshield.SetActive(true);
        }

        void HideMagshield() {
            magshield = false;
            Player.Player.I.powerup.magshield.SetActive(false);
        }

        void ReflectBullets() {
            //TODO
        }
        
        void Detect(string layer) {
            RaycastHit2D[] hits = Physics2D.CircleCastAll(Player.Player.I.t.position, Player.Player.I.powerup.detectorRadius * 1.5f, 
                Vector2.zero, 0, 1 << LayerMask.NameToLayer(layer));
            
            if (hits.Length <= 0) return;
            
            foreach (RaycastHit2D hit in hits) {
                if (layer == "Trap") {
                    if (hit.distance < Player.Player.I.powerup.detectorRadius && !hit.transform.GetComponent<Trap>().triggered)
                        ItemLabelMgr.I.ShowTrapMarker(hit.transform);
                    else 
                        ItemLabelMgr.I.HideTrapMarker(hit.transform);
                } else if (layer == "Enemy") {
                    if (hit.distance < Player.Player.I.powerup.detectorRadius)
                        ItemLabelMgr.I.ShowEnemyMarker(hit.transform);
                    else
                        ItemLabelMgr.I.HideEnemyMarker(hit.transform);
                }
            }
        }

        public static Tool CreateInstance(Tool Tool) {
            Tool data = CreateInstance<Tool>();
            data.Init(Tool.name, Tool.sprite, Tool.description, Tool.type, Tool.quantity, Tool.maxStack, Tool.effect);
            return data;
        }
    }
}