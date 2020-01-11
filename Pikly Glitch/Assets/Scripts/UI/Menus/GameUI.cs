using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Pikl.Extensions;
using TeamUtility.IO;
using Pikl.Profile;
using Pikl.Data;
using System.Linq;

namespace Pikl.UI {
    public class GameUI : Menu {
        public Player.Player player;

        public GameObject craftingUI, inventoryCraftHighlights, craftingSelections, debugPanel;
        public RecipeBook recipeBook;
        public Text health, armour, stamina, terminalText, debugValues;

        public Image inventory, terminalDisplay;
        public Text[] inventoryItems = new Text[20];
        public Text[] craftingItems = new Text[20];
        public Color weaponColour, consumableColour, materialColour, toolColour;
        public Text ammo, description;

        public bool debug;

        Color whiteAlpha = new Color(1, 1, 1, 0);

        string emptyItemString = "-";

        int selectedCraft = 0;

        List<Recipe> availableCrafts = new List<Recipe>();

        void Start() {
            //MessageMgr.I.AddListener<string>("EnterScene", OnEnterScene);
            //MessageMgr.I.AddListener("BossAppear", OnBossAppear);
        }

        //void OnEnterScene(string scene) {
        //}

        public override void Open() {
            if (InputMgr.PlayerOneConfiguration.name == InputAdapter.JoystickConfiguration)
                Cursor.visible = false;

            //Cursor.SetCursor(crosshairCursor, new Vector2(crosshairCursor.width / 2, crosshairCursor.height / 2), CursorMode.Auto);

            StartCoroutine(GetPlayer());
            //StartCoroutine(UpdateLevelProgress());

            //UIMgr.I.PauseFilterOff();

            Debug.Log("GAME UI Open");

            terminalDisplay.enabled = false;
            terminalText.enabled = false;

            CreateInventory();

#if UNITY_EDITOR
            debug = true;
#endif 

            base.Open();
        }

        public override void Close() {
            player = null;
            //Cursor.SetCursor(mouseCursor, Vector2.zero, CursorMode.Auto);
            base.Close();
        }

        bool isAlreadyDown = false;
        public override void OnUpdate() {
            if (player != null) {
                UpdateHealth();
                UpdateArmour();
                UpdateStamina();
                //UpdateVelocity();
                //UpdateState();
                UpdateInventory();
#if UNITY_EDITOR
                if (debug) {
                    debugPanel.SetActive(true);
                    UpdateDebugValues();

                } else {
                    debugPanel.SetActive(false);
                }
#endif

                if (craftingUI.activeSelf) {
                    ClearCraftingList();
                    if (CheckAvailableCrafts()) {
                        UpdateCraftingSelection();
                        if (!isAlreadyDown && InputMgr.GetAxisRaw("Shoot") != 0) {
                            isAlreadyDown = true;
                            availableCrafts[selectedCraft].Craft();
                        } else if (InputMgr.GetAxisRaw("Shoot") == 0) {
                            isAlreadyDown = false;
                        }
                    }
                }
            }

            base.OnUpdate();
        }

        IEnumerator GetPlayer() // In case the player isn't immediately available.
        {
            do {
                //player = Ref.I["PlayerScript"] as Player.Player;
                player = GameObject.Find("Player").GetComponent<Player.Player>();

                yield return new WaitForEndOfFrame();
            } while (player == null);
        }

        void CreateInventory() {
            for(int i = 0; i < player.inventory.size; i++) {
            }
        }

        public void DisplayTerminal(Terminal t) {
            if (terminalDisplay.enabled) {
                terminalDisplay.enabled = false;
                terminalText.enabled = false;
            } else {
                terminalText.text = t.name + ": " + t.text;
                terminalDisplay.enabled = true;
                terminalText.enabled = true;
            }
        }

        public void CloseTerminal() {
            terminalDisplay.enabled = false;
            terminalText.enabled = false;
        }

        public void ToggleCraftingUI() {
            if (craftingUI.activeSelf) {
                craftingUI.SetActive(false);
            } else {
                craftingUI.SetActive(true);
                selectedCraft = 0;
            }
            
            player.input.lastCraftToggle = Time.time;
        }

        void ClearCraftingList() {
            foreach (Text t in craftingItems)
                t.text = emptyItemString;
        }

        bool CheckAvailableCrafts() {
            availableCrafts = new List<Recipe>();
            //Find the craftables
            foreach(Recipe recipe in recipeBook.recipes) {
                if (recipe.Craftable()) {
                    availableCrafts.Add(recipe);
                }
            }

            //Display the craftables in ui
            if (availableCrafts.Count > 0) {
                for (int i = 0; i < availableCrafts.Count; i++) {
                    craftingItems[i].text = availableCrafts[i].item.name;
                    for (int j = 0; j < availableCrafts.Count; j++) {
                        inventoryCraftHighlights.transform.GetChild(j).gameObject.SetActive(availableCrafts[i].item.name == player.inventory.items[j].name);
                    }
                }
                return true;
            }

            return false;

            //bool atLeastOneCraftable = false;
            //List<Item> availableCrafts = new List<Item>();

            //foreach (Recipe r in recipeBook.recipes) {
            //    bool hasAllIngredients = false;

            //    foreach (Ingredient ingredient in r.ingredients) {
            //        IEnumerable<Item> matchingItems = player.inventory.items.Where(e => e.name == ingredient.item.name);
            //        int tally = 0;

            //        foreach(Item i in matchingItems)
            //            tally += i.quantity;

            //        hasAllIngredients = tally >= ingredient.quantity;

            //        if (!hasAllIngredients) break;
            //    }

            //    if (hasAllIngredients) {
            //        availableCrafts.Add(r.item);
            //        atLeastOneCraftable = true;
            //    }
            //}

            //if (availableCrafts.Count > 0) {
            //    for(int i = 0; i < availableCrafts.Count; i++) {
            //        craftingItems[i].text = availableCrafts[i].name;
            //        for(int j = 0; j < 20; j++) {
            //            inventoryCraftHighlights.transform.GetChild(j).gameObject.SetActive(availableCrafts[i].name == player.inventory.items[j].name);
            //        }

            //    }
            //}

            //return atLeastOneCraftable;
        }

        void UpdateCraftingSelection() {
            float selectionDir = InputMgr.GetAxisRaw("Mouse Scrollwheel");

            selectedCraft -= (int)selectionDir;
            selectedCraft = Mathf.Clamp(selectedCraft, 0, 19);

            for(int i = 0; i < craftingItems.Length; i++) {
                GameObject o = craftingSelections.transform.GetChild(i).gameObject;
                o.SetActive(o.name == string.Format("Item {0} Outline", selectedCraft + 1));
            }

            //for (int i = 0; i < craftingItems.Length; i++) {
            //    GameObject o = craftingSelections.transform.GetChild(i).gameObject;
            //    if (o.name == string.Format("Item {0} Outline", selectedCraft + 1)) {
            //        o.SetActive(true);
            //        //get recipe for selected
            //        //top down, highlight items in the recipe, checking quantities

            //        int j = 0;
            //        foreach (Item item in player.inventory.items) {
            //            foreach(Recipe r in recipeBook.recipes) {
            //                int quantityTally = 0;
            //                foreach(Ingredient ing in r.ingredients) {
            //                    quantityTally += item.quantity;
            //                    if (item.name == ing.item.name) {
            //                        inventoryCraftHighlights.transform.GetChild(j).gameObject.SetActive(true);
            //                        if (quantityTally >= ing.quantity)
            //                            break;
            //                    }
            //                }
            //            }
            //            j++;
            //        }

            //    } else {
            //        o.SetActive(false);
            //    }
            //}
        }

        void UpdateHealth() {
            health.text = player.health.HP.ToString();
        }

        void UpdateArmour() {
            armour.text = player.health.Armour.ToString();
        }

        void UpdateStamina() {
            stamina.text = string.Concat(player.evade.Stamina, "/", player.evade.MaxStamina);

            stamina.color = player.evade.Stamina < player.evade.EvadeCost ? Color.red : Color.white;
        }

        void UpdateDebugValues() {
            debugValues.text = string.Empty;

            debugValues.text += string.Format("{0:f1}, {1:f1}", player.rb.velocity.x, player.rb.velocity.y);
            debugValues.text += System.Environment.NewLine;
            debugValues.text += player.CurrentState.ToString().Split('.').Last();
            debugValues.text += System.Environment.NewLine;

            foreach (var item in player.asyncStates) { 
                debugValues.text += item.Value.ToString().Split('.').Last() + " ";
            }
        }


        void UpdateInventory() {
            int i = 0;
            foreach (Item item in player.inventory.items) {
                switch(item.type) {
                    case ItemType.Throwable:
                    case ItemType.Weapon: inventoryItems[i].color = item.selected ? weaponColour.Darker() : weaponColour; break;

                    case ItemType.Consumable: inventoryItems[i].color = item.selected ? consumableColour.Darker() : consumableColour; break;

                    case ItemType.Tool: inventoryItems[i].color = item.selected ? toolColour.Darker() : toolColour; break;

                    case ItemType.Material: inventoryItems[i].color = item.selected ? materialColour.Darker() : materialColour; break;

                    default: inventoryItems[i].color = item.selected ? Color.white.Darker() : Color.white; break;
                }

                inventoryItems[i].text = ParseItem(item);
                i++;
            }

            switch(player.inventory.SelectedType) {
                case ItemType.Weapon:
                    Weapon weapon = player.inventory.SelectedItem as Weapon;
                    if (weapon.reloading) {
                        ammo.text = ((int)(Mathf.InverseLerp(weapon.lastReloadTime, weapon.lastReloadTime + weapon.reloadSpeed, Time.time) * 100)).ToString() + " %";
                    } else {
                        ammo.text = string.Concat(weapon.clipAmmo, '/', weapon.totalAmmo);
                    }

                    ammo.color = weapon.clipAmmo == 0 || weapon.reloading ? Color.red : Color.white;
                    break;
                default:
                    ammo.text = "- / -";
                    ammo.color = Color.white;
                    break;
            }

            description.text = player.inventory.SelectedItem.description;
        }

        string ParseItem(Item i) {
            return string.Concat(i.selected ? "  " : string.Empty, emptyItemString, i.name, i.quantity > 1 ? string.Concat(' ', 'x', i.quantity) : string.Empty);
        }
    }
}